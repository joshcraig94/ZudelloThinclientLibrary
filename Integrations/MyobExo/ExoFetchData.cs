using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ZudelloApi;
using ZudelloThinClientLibary;


namespace MyobExoConnector.EXO
{
    public class MyQueueObject
    {
        public object data { get; set; }

    }


    public class ExoFetchData
    {

        public static Dictionary<int, Zmapping> getBody()
        {


            Dictionary<int, Zmapping> myMappings = new Dictionary<int, Zmapping>();
            using (var db = new ZudelloContext())
            {
                int i = 0;

                var MapID = db.Zmapping.Where(y => y.IsOutgoing == 1).ToList();

                foreach (var mappings in MapID.OrderBy(y => y.ProcessOrder).ThenByDescending(l => l.connection_id))
                {
                    Zmapping Data = new Zmapping()
                    {
                        DocType = mappings.DocType,
                        Body = mappings.Body,
                        Section = mappings.Section,
                        database = mappings.database,
                        Id = mappings.Id,
                        connection_id = mappings.connection_id

                    };


                    myMappings.Add(i, Data);
                    i++;
                }
                db.DisposeAsync();
                return myMappings;
            }

        }





        public static async Task PushExoDataToZudello()
        {

            string Token = ZudelloLogin.Login();



            SQLCredentials ConnectionString = new SQLCredentials();
            Dictionary<int, string> Connection = ConnectionString.ConnectionStringBuilder();


            /**** 
             * 
             * Add threading into here for the foreach loop
             * will need to chunk based on amount of connections    
             */

            //get the query from mappings table
            Dictionary<int, Zmapping> MappingsBody = getBody();
            int numberOfLogicalCores = Environment.ProcessorCount;
            List<Thread> threads = new List<Thread>(numberOfLogicalCores);
            int sizeOfOneChunk = (MappingsBody.Count / numberOfLogicalCores) + 1;
            ConcurrentBag<string> cb = new ConcurrentBag<string>();
            ConcurrentBag<int> cbMaps = new ConcurrentBag<int>();
            for (int i = 0; i < numberOfLogicalCores; i++)
            {

                int ab = i;
                var thr = new Thread(

                () =>
                {
                    try
                    {
                        int count = 0;
                        Dictionary<int, Zmapping> MappingsChunked = MappingsBody.Skip(ab * sizeOfOneChunk)
                            .Take(sizeOfOneChunk).ToDictionary(p => p.Key, p => p.Value);


                        foreach (var Mappings in MappingsChunked)
                        {

                            bool isXMl = true;
                            //Get the SQL statement
                            string mySql = Mappings.Value.Body;
                            //Get the Mappings 
                            dynamic zudelloObject = Mappings.Value;                          

                            
                            string SQLQuery = ExoTools.RenderToSql(mySql, zudelloObject);

#warning make this in config also for process method
                            

                            string cmd = SQLQuery;

                            //Open SQL connection and run the SQL query 


                            //Get connection details by Key ID
                            SqlConnection con = new SqlConnection(Connection[Mappings.Value.connection_id]);
                            SqlCommand SelectCommand = new SqlCommand(cmd, con);
                            // SqlDataReader myreader;
                            con.Open();

                            if (cmd.ToLower().Contains("for xml"))
                                isXMl = true;

                            if (String.IsNullOrEmpty(cmd)) continue;
                            var jsonResult = new StringBuilder();
                            var xmlConvert = new StringBuilder();
                            //SelectCommand.CommandText = cmd;
                            var myreader = SelectCommand.ExecuteReader();

                            if (!myreader.HasRows)
                            {
                                //if there is no data then close connection and next loop
                                con.Close();
                                con.Dispose();
                                continue;
                            }
                            else
                            {
                                while (myreader.Read())
                                {



                                    jsonResult.Append(myreader.GetValue(0).ToString());


                                }

                                //Console.WriteLine(jsonResult);  

                                dynamic obj = "";
                                string ConvertedXml = "";
                                if (isXMl == true)
                                {
                                    ConvertedXml = ExoTools.ZudelloXMLConverter(jsonResult.ToString());

                                    //  string ConvertedXml = ExoTools.ZudelloXMLConverter(jsonResult.ToString());
                                    obj = JsonConvert.DeserializeObject<ExpandoObject>(ConvertedXml);

                                }
                                else
                                {
                                    obj = JsonConvert.DeserializeObject<ExpandoObject>(jsonResult.ToString());
                                }
                                foreach (var ObjectType in obj)

                                {
                                    //not sure why .Value is causing error but not affecting build
                                    foreach (var dataValues in ObjectType.Value)
                                    {
                                        string data = "";
                                        if (isXMl == true)
                                        {
                                            data = JsonConvert.SerializeObject(dataValues);


                                        }
                                        else
                                        {
                                            MyQueueObject dataWrapper = new MyQueueObject();
                                            dataWrapper.data = dataValues;
                                            data = JsonConvert.SerializeObject(dataWrapper);
                                        }

                                        // add to concurrent bag
                                        cb.Add(data);
                                        count++;
                                        //  Console.WriteLine(data);


                                    }
                                }

                                cbMaps.Add(Mappings.Value.Id);



                            }


                            con.Close();
                            con.Dispose();
                        }
                    }
                    catch (Exception ex)
                    { 
                    
                    }
                }
                );

                threads.Add(thr);
            }


            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
                
            }

            threads.Clear();


            int sizeOfOneChunkQueue = (cb.Count / numberOfLogicalCores) + 1;
            for (int x = 0; x < numberOfLogicalCores; x++)
            {

                int abx = x;
                var SendToQueuethr = new Thread(

                () =>
                {

                    try
                    {
                        
                        int count = 0;
                        List<string> queuedBag = cb.Skip(abx * sizeOfOneChunkQueue)
                            .Take(sizeOfOneChunkQueue).ToList();


                        foreach (var Mappings in queuedBag)
                        {
                                            
                           Console.WriteLine(ZudelloLogin.SendToZudelloQueue(Token, Mappings));
                        }
                    }
                    catch (Exception ex)
                    { 
                    
                    
                    }
                    });
                threads.Add(SendToQueuethr);
            }

            foreach (var qthread in threads)
            {
                qthread.Start();
            }

            foreach (var qthread in threads)
            {
                qthread.Join();

            }



            /*
                            List<Task> bagConsumeTasks = new List<Task>();
                        int itemsInBag = 0;
                        while (!cb.IsEmpty)
                        {
                            bagConsumeTasks.Add(Task.Run(() =>
                            {                 
                                string item;
                                if (cb.TryTake(out item))
                                {

                                    Console.WriteLine(ZudelloLogin.SendToZudelloQueue(Token, item));
                                    itemsInBag++;
                                }
                            }));
                        } */

            //   Task.WaitAll(bagConsumeTasks.ToArray());

            foreach (int id in cbMaps)
            { 
            using (var db = new ZudelloContext())
            {
                var lastSync = db.Zlastsync.Where(s => s.MappingId == id).FirstOrDefault();

                lastSync.LastSync = DateTime.Now.ToString();
                
                db.SaveChanges();
                db.DisposeAsync();
            }

                  
            }                
        }
    }
}
          

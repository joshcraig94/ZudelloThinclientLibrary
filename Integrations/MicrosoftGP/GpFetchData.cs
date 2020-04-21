using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ZudelloApi;
using ZudelloThinClientLibary;

namespace MicrosoftGPConnector
{
    public class MyQueueObject
    {
        public object data { get; set; }

    }


    public class GpFetchData
    {
        
        public static Dictionary<int, Zmapping> getBody()
        {
            

            Dictionary<int, Zmapping> myMappings = new Dictionary<int, Zmapping>();
            using (var db = new ZudelloContext())
            {

                int i = 0;
                var MapID = db.Zmapping.Where(c => c.IsOutgoing == 1).ToList();

                foreach (var mappings in MapID.OrderBy(c => c.ProcessOrder).ThenByDescending(l => l.connection_id))
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

                return myMappings;
            }

        }





        public static void PushGpDataToZudello()
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
            foreach (var Mappings in getBody())
            {

                //bool isXml = false;
                //Get the SQL statement
                string mySql = Mappings.Value.Body;
                //Get the Mappings 
                dynamic zudelloObject = Mappings.Value;

                string SQLQuery = GpTools.RenderToSql(mySql, zudelloObject);

               //if (mySql.Contains("XML PATH")) isXml = true;
               #warning make this in config also for process method


                string cmd = SQLQuery;

                //Open SQL connection and run the SQL query 


                //Get connection details by Key ID
                SqlConnection con = new SqlConnection(Connection[Mappings.Value.connection_id]);
                SqlCommand SelectCommand = new SqlCommand(cmd, con);
                // SqlDataReader myreader;
                con.Open();

                var jsonResult = new StringBuilder();

                var myreader = SelectCommand.ExecuteReader();

                if (!myreader.HasRows)
                {
                    //if there is no data then close connection and next loop
                    con.Close();
                    continue;
                }
                else
                {
                    while (myreader.Read())
                    {
                        jsonResult.Append(myreader.GetValue(0).ToString());

                    }
                    dynamic obj;
                    //Console.WriteLine(jsonResult);  
                  
                     obj = JsonConvert.DeserializeObject<ExpandoObject>(jsonResult.ToString());
                    
                    foreach (var ObjectType in obj)

                    {
                        foreach (var dataValues in ObjectType.Value)
                        {

                            MyQueueObject dataWrapper = new MyQueueObject();

                            dataWrapper.data = dataValues;
                         
                            string data = JsonConvert.SerializeObject(dataWrapper);
                            Console.WriteLine(data);
                            Console.WriteLine(ZudelloLogin.SendToZudelloQueue(Token, data));

                        }
                    }




                    //Update last sync time for this mapping ID 
                    using (var db = new ZudelloContext())
                    {
                        var lastSync = db.Zlastsync.Where(s => s.MappingId == Mappings.Value.Id).FirstOrDefault();

                        lastSync.LastSync = DateTime.Now.ToString();
                        db.SaveChanges();
                    }

                }
                con.Close();

            }

        }
    }
}

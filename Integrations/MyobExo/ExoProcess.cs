using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using System.Linq;
using ZudelloApi;
using System.Dynamic;
using ZudelloThinClientLibary;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MyobExoConnector.EXO
{


    public class QueueProcess
    {
      public  Zmapping map { get; set; } = new Zmapping();
      public Zqueue queue { get; set; } = new Zqueue();
    }

    public class ProccessResponse
    {
        public string SyncHistryUuid { get; set; } = null;
        public string Team { get; set;}
        public bool Successful { get; set; }
        public string Information { get; set; }
    }
    public  class ExoProcess
    {

        public static async Task ProcessRecords()
        {

            int? MasterDataAwaiting = null;
            List<QueueProcess> queueToProcess = new List<QueueProcess>();
            using (var db = new ZudelloContext())
            {




                var listQueued = (from queue in db.Zqueue
                                  join map in db.Zmapping on queue.MappingId equals map.Id
                                  where map.IsMasterData == 1 && queue.Status.Trim() == "Waiting" //Will make more generic later
                                  select new
                                  {
                                      queue,
                                      map
                                  }).ToList();




                foreach (var data in listQueued)
                {
                    QueueProcess m = new QueueProcess();
                    m.map = data.map;
                    m.queue = data.queue;
                    queueToProcess.Add(m);
                }



                db.DisposeAsync();
            }



            int numberOfLogicalCores = Environment.ProcessorCount;
            List<Thread> threads = new List<Thread>(numberOfLogicalCores);
            int sizeOfOneChunk = (queueToProcess.Count / numberOfLogicalCores) + 1;
            ConcurrentBag<string> cb = new ConcurrentBag<string>();
            ConcurrentDictionary<int, ProccessResponse> successDictionary = new ConcurrentDictionary<int, ProccessResponse>();
            for (int i = 0; i < numberOfLogicalCores; i++)
            {
                int ab = i;
                var thr = new Thread(

                () =>
                {
                    int count = 0;
                    List<QueueProcess> chunkedMaster = queueToProcess.Skip(ab * sizeOfOneChunk)
                        .Take(sizeOfOneChunk).ToList();
                    foreach (var queueData in chunkedMaster)
                    {
                        ProccessResponse success = new ProccessResponse();
                        success = ProcessMethod(queueData);
                        successDictionary.TryAdd(queueData.queue.Id, success);

                    }
                });
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
            //clear threads list 
            threads.Clear();
          
            RunSuccessUpdate(successDictionary);
            //clear Success Dictionary
            successDictionary.Clear();

            //Get transactional data to process
            //Clear Queue List 
            //  queueToProcess.Clear();

            List<QueueProcess> tQueueToProcess = new List<QueueProcess>();
            using (var db = new ZudelloContext())
            {
                
                var filterQueue = db.Zqueue.Where(x => x.Queue_Id != null
                                                 && x.Status == "Failed").Select(x => x.Queue_Id).ToArray();


                var listQueued = (from queue in db.Zqueue
                                  join map in db.Zmapping on queue.MappingId equals map.Id
                                  where map.IsMasterData != 1
                                  && queue.Status.Trim() == "Waiting"
                                  && !filterQueue.Contains(queue.Id)
                                  select new
                                  {
                                      queue,
                                      map
                                  }).ToList();


                foreach (var data in listQueued)
                {
                    QueueProcess m = new QueueProcess();
                    m.map = data.map;
                    m.queue = data.queue;
                    tQueueToProcess.Add(m);
                }


            }

            //Clear concurrent Dictionary
            //Transactional Data

            /*
             * 
             * 
             * 
             */

            //Reset sizeOf the Chunk
            sizeOfOneChunk = (tQueueToProcess.Count / numberOfLogicalCores) + 1;

            for (int x = 0; x < numberOfLogicalCores; x++)
            {
                int xb = x;
                var transThr = new Thread(

                () =>
                {
                    int count = 0;
                    List<QueueProcess> chunkedTrans = tQueueToProcess.Skip(xb * sizeOfOneChunk)
                        .Take(sizeOfOneChunk).ToList();
                    foreach (var queueData in chunkedTrans)
                    {
                        //think this was bug
                        ProccessResponse success = new ProccessResponse();
                        success = ProcessMethod(queueData);
                        successDictionary.TryAdd(queueData.queue.Id, success);
                    }
                });
                threads.Add(transThr);
            }

            foreach (var transThread in threads)
            {
               
                transThread.Start();           
            }

            foreach (var transThread in threads)
            {
                transThread.Join();

            }
            //clear threads list 
           // threads.Clear();

            RunSuccessUpdate(successDictionary);
            RunResponse(successDictionary);
        }


        public static void RunSuccessUpdate(ConcurrentDictionary<int, ProccessResponse> successDictionary)
        {
            foreach (var update in successDictionary)
            {

                ProccessResponse pr = successDictionary.Values.FirstOrDefault();
                int counter = 0;
                string success = "Succuess";
                string msg = pr.Information;
                if (pr.Successful == false)
                {
                    success = "Failed";
                    
                }

                using (var db = new ZudelloContext())
                {

                    var queue = db.Zqueue.Where(i => i.Id == update.Key).FirstOrDefault();
                    queue.Status = success;
                    queue.Exception = msg;
                    db.SaveChangesAsync();
                    counter++; //Maybe add in logic later if all is not updated then handel errors.
                    db.DisposeAsync();
                }
            }


        }

        public static void RunResponse(ConcurrentDictionary<int, ProccessResponse> successDictionary)
        {
           
            string Token = ZudelloLogin.Login();
            List<Zconnections> teams = new List<Zconnections>();
            using (var db = new ZudelloContext())
            {
                //Maybe hash the records

                teams = db.Zconnections.ToList();
               
            }

            foreach (var update in successDictionary)
            {
                

                int counter = 0; //think there is a bug with duplicate invoices trying to be processed. 

                ProccessResponse pr = successDictionary.Values.FirstOrDefault();

                if (pr.Team == null) continue;           
                string success = "Succuess";
                string msg = pr.Information;
                Response sendHome = new Response();
                sendHome.body = pr.Information;
                sendHome.status = 200;
                sendHome.uuid = pr.SyncHistryUuid;
         

                if (pr.Successful == false)
                {
                    sendHome.status = 500;

                }

               string body = JsonConvert.SerializeObject(sendHome);

                try
                {                 
                   Console.WriteLine(ZudelloLogin.SendProcessResponse(Token, body, pr.Team));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.InnerException);

                }

            }



        }
        public static ProccessResponse ProcessMethod(dynamic queueData)
        {

            ProccessResponse pr = new ProccessResponse();

            
            //Get list of Databases
            SQLCredentials ConnectionString = new SQLCredentials();
            Dictionary<int, string> Connection = ConnectionString.ConnectionStringBuilder();
            Console.WriteLine(queueData.queue.Id.ToString());
            dynamic zudelloObject = "";
            try
               {
                   zudelloObject = JsonConvert.DeserializeObject<ExpandoObject>(queueData.queue.Body);
                   string order = queueData.map.ProcessOrder.ToString();
                 
                   //Created at 
                   string obj = queueData.map.DocType.ToString();
                   string uuid = "";//zudelloObject.invoiceUUID.ToString();
                   string queueID = queueData.queue.Id.ToString();

                   //Generate the insert Query
                   string SQLQuery = ExoTools.RenderToSql(queueData.map.Body, zudelloObject);


                if (zudelloObject.uuid != null)
                {
                    pr.SyncHistryUuid = zudelloObject.uuid;
                    pr.Team = zudelloObject.document.teamUuid;
                }

                   string cmd = SQLQuery;

                       //Connect to correct database 
                       using (SqlConnection mConnection = new SqlConnection(Connection[queueData.map.connection_id]))
                       {
                           mConnection.Open();                   
                           //Get a list of all ther table names in the Database
                           using (SqlCommand command = new SqlCommand(cmd, mConnection))
                           {
                             command.ExecuteNonQuery();

                           }
                            mConnection.Dispose();
                            // mConnection.Dispose();
                       }

                         pr.Successful = true;
                         pr.Information = "Success";
             
                         return pr;

               }


               catch (Exception ex)
               {
                // Console.WriteLine(ex.Message);

               
                pr.Successful = false;
                pr.Information = String.Format("Exception Message: {0} ||" +
                    " Exception Stack Trace: {1} ||" +
                    " Exception Inner Ex: {2} ", ex.Message, ex.StackTrace, ex.InnerException);

               
                return pr;

               }

 

            
        }

   
    }
}

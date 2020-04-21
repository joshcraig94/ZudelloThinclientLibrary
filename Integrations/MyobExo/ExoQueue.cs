using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZudelloApi;
using ZudelloThinClientLibary;

namespace MyobExoConnector.EXO

{

    public class LastSync
    { 
       public int teamId { get; set; }
       public string lastUpdated { get; set; }
    }

    public class ExoMappingAndDB : Zconnections
    {
        public int mappingID { get; set; }

    }


    public class ExoQueue
    {
        public static int?  SaveToDBQueue(string rawJson, int type, string status, ExoMappingAndDB map, int? linkedId = null )
        {
            try
            {
                using (var db = new ZudelloContext())
                {

                    var res = db.Zqueue.Add(new Zqueue { Body = rawJson,
                                                        MappingId = type, 
                                                        Status = status,
                                                        ConnectionId = map.Id, 
                                                        Queue_Id = linkedId
                    });

                    db.SaveChanges();
                    db.DisposeAsync();
                    return res.Entity.Id;
                }
            }
            catch (Exception ex)
            {

                return null;
            
            }


        }



        public static ExoMappingAndDB GetMappingAndDatabase(string Type = "", string connectionId = "")
        {
            ExoMappingAndDB mapDb = new ExoMappingAndDB();

            using (var db = new ZudelloContext())
            {

                var myConnectionID = db.Zconnections.Where(c => c.ConnectionUuid == connectionId).FirstOrDefault();

                var MapID = db.Zmapping.Where(i => i.DocType == Type && i.IsOutgoing == 0 && i.connection_id == myConnectionID.Id);


                int count = 0;
                try
                {
                    count = MapID.Count();
                }
                catch
                {

                }
                if (count > 1) return null;
                foreach (var id in MapID)
                {
                    mapDb.Id = myConnectionID.Id;
                    mapDb.InitialCatalog = myConnectionID.InitialCatalog;
                    mapDb.DataSource = myConnectionID.DataSource;
                    mapDb.IntergrationType = myConnectionID.IntergrationType;
                    mapDb.mappingID = id.Id;
                    return mapDb;
                }

                db.DisposeAsync();
            }



            return null;

        }

        public static bool InQueue(int? mappingId, string toSearch)
        {
            using (var db = new ZudelloContext())
            {
                //Maybe hash the records
                var MapID = db.Zqueue.Where(i => i.MappingId == mappingId && i.Body.Contains(toSearch));
                int count = MapID.Count();
                db.DisposeAsync();
                if (count > 0) return true;
                else return false;

            }

        }

        public static async Task AddtoQueue()
        {



            //Check Mappings 
            ZudelloLogin.GetZudelloMappings().Wait();
            string Token = ZudelloLogin.Login();
            List<Zconnections> teams = new List<Zconnections>();
            using (var db = new ZudelloContext())
            {
                //Maybe hash the records

               teams = db.Zconnections.ToList();
               await db.DisposeAsync();
            }

            int numberOfLogicalCores = Environment.ProcessorCount;
            List<Thread> threads = new List<Thread>(numberOfLogicalCores);
            int sizeOfOneChunk = (teams.Count / numberOfLogicalCores) + 1;            
            ConcurrentDictionary<int, dynamic> cb = new ConcurrentDictionary<int, dynamic>();
            ConcurrentDictionary<dynamic, LastSync> lastSyncBag = new ConcurrentDictionary<dynamic, LastSync>();
            for (int i = 0; i < numberOfLogicalCores; i++)
            {
                int ab = i;
                var thr = new Thread(

                () =>
                {

                    int count = 0;
                    List<Zconnections> teamChunked = teams.Skip(ab * sizeOfOneChunk)
                        .Take(sizeOfOneChunk).ToList();
                                                                                          

                    foreach (var team in teamChunked)
                    {


                        try
                        {
                            dynamic ZudelloTeam = JsonConvert.DeserializeObject<ExpandoObject>(team.ZudelloCredentials);
                            string InvoiceList = ZudelloLogin.CallZudelloDocs(Token, ZudelloTeam.team, team.Id);
                            dynamic ToSync = JObject.Parse(InvoiceList);

                            foreach (var data in ToSync.data)
                            {
                                try
                                {

                                    string myDataString = data.ToString();
                                    // Console.WriteLine(Id.uuid + "," + Id.items);
                                    Console.WriteLine(data);


                                    ExoMappingAndDB mapId = GetMappingAndDatabase(data.document.docType.ToString(),
                                                                                  data.document.connectionUuid.ToString());
                                    int? rowId = null;
                                    if (mapId != null)
                                     rowId = SaveToDBQueue(myDataString, mapId.mappingID, "Waiting", mapId);



                                    var validate = Validations.ExoSupplierInvoice(myDataString, mapId);

                                    if (validate.CreateSupplier == true)
                                    {
                                        //HAVE to hard code type for the supplier and inventory? 
                                        mapId = GetMappingAndDatabase(data.document.supplier.docType.ToString(), data.document.connectionUuid.ToString());

                                        // Check if already in the queue
                                        if (InQueue(mapId.mappingID, data.document.supplier.code.ToString()) == false)
                                            SaveToDBQueue(data.document.supplier.ToString(), mapId.mappingID, "Waiting", mapId, rowId);
                                    }

                                    if (validate.CreateInventory == true)
                                    {


                                        foreach (var itemData in data.document.lines)
                                        {
                                            mapId = GetMappingAndDatabase(itemData.docType.ToString(), data.document.connectionUuid.ToString());

                                            if (validate.InventoryToCreate.Contains(itemData.item.sku.ToString()))
                                            {

                                                // Check if already in the queue
                                                if (InQueue(mapId.mappingID, itemData.item.sku.ToString()) == false)
                                                    SaveToDBQueue(itemData.ToString(), mapId.mappingID, "Waiting", mapId, rowId);

                                            }

                                        }

                                    }

                                    LastSync lastSync = new LastSync();
                                    lastSync.teamId = team.Id;
                                    lastSync.lastUpdated = data.created_at.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

                                    lastSyncBag.TryAdd(data.uuid, lastSync);

                                
                                }
                                catch (Exception ex)
                                {
                                 
                                    Console.WriteLine(ex.Message);

                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            //Write to loggings tables
                            Console.WriteLine(ex.Message);



                        }

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


            var dataList = lastSyncBag.Values.ToList();
            foreach (var lastSync in dataList)
            {
              

                using (var db = new ZudelloContext())
                {


                    var updateLastSync = (from a in db.Zlastsync
                                          join c in db.Zmapping on a.MappingId equals c.Id
                                          where c.DocType == "CallZudelloDocs" && c.connection_id == lastSync.teamId
                                          select a).FirstOrDefault();

                    if (DateTime.Parse(updateLastSync.LastSync) > DateTime.Parse(lastSync.lastUpdated))
                    {
                 
                      
                    }
                    else
                    {
                       
                        updateLastSync.LastSync = lastSync.lastUpdated;                       
                       
              
                        db.SaveChanges();
                     


                    }

                    db.Dispose();

                }

            }
            
         



        }
    }
}


    


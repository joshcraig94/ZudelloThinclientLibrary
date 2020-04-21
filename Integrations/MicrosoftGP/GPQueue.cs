using Microsoft.Dynamics.GP.eConnect.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZudelloApi;
using ZudelloThinClientLibary;

namespace MicrosoftGPConnector
{
   
    public class GpMappingAndDatabase : Zconnections
    {
        public int mappingID { get; set; }

    }


    public class GpQueue
    {
        public static void SaveToDBQueue(string rawJson, int type, string status, GpMappingAndDatabase map)
        {

            using (var db = new ZudelloContext())
            {

                db.Zqueue.Add(new Zqueue { Body = rawJson, MappingId = type, Status = status, ConnectionId = map.Id });
                db.SaveChanges();

            }

        }



        public static GpMappingAndDatabase GetMappingAndDatabase(string Type = "", string connectionId = "")
        {
            GpMappingAndDatabase mapDb = new GpMappingAndDatabase();

            using (var db = new ZudelloContext())
            {

                var myConnectionID = db.Zconnections.Where(c => c.ConnectionUuid == connectionId).FirstOrDefault();

                var MapID = db.Zmapping.Where(i => i.DocType == Type && i.IsOutgoing == 0 && i.connection_id == myConnectionID.Id);


                int count = MapID.Count();
                if (count > 1) return null; // if there is the same mapping in the connection will return null hehe
                foreach (var id in MapID)
                {
                    mapDb.Id = myConnectionID.Id;
                    mapDb.InitialCatalog = myConnectionID.InitialCatalog;
                    mapDb.DataSource = myConnectionID.DataSource;
                    mapDb.IntergrationType = myConnectionID.IntergrationType;
                    mapDb.mappingID = id.Id;
                    return mapDb;
                }
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
                if (count > 0) return true;
                else return false;

            }

        }

        public static void AddtoQueue()
        {


            //Check Mappings 
            ZudelloLogin.GetZudelloMappings();


            string Token = ZudelloLogin.Login();
            string InvoiceList = ZudelloLogin.CallZudelloDocs(Token);


            dynamic ToSync = JObject.Parse(InvoiceList);

            foreach (var data in ToSync.data)
            {

                try
                {
                   
                    string myDataString = data.ToString();
                    // Console.WriteLine(Id.uuid + "," + Id.items);
                    Console.WriteLine(data);


                    GpMappingAndDatabase mapId = GetMappingAndDatabase(data.document.docType.ToString(), data.document.connectionUuid.ToString());

                    if (mapId != null)
                        SaveToDBQueue(myDataString, mapId.mappingID, "Waiting", mapId);

                  

                        var validate = GpValidations.SupplierInvoiceValidate(myDataString, mapId);

                         if (validate.CreateSupplier == true)
                         {
                             //HAVE to hard code type for the supplier and inventory? 
                             mapId = GetMappingAndDatabase(data.document.supplier.docType.ToString(), data.document.connectionUuid.ToString());

                             // Check if already in the queue
                             if (InQueue(mapId.mappingID, data.document.supplier.code.ToString()) == false)
                                 SaveToDBQueue(data.document.supplier.ToString(), mapId.mappingID, "Waiting", mapId);
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
                                         SaveToDBQueue(itemData.ToString(), mapId.mappingID, "Waiting", mapId);

                                 }

                             }

                         } 

                    using (var db = new ZudelloContext())
                    {


                        var updateLastSync = (from a in db.Zlastsync
                                              join c in db.Zmapping on a.MappingId equals c.Id
                                              where c.DocType == "CallZudelloDocs"
                                              select a).FirstOrDefault();

                        //var dateParse = DateTime.ParseExact(data.created_at.ToString(), "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture);
                        //string zudelloDate = data.created_at;

                        //  dateParse = dateParse


                        string pr = data.created_at.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");


                        updateLastSync.LastSync = pr;
                        db.SaveChanges();


                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);

                }

            }
        }
    }
}

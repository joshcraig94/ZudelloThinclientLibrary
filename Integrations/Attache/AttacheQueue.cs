using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZudelloApi;
using ZudelloThinClient.Attache.AttacheSettings;
//using ZudelloThinClient.SQLite;
using ZudelloThinClientLibary;
using AttacheValidation;

namespace ZudelloThinClient.Attache
{
    //<code>Null refference</code> issue
   public class AttacheQueue
    {        
        
        
        public static void SaveToDBQueue(string rawJson, int? type, string status)
        {
            
            using (var db = new ZudelloContext())
            {
                
                db.Zqueue.Add(new Zqueue { Body = rawJson, MappingId = type, Status = status,ConnectionId = 1 });               
                db.SaveChanges();

            }

        }

        public static int? GetMappingTypeID(string Type ="")
        {
           
            
            using (var db = new ZudelloContext())
            {
                var MapID = db.Zmapping.Where(i => i.DocType == Type && i.IsOutgoing == 0);
                

                int count = MapID.Count();
                if (count > 1) return null;
                foreach (var id in MapID)
                {
                   // Console.WriteLine("Found ID");

                    return id.Id;
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

        public static async Task AddtoQueue()
        {


            //Check Mappings 
             ZudelloLogin.GetZudelloMappings().Wait();


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
                    int? mapId = GetMappingTypeID(data.document.docType.ToString());
                    if (mapId != null) 
                    SaveToDBQueue(myDataString, mapId, "Waiting");


                    var validate = AttacheValidations.AttacheSupplierInvoice(myDataString);

                    if (validate.CreateSupplier == true)
                    {
                        //HAVE to hard code type for the supplier and inventory? 
                        mapId = GetMappingTypeID(data.document.supplier.docType.ToString());

                        // Check if already in the queue
                        if (InQueue(mapId, data.document.supplier.code.ToString()) == false)
                            SaveToDBQueue(data.document.supplier.ToString(), mapId, "Waiting");
                    }

                    if (validate.CreateInventory == true)
                    {


                        foreach (var itemData in data.document.lines)
                        {

                            mapId = GetMappingTypeID(itemData.docType.ToString());

                            if (validate.InventoryToCreate.Contains(itemData.item.sku.ToString()))
                            {

                                // Check if already in the queue
                                if (InQueue(mapId, itemData.item.sku.ToString()) == false)
                                    SaveToDBQueue(itemData.ToString(), mapId, "Waiting");

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

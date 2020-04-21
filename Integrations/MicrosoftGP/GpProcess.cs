using Microsoft.Dynamics.GP.eConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml;
using ZudelloThinClientLibary;

namespace MicrosoftGPConnector
{
    public class GpProcess
    {
        public static void ProcessRecords()
        {



            using (var db = new ZudelloContext())
            {

                List<int> Success = new List<int>();
                var MasterDataAwaiting = (from a in db.Zqueue
                                          join c in db.Zmapping on a.MappingId equals c.Id
                                          where c.IsMasterData == 1 && a.Status == "Failed"
                                          select a).Count();


                //The Cleaner Should be checking if these are in which folders 
                if (MasterDataAwaiting > 0)
                {
                    //run procedure to check folders
                    return;

                }


                if (MasterDataAwaiting == 0)
                {

                    //Get All Master Data in waiting status
                    var MasterData = (from queue in db.Zqueue
                                      join map in db.Zmapping on queue.MappingId equals map.Id
                                      where map.IsMasterData == 1 && queue.Status.Trim() == "Waiting" //Will make more generic later
                                      select new { queue, map }).ToList();

                    if (MasterData.Count() > 0)
                    {
                        foreach (var queueData in MasterData)
                        {

                            bool success = ProcessMethod(queueData);

                            if (success == true)
                            {
                                //Mark Record as processing
                                queueData.queue.Status = "Success";
                                db.SaveChanges();
                            }

                            else
                            {
                                queueData.queue.Status = "Failed";
                                db.SaveChanges();
                            }

                        }
                        return; //exit
                    }

                }

                //Process transactional Data

                var TransData = (from queue in db.Zqueue
                                 join map in db.Zmapping on queue.MappingId equals map.Id
                                 where map.IsMasterData == 0
                                 && queue.Status.Trim() == "Waiting" //Will make more generic later
                                 select new { queue, map }).ToList();

                foreach (var queueData in TransData)
                {

                    bool success = ProcessMethod(queueData);

                    if (success == true)
                    {
                        queueData.queue.Status = "Success";
                        db.SaveChanges();
                    }

                    else
                    {
                        queueData.queue.Status = "Failed";
                        db.SaveChanges();
                    }

                }







            }




        }

        public static bool ProcessMethod(dynamic queueData)
        {

            
            string econnectDocument;
            string sXsdSchema;
            string sConnectionString;


            try
            {
                dynamic zudelloObject = JsonConvert.DeserializeObject<ExpandoObject>(queueData.queue.Body);
                string order = queueData.map.ProcessOrder.ToString();

                //Created at 
                string obj = queueData.map.DocType.ToString();
                string uuid = "";//zudelloObject.invoiceUUID.ToString();
                string queueID = queueData.queue.Id.ToString();

                //Generate the eConnectXml
                string xmlRendered = GpTools.RenderXml(queueData.map.Body, zudelloObject);
                using(eConnectMethods e = new eConnectMethods())
                {
                    try
                    {

                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.LoadXml(xmlRendered);
                        econnectDocument = xmldoc.OuterXml;

                        //User ID=sa;Password=sa
                        using (var db = new ZudelloContext())
                        {

                            
                            var Connection = db.Zconnections.Where(i => i.Id == 1).FirstOrDefault();

                            sConnectionString = String.Format(@"Data Source={0};Integrated Security = SSPI; Persist Security Info = false ; Initial Catalog ={1};",Connection.DataSource,Connection.InitialCatalog);
                            db.Dispose();
                        }
                      // sConnectionString = @"Data Source=LAPTOP-BUDQ9SBN\DYNAMICGP;Integrated Security = SSPI; Persist Security Info = false ; Initial Catalog = GP_DE;";
                        // Create an XML Document object for the schema
                        XmlDocument XsdDoc = new XmlDocument();

                        // Create a string representing the eConnect schema
                        sXsdSchema = XsdDoc.OuterXml;

                        // Pass in xsdSchema to validate against.
                     bool created =  e.CreateEntity(sConnectionString, econnectDocument);
                        if (created == true)
                            return true;
                        else return false;
                    }

                    // The eConnectException class will catch eConnect business logic errors.
                    // display the error message on the console
                    catch (eConnectException exc)
                    {
                        Console.Write(exc.ToString());
                        e.Dispose();
                        return false;

                    }
                    // Catch any system error that might occurr.
                    // display the error message on the console
                    catch (System.Exception ex)
                    {
                        Console.Write(ex.ToString());
                        e.Dispose();
                        return false;

                    }
            
                }              

            }


            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;

            }
        }
    }
}

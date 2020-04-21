using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using Scriban;
using ZudelloApi;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ZudelloApi;
using Scriban.Runtime;
using System.ComponentModel;
using System.Reflection;
using ZudelloThinClient.Attache.AttacheODBC;
using System.Data.Odbc;
using System.Dynamic;
using ZudelloThinClient.Attache.AttacheSettings;
using ZudelloThinClientLibary;
using System.Threading.Tasks;

namespace ZudelloThinClient.Attache
{

    /***********************************************************************************************
     * Need to add in logic to check if a PO exisits Or Receipt Exisits. 
     * 4 ways a Invoice Can be entered;
     * 1. No PO or Receipt as per current implmentation. 
     * 2. Multiple PO and Receipts. 
     * 3. Receipts Readin 
     * 4. PO Read in
     * Question, Best to add this logic in the Templates using the Tempalte Language 
     * or Code here with switch statement
     ************************************************************************************************/

  public  class AttacheProcess
    {



        public static async Task ProcessRecords()
        {
            AttacheConfiguration Folder = ZudelloSetup.GetAttacheSettings();


            using (var db = new ZudelloContext())
            {

                List<int> Success = new List<int>();
                var MasterDataAwaiting = (from a in db.Zqueue
                                           join c in db.Zmapping on a.MappingId equals c.Id
                                           where c.IsMasterData == 1 && a.Status == "Processing"
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
                            queueData.queue.Status = "Processing";                         
                            db.SaveChanges();
                        }
                         
                    }
                        return; //exit
                    }

                }

                //Process transactional Data

                    var TransData = (from queue in db.Zqueue
                                      join map in db.Zmapping on queue.MappingId equals map.Id
                                      where map.IsMasterData == 0 && queue.Status.Trim() == "Waiting" //Will make more generic later
                                      select new { queue, map }).ToList();
                   
                    foreach (var queueData in TransData)
                    {

                        bool success = ProcessMethod(queueData);

                        if (success == true)
                        {
                            queueData.queue.Status = "Processing";
                            db.SaveChanges();
                        }

                    }



                



            }




        }




        public static string RenderToKfi(string kfiTemplate, object obj = null)
        {

            var scribanObject = new ScriptObject();
            scribanObject.Import(typeof(MyScribianFunctions));
            scribanObject.Import(typeof(AttacheScribanExtension));

            scribanObject.Import("z", new Func<dynamic>(() => obj));


            var context = new TemplateContext();
            context.PushGlobal(scribanObject);
            var template = Template.Parse(kfiTemplate);
            if (template.HasErrors)
                throw new Exception(string.Join("\n",
                    template.Messages.Select(x => $"{x.Message} at {x.Span.ToStringSimple()}")));

            string ToFormat = template.Render(context);
            //Formatting 

            string Replace1 = ">\\s+";
            string Replace2 = "\\s+<";

            ToFormat = Regex.Replace(ToFormat, Replace1, ">");
            ToFormat = Regex.Replace(ToFormat, Replace2, "<");
            return ToFormat;
        }

        public static bool SaveToKfi(string order, string obj, string uuid, string queueID ,string kfiRendered)
        {

            string currentTime = string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now); // Regex.Replace(DateTime.Now.ToString("hh.mm.ss.ffffff"), @"[^0-9a-zA-Z]+", "");
            string inboxPath = ZudelloSetup.GetAttacheSettings().AttacheInbox;
            try
            {
                
                File.WriteAllText(inboxPath + String.Format("{0}_{1}_{2}_{3}_{4}.kfi", order, currentTime, obj.Replace(":",""), uuid, queueID), kfiRendered.RemoveEmptyTags());
                return true;

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public static bool ProcessMethod(dynamic queueData)
        {

            try 
            { 
                dynamic zudelloObject = JsonConvert.DeserializeObject<ExpandoObject>(queueData.queue.Body);
                string order = queueData.map.ProcessOrder.ToString();
                //Created at 
                string obj = queueData.map.DocType.ToString();
                string uuid = "";//zudelloObject.invoiceUUID.ToString();
                string queueID = queueData.queue.Id.ToString();
                string attacheKfi = RenderToKfi(queueData.map.Body, zudelloObject);
                bool saved = SaveToKfi(order, obj, uuid, queueID, attacheKfi);
                return true;


            }


            catch (Exception ex)
            {
              Console.WriteLine(ex.Message);
              return false;

            }




        }


    }


    
}

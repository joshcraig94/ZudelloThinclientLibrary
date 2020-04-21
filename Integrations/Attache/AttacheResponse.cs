using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZudelloApi;
using ZudelloThinClient.Attache.AttacheSettings;
using ZudelloThinClientLibary;

namespace ZudelloThinClient.Attache
{
#warning add in API call to zudello 
    public class AttacheResponse
    {
        public static async Task AttacheResponseSender()
        {
            string token = ZudelloLogin.Login();
            ResponseZudello ResponseStatus = new ResponseZudello();

            using (var db = new ZudelloContext())
            {

                var queueList = db.Zqueue.Where(i => i.Status == "Success" && (i.ResponseSent == 0)).ToList();

                try
                {
                    foreach (var l in queueList)
                    {
                        ResponseObject successList = new ResponseObject();
                        dynamic Body = JObject.Parse(l.Body);
                        successList.body = "Success";
                        successList.uuid = Body.uuid;
                        successList.queueid = l.Id;
                        successList.status = 200;
                        ResponseStatus.data.Add(successList);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Master Data");
                }

                foreach (var data in ResponseStatus.data)
                {
                    string responseBody = JsonConvert.SerializeObject(data);

                    HttpStatusCode Response = ZudelloLogin.SendProcessResponse(token, responseBody);

                    if (Response == HttpStatusCode.OK)
                    { 
                        var updateSent = db.Zqueue.Where(i => i.Id == data.queueid).FirstOrDefault();
                        updateSent.ResponseSent = 1;
                        db.SaveChanges();
                    }

                }

                db.Dispose();

            }

        }


    }


    public class ResponseZudello
    {
        public List<ResponseObject> data { get; set; } = AttacheResponses.KfiErrorGetter();

    }

    public class ErrorFiles
    {
        public object errorFile { get; set; }
        public object originalFile { get; set; }
    }
    public class ResponseObject
    {
     
        public string uuid { get; set; }
        public int status { get; set; }
        public string body { get; set; }   
        public int queueid { get; set; }
      
   
    }

   


    

    public class AttacheResponses 
    {           


        public static List<ResponseObject> KfiErrorGetter()
        {

            List<ResponseObject> Response = new List<ResponseObject>();
            List<Zqueue> queueList = new List<Zqueue>();
            List<string> fileDir = new List<string>();
            try
            {
                using (var db = new ZudelloContext())
                {
                    //Get a list of all failed items. 
                    //var failureQueue = db.Zqueue.Where(i => i.Status == "Failure" || i.Status == "Success" && ( i.ResponseSent == 0)).Select();
                    queueList  = db.Zqueue.Where(i => i.Status == "Failure" && (i.ResponseSent == 0) || i.Status == "NotProcessed" && (i.ResponseSent == 0)).ToList();
                 

                    //Get a List of all .eer Files
                    AttacheConfiguration FolderNames = ZudelloSetup.GetAttacheSettings();
                    
                    var Fdir = FolderNames.AttacheInbox + "Failure";
                    fileDir.Add(Fdir);
                    Fdir = FolderNames.AttacheInbox + "NotProcessed";
                    fileDir.Add(Fdir);

                    foreach (var dir in fileDir)
                    { 

                    DirectoryInfo d = new DirectoryInfo(dir);
                    FileInfo[] Files = d.GetFiles("*.err"); 

                    List<string> err = new List<string>();
                    foreach (FileInfo file in Files)
                    {
                        ResponseObject SendFailedData = new ResponseObject();
                        ErrorFiles Errors = new ErrorFiles();
                        //Check if failed item is part of queue
                        try
                        {

                         
                            int index1 = file.FullName.LastIndexOf('_');
                            string queueId = Regex.Match(file.FullName.Substring(index1), @"\d+").Value;

                            // Convert to int to check
                            int id = 0;
                            int counter = 0;
                            Int32.TryParse(queueId, out id);
                            try
                            {                               
                                counter = queueList.Where(i => i.Id == id).Count(); 
                            }

                            catch (Exception ex)
                            { 
                            
                            }

                            if (counter > 0)
                            {
                                try
                                {
                                    //Send to Zudello this error File. 

                                    Errors.errorFile = File.ReadAllText(file.FullName);

                                    // Console.WriteLine(File.ReadAllText(file.FullName));
                                    FileInfo[] originalKfi = d.GetFiles(String.Format("*_{0}.kfi", id.ToString()));
                                    //Get the Original KFI

                                    //Console.WriteLine(File.ReadAllText(originalKfi.FirstOrDefault().FullName));
                                    try
                                    {
                                        Errors.originalFile = File.ReadAllText(originalKfi.FirstOrDefault().FullName);

                                        // Console.ReadLine();
                                    }

                                    catch (Exception ex)
                                    {
                                        //If we cannot locate original 
                                        Errors.originalFile = ex.Message;
                                    }
                                    //Bool check to confirm error file was sent to zudello.                              

                                    ZudelloLogin.SendProcessResponse();
                                    var updateQuery = db.Zqueue.Where(i => i.Id == id).FirstOrDefault();

                                    //get the} Syn Histry Uuid
                                    dynamic Body = JObject.Parse(updateQuery.Body);
                                    SendFailedData.body = JsonConvert.SerializeObject(Errors);
                                    SendFailedData.status = 500;
                                    SendFailedData.uuid = Body.uuid;
                                    SendFailedData.queueid = updateQuery.Id;

                                    Response.Add(SendFailedData);


                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    //Will be issue with items and customers need to revise
                                    continue; 
                                }

                            }
                            else
                            {
                                //Next Item already been sent to Zudello Redundant now
                                continue;
                            
                            }
                          

                        }

                        catch (Exception ex)
                        {                            


                            return Response;

                        }
                    
                    }

                    }
                    //Add the success data to return Object

                    return Response;

                }
            }
            catch (Exception ex)
            {


                return Response;
            }



        }

     


    }
}

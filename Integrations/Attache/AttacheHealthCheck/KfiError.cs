using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZudelloApi;
using ZudelloThinClient.Attache.AttacheSettings;
using ZudelloThinClientLibary;

namespace ZudelloThinClient.Attache.AttacheHealthCheck
{
#warning add in API call to zudello 
    public class ErrorFileSender
    {
        public static void KfiErrorSender()
        {
            
            ErrorToZudello KfiError = new ErrorToZudello();
            string Token = ZudelloLogin.Login();
            Data QData = new Data();
            Output output = new Output();
            QData.object_type = "KfiError";
            QData.data = KfiError;
            output.data = QData;
            string data = JsonConvert.SerializeObject(output);

            Console.WriteLine(data);          
            Console.WriteLine(ZudelloLogin.SendToZudelloQueue(Token, data));         
            
        }


    }


    public class ErrorToZudello
    {
        public List<ErrorData> data { get; set; } = KfiResponse.KfiErrorGetter();

    }
    public class ErrorData
    {
        public object errorFile { get; set; }
        public object originalFile { get; set; } 

        public object body { get; set; }

    }

   


    

    public class KfiResponse 
    {           


        public static List<ErrorData> KfiErrorGetter()
        {
            List<ErrorData> errList = new List<ErrorData>();

            try
            {
                using (var db = new ZudelloContext())
                {
                    //Get a list of all failed items. 
                    var failureQueue = db.Zqueue.Where(i => i.Status == "Failure").Select(s => s.Id).ToList();

                    

                    //Get a List of all .eer Files
                    AttacheConfiguration FolderNames = ZudelloSetup.GetAttacheSettings();

                    var Fdir = FolderNames.AttacheInbox + "Failure";
                    DirectoryInfo d = new DirectoryInfo(Fdir);
                    FileInfo[] Files = d.GetFiles("*.err"); 

                    List<string> err = new List<string>();
                    foreach (FileInfo file in Files)
                    {
                        ErrorData errData = new ErrorData();
                        //Check if failed item is part of queue
                        try
                        {

                         
                            int index1 = file.FullName.LastIndexOf('_');
                            string queueId = Regex.Match(file.FullName.Substring(index1), @"\d+").Value;

                            // Convert to int to check
                            int id = 0;
                            Int32.TryParse(queueId, out id);

                            if (failureQueue.Contains(id))
                            {

                                //Send to Zudello this error File. 

                                errData.errorFile = File.ReadAllText(file.FullName);
                                
                                // Console.WriteLine(File.ReadAllText(file.FullName));
                                FileInfo[] originalKfi = d.GetFiles(String.Format("*_{0}.kfi", id.ToString()));
                                //Get the Original KFI

                                //Console.WriteLine(File.ReadAllText(originalKfi.FirstOrDefault().FullName));
                                try
                                {
                                    errData.originalFile = File.ReadAllText(originalKfi.FirstOrDefault().FullName);
                                  
                                    // Console.ReadLine();
                                }

                                catch (Exception ex)
                                {
                                    //If we cannot locate original 
                                    errData.originalFile = ex.Message;
                                }
                                //Bool check to confirm error file was sent to zudello.                              


                                var updateQuery = db.Zqueue.Where(i => i.Id == id).FirstOrDefault();
                                errData.body = JsonConvert.DeserializeObject(updateQuery.Body);
                                updateQuery.Status = "ZudelloReview";
                                db.SaveChanges();

                                errList.Add(errData);



                            }
                            else
                            {
                                //Next Item already been sent to Zudello
                                continue;
                            
                            }
                          

                        }

                        catch (Exception ex)
                        {
                            return errList;


                        }
                    
                    }

                    return errList;







                }
            }
            catch (Exception ex)
            {
                return errList;
            }



        }

     


    }
}

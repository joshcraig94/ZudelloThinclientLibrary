using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZudelloThinClientLibary;
//using ZudelloThinClient.SQLite;
using ZudelloThinClientLibary.SetupConfig;
using ZudelloThinClientLibary.SQLite;

namespace ZudelloApi
{
    //QueueObject

    public class Response
    {
        public string uuid { get; set; }
        public int status { get; set; }

        public string body { get; set; }
    }
    public class Output
    {
        public Data data { get; set; } = new Data();

    }
    public class Data
    {
        public object object_type { get; set; }

        public string connection_uuid { get; set; } = ZudelloInitalSettings.GetZudelloQueueConnection().connection_uuid;
        public object data { get; set; }
    }
    public class TokenObj
    {
        public string token { get; set; }

    }
    public class ConnectionUuid
    {
        public string connection_uuid { get; set; }
    }
    public class FirstLoginBody
    {
        public string email { get; set; }
        public string password { get; set; }

        public string platform { get; set; } = "WEBSITE";
    }
    public class LoginBody
    {
        public string email { get; set; } = ZudelloInitalSettings.ZudelloCredetials().email;
        public string password { get; set; } = ZudelloInitalSettings.ZudelloCredetials().password;
        public string platform { get; set; } = ZudelloInitalSettings.ZudelloCredetials().platform;
    }
    public class ZudelloLogin
    {


        public static HttpStatusCode SendProcessResponse(string token = "", string body = "", string myTeam = null, int connectionId = 0)
        {
            try
            {

                string team = "";
                HttpStatusCode result;
                string website = ZudelloInitalSettings.GetZudelloSettings().ZudelloWebsite;
                string key = "";
                string apiurl;
                string ApiResult = "";
                string entity = @"thinclient/response";


                //Multiple Database Teams
                if (myTeam == null)
                {
                    team = ZudelloInitalSettings.GetZudelloSettings().ZudelloTeam;
                }
                else
                {
                    team = myTeam;
                }

                var client = new RestClient("https://api.zudello.com/api/thinclient/response");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("x-team", team);
                request.AddHeader("Authorization", "Bearer " + token);             
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                   return result = response.StatusCode;

                }
                else
                {
                    return result = response.StatusCode;

                }
              

            }
            catch (Exception ex)
            {
                return HttpStatusCode.BadRequest;
            }

            

        }
    

            

        public static string Login(string team = "", params FirstLoginBody[] loginoveride)
        {
            string result = "";
            //ZudelloSetup.GetZudelloSettings().ZudelloWebsite +
            string website = @"https://api.zudello.com/api/login/";
            string key = "";
            string apiurl;
            string body = "";

            if (loginoveride.Count() > 0)
            {
                foreach (FirstLoginBody x in loginoveride)
                {

                    body = JsonConvert.SerializeObject(x);
                    break;
                }
            }

            else
            {

                LoginBody login = new LoginBody();
                body = JsonConvert.SerializeObject(login);
            }

            // work out API call
            // do the api call
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            apiurl = website;
            var request = (HttpWebRequest)WebRequest.Create(apiurl);
            var data = Encoding.ASCII.GetBytes(body);

            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/json";
            request.Accept = "application/json";

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = responseString;

            }
            else
            {
                result = responseString;

            }

            if (team.Trim() == "")
            {
                TokenObj token = JsonConvert.DeserializeObject<TokenObj>(result);
                return token.token;
            }

            // If team is blank then the team has already been saved, else i need to walk object and extract team for logging in. 
            else

            {
                return result;
            }

        }

        public static string CallZudelloDocs(string token = "", string myTeam = null, int connectionId = 1)
        {



            string team = "";
            string result = "";
            string website = ZudelloInitalSettings.GetZudelloSettings().ZudelloWebsite;
            string key = "";
            string apiurl;
            string ApiResult = "";
            string entity = ZudelloInitalSettings.GetZudelloSettings().ZudelloInvoiceEndpoint;
            string createTimeQuery = "?created_at=";
            string createdate = "2010-11-20T05:55:29.000Z";

            //Multiple Database Teams
            if (myTeam == null) {
                team = ZudelloInitalSettings.GetZudelloSettings().ZudelloTeam;
            }
            else {
                team = myTeam;
            }


            try
            {
                using (var db = new ZudelloContext())
                {


                    var updateLastSync = (from a in db.Zlastsync
                                          join c in db.Zmapping on a.MappingId equals c.Id
                                          where c.DocType == "CallZudelloDocs" && c.connection_id == connectionId
                                          select a).FirstOrDefault();

                    createdate = updateLastSync.LastSync;//DateupdateLastSync.LastSync;
                 


                }
            }
            catch (Exception ex)
            {

            }

            createTimeQuery = createTimeQuery + createdate;
            // work out API call
            apiurl = website + entity + createTimeQuery;



            // do the api call
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = (HttpWebRequest)WebRequest.Create(apiurl);

            // var data = Encoding.ASCII.GetBytes(apibody);

            request.Method = "GET";
            //request.ContentLength = data.Length;
            request.ContentType = "application/json";
            request.Accept = "application/json";


            string platform = ZudelloInitalSettings.GetZudelloSettings().ZudelloPlatform;
            //"4rUtOPCRnGA5I1qXXLPPn16BdgjDPw6R"
            request.Headers.Add("x-team", team);
            request.Headers.Add("x-platform", platform);
            request.Headers.Add("Authorization", "Bearer " + token);




            /*  using (var stream = request.GetRequestStream())
              {
                  stream.Write(data, 0, data.Length);
              } */

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = responseString;

            }
            else
            {
                result = responseString;
            }
            return result;

        }

        public static string CallZudelloMapping(string token, string myTeam = null)
        {

            string result = "";
            string website = "";
            string key = "";
            string apiurl;
            string ApiResult = "";
            string entity = ZudelloInitalSettings.GetZudelloSettings().ZudelloMappingEndpoint;
            string intergration = ZudelloInitalSettings.GetZudelloSettings().intergrationType;
            string team = "";
            //Multiple Database Teams
            if (myTeam == null)
            {
                team = ZudelloInitalSettings.GetZudelloSettings().ZudelloTeam;
            }
            else
            {
                team = myTeam;
            }



            string createTimeQuery = "?updated_at=";
            string createdate = "2010-11-20T05:55:29.000Z";

            using (var db = new ZudelloContext())
            {


                try
                {
                    var updateLastSync = (from a in db.Zlastsync
                                          join c in db.Zmapping on a.MappingId equals c.Id
                                          where c.DocType == "CallZudelloMapping"
                                          select a).FirstOrDefault();

                    createdate = updateLastSync.LastSync;//DateupdateLastSync.LastSync;
                  //  db.SaveChanges();
                    db.DisposeAsync();
                }

                catch (Exception ex)
                {


                    db.DisposeAsync();

                }


            }

            createTimeQuery = createTimeQuery + createdate;
            // work out API call
            apiurl = website + entity + intergration + createTimeQuery;

            // work out API call
            //apiurl = website + entity + uuid;

            // do the api call
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = (HttpWebRequest)WebRequest.Create(apiurl);

            // var data = Encoding.ASCII.GetBytes(apibody);

            request.Method = "GET";
            //request.ContentLength = data.Length;
            request.ContentType = "application/json";
            request.Accept = "application/json";


            string platform = ZudelloInitalSettings.GetZudelloSettings().ZudelloPlatform;
            //"4rUtOPCRnGA5I1qXXLPPn16BdgjDPw6R"
            request.Headers.Add("x-team", team);
            request.Headers.Add("x-platform", platform);
            request.Headers.Add("Authorization", "Bearer " + token);




            /*  using (var stream = request.GetRequestStream())
              {
                  stream.Write(data, 0, data.Length);
              } */

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = responseString;
            }
            else
            {
                result = responseString;
            }
            return result;

        }

        public static string DeleteMapping(string token, string uuid)
        {
            string result = "";
            string website = "";
            string key = "";
            string apiurl;
            string ApiResult = "";
            string team = ZudelloInitalSettings.GetZudelloSettings().ZudelloTeam;
            string integrationType = ZudelloInitalSettings.GetZudelloSettings().intergrationType;
            string entity = @"https://api.zudello.com/api/thinclient/";
            string platform = ZudelloInitalSettings.GetZudelloSettings().ZudelloPlatform;
            // work out API call

            apiurl = entity + integrationType + uuid;

            // do the api call            

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = (HttpWebRequest)WebRequest.Create(apiurl);

            request.Method = "DELETE";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers.Add("x-team", team);
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Headers.Add("x-platform", platform);


            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = responseString;

            }
            else
            {
                result = responseString;

            }


            return result;

        }

        public static string SendToZudelloQueue(string token, string body, string myTeam = null)
        {
            string result = "";
            string website = "";
            string key = "";
            string apiurl;
            string ApiResult = "";
            string team = "";
            string entity = ZudelloInitalSettings.GetZudelloSettings().ZudelloQueue;

            // work out API call

            //Multiple Database Teams
            if (myTeam == null)
            {
                team = ZudelloInitalSettings.GetZudelloSettings().ZudelloTeam;
            }
            else
            {
                team = myTeam;
            }


            apiurl = entity + team + "/queueforwarding";

            // do the api call            

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = (HttpWebRequest)WebRequest.Create(apiurl);
            var data = Encoding.ASCII.GetBytes(body);

            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers.Add("x-team", team);
            request.Headers.Add("Authorization", "Bearer " + token);

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = responseString;
                using (var db = new ZudelloContext())
                {


                    var updateLastSync = (from a in db.Zlastsync
                                          join c in db.Zmapping on a.MappingId equals c.Id
                                          where c.DocType == "SendToZudelloQueue"
                                          select a).FirstOrDefault();

                    updateLastSync.LastSync = DateTime.UtcNow.ToString();
                    db.SaveChanges();


                }

            }
            else
            {
                result = responseString;

            }


            return result;

        }


        public static string CallZudelloSettings(string token, string team = "")
        {

            string result = "";
            string website = "";
            string key = "";
            string apiurl;
            string ApiResult = "";
            string entity = @"https://api.zudello.com/api/thinclient/attache/settings";

            // work out API call
            apiurl = website + entity;

            // do the api call
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = (HttpWebRequest)WebRequest.Create(apiurl);

            // var data = Encoding.ASCII.GetBytes(apibody);

            request.Method = "GET";
            //request.ContentLength = data.Length;
            request.ContentType = "application/json";
            request.Accept = "application/json";


            string platform = "WEBSITE";

            request.Headers.Add("x-team", team);
            request.Headers.Add("x-platform", platform);
            request.Headers.Add("Authorization", "Bearer " + token);



            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = responseString;

                try
                {
                    using (var db = new ZudelloContext())
                    {


                        var updateLastSync = (from a in db.Zlastsync
                                              join c in db.Zmapping on a.MappingId equals c.Id
                                              where c.DocType == "CallZudelloSettings"
                                              select a).FirstOrDefault();

                        updateLastSync.LastSync = DateTime.UtcNow.ToString();
                        db.SaveChanges();


                    }
                }
                catch (Exception ex)
                {


                }

            }
            else
            {
                result = responseString;


            }
            return result;

        }

        public static async Task SetSyncTable()
        {
            using (var db = new ZudelloContext())
            {
                var toSet = (from zm in db.Zmapping
                             join zs in db.Zlastsync
                                 on zm.Id equals zs.MappingId into r
                             from zs in r.DefaultIfEmpty()
                             where zs == null
                             select zm).ToList();


                foreach (var s in toSet)
                {
                    Zlastsync lastSyncTable = new Zlastsync();
                    lastSyncTable.MappingId = s.Id;
                    if (s.Type == "API")
                    {
                        lastSyncTable.LastSync = "2000-11-20T05:55:29.000Z";
                    }
                    else
                    {
                        lastSyncTable.LastSync = DateTime.UtcNow.AddYears(-100).ToString();
                    }
                    db.Add(lastSyncTable);
                    db.SaveChanges();
                    Console.WriteLine(String.Format("Sync Table Updated:{0}, {1} ", s.DocType, s.Type));
                }



            }



        }
        public static async Task GetZudelloMappings(string team = null)
        {
            string Token = ZudelloLogin.Login();
            //Output object should be this
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
            ConcurrentBag<Zmapping> cb = new ConcurrentBag<Zmapping>();
            ConcurrentDictionary<int, dynamic> cbd = new ConcurrentDictionary<int, dynamic>();
            for (int i = 0; i < numberOfLogicalCores; i++)
            {
                int ab = i;
                var thr = new Thread(
                    () =>
                    {
                        int count = 0;
                        List<Zconnections> teamChunked = teams.Skip(ab * sizeOfOneChunk)
                            .Take(sizeOfOneChunk).ToList();

                        foreach (var t in teamChunked) {
                            dynamic ZudelloTeam = JsonConvert.DeserializeObject<ExpandoObject>(t.ZudelloCredentials);
                            bool newMappings = false;
                            string GetMapping = ZudelloLogin.CallZudelloMapping(Token, ZudelloTeam.team);
                            ZmappingJson MappingFiles = JsonConvert.DeserializeObject<ZmappingJson>(GetMapping);
                           dynamic zudelloObject = JObject.Parse(GetMapping);

                            foreach (Zmapping map in MappingFiles.data)
                            {
                                
                                Console.WriteLine(map.UpdatedAt);

                                cb.Add(map);
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


            foreach (var map in cb)
            {
                bool newMappings = false;
                using (var db = new ZudelloContext())
                {


                  try {
                        Zmapping AddtoMap = map;

                        Zmapping MapID = db.Zmapping.Where(i => i.DocType == map.DocType 
                        && i.Type == map.Type 
                        && i.connection_id == map.connection_id).FirstOrDefault();

                        if (MapID.Id > 0)
                        {

                            MapID.Body = map.Body; //= map.ShallowCopy();
                            MapID.IsOutgoing = map.IsOutgoing;
                            MapID.ProcessOrder = map.ProcessOrder;
                            MapID.IsMasterData = map.IsMasterData;
                            MapID.connection_id = map.connection_id;
                            MapID.Type = map.Type;
                            MapID.IntergrationUuid = map.IntergrationUuid;

                            // MapID.uuid = map.uuid;

                            db.Update(MapID);
                            db.SaveChanges();
                            var updateLastSync = (from a in db.Zlastsync
                                                  join c in db.Zmapping on a.MappingId equals c.Id
                                                  where c.DocType == "CallZudelloMapping"
                                                  select a).FirstOrDefault();


                            string pr = String.Format(DateTime.UtcNow.ToString(), "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                            updateLastSync.LastSync = pr;
                          //  db.SaveChanges();
                        }
                        else

                        {

                            //add to mapping if it does not exist.
                            db.Zmapping.Add(AddtoMap);
                            db.SaveChanges();
                            Console.WriteLine(String.Format("Doc Type {0} has been added: Type {1} has been added", map.DocType, map.Type));
                            newMappings = true;



                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);//some error with mappings 

                        try
                        {
                            Zmapping AddtoMap = map;
                            db.Zmapping.Add(AddtoMap);
                            db.SaveChanges();
                            Console.WriteLine(String.Format("Doc Type {0} has been added: Type {1} has been added", map.DocType, map.Type));
                            newMappings = true;
                        }

                        catch (Exception exm)
                        {

                            Console.WriteLine(exm.Message);

                        }


                        finally
                        {

                            if (map.Type == "SQL_LITE_CMD")
                            {

                                Console.WriteLine(Tools.RunSQLLiteCmd(Token, map));

                            }


                        }


                    }
                    finally
                    {

                        if (map.Type == "SQL_LITE_CMD")
                        {

                            Console.WriteLine(Tools.RunSQLLiteCmd(Token, map));

                        }


                    }

                    db.DisposeAsync();

                }

                if (newMappings == true)
                {
                    try //Add into sync table if there are any new records
                    {
                        SetSyncTable().Wait();
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

        }

        public static string PostZudelloConnections(string token,string integration, string body)
        {
            string result = "";
            string website = "";
            string key = "";
            string apiurl;
            string ApiResult = "";
            string team = ZudelloInitalSettings.GetZudelloSettings().ZudelloTeam;
            string entity = "https://api.zudello.com/api/thinclient/";

            // work out API call

            apiurl = entity + integration + "/connections";

            // do the api call            

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = (HttpWebRequest)WebRequest.Create(apiurl);
            var data = Encoding.ASCII.GetBytes(body);

            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers.Add("x-team", team);
            request.Headers.Add("Authorization", "Bearer " + token);

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            if (response.StatusCode == HttpStatusCode.OK)
            {

                result = responseString;
            }
            else
            {
                result = responseString;

            }


            return result;

        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Text;
using ZudelloThinClient.Attache.AttacheODBC;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ServiceProcess;
using ZudelloApi;
using ZudelloThinClientLibary;

namespace ZudelloThinClient.Attache.AttacheHealthCheck
{
    public class HealthCheckSender
    { 
        public static void SendHealthReport ()
        {


            HealthData healthReport = new HealthData();

           
            //Check to see if health Report has any Kfi Errors with Failure, if so send these to Zudello
            var f = healthReport.data.queueHealthCheck.Where(i => i.status == "Failure").Count();

            if (f > 0)
            {
                ErrorFileSender.KfiErrorSender();
            }
          
                                                           


            string Token = ZudelloLogin.Login();
            Data QData = new Data();
            Output output = new Output();         
            QData.object_type = "HealthCheck";
            QData.data = healthReport;
            output.data = QData;
            string data = JsonConvert.SerializeObject(output);

            Console.WriteLine(data);           
            Console.WriteLine(ZudelloLogin.SendToZudelloQueue(Token, data));


        }
    
    
    }

 


    public class HealthData
    {
        public Health data { get; set; } = new Health();

    }

    public class Health
    {
        public string status { get; } = HealthCheckSync.ZudelloServiceStatus();
        public ConnectionUp odbcUp { get; } = HealthCheckSync.CheckOdbcConnection();
        public ConnectionUp sqlLiteUp { get; } = HealthCheckSync.CheckSqlLiteConnection();
        public List<HealthSync> lastSync { get; set; } = HealthCheckSync.HealthSyncCheck();
        public List<QueueHealth> queueHealthCheck { get; set; } = HealthCheckSync.QueueHealthCheck();

    }

    public class ConnectionUp
    { 
    
        public bool status { get; set; }
        public string exMessage { get; set; }
    
    }


    public class HealthSync : Zlastsync
    {

        public string type { get; set; }
        public string docType { get; set; }
        public string exMessage { get; set; }
        

    }

    public class QueueHealth 
    {
        public string status { get; set;}
        public int count { get; set; }
        public string exMessage { get; set; }

    }
  
        public class HealthCheckSync
    {      
      
        public static string ZudelloServiceStatus()
        {
            
            
           try
            {
                ServiceController sc = new ServiceController("ZudelloThinClientService");
            
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        return "Running";
                    case ServiceControllerStatus.Stopped:
                        return "Stopped";
                    case ServiceControllerStatus.Paused:
                        return "Paused";
                    case ServiceControllerStatus.StopPending:
                        return "Stopping";
                    case ServiceControllerStatus.StartPending:
                        return "Starting";
                    default:
                        return "Status Changing";
                }
            }
            catch(Exception ex)

            {
                return ex.Message;
            }

        }

        public static List<HealthSync> HealthSyncCheck()
        {
            List<HealthSync> hs = new List<HealthSync>();
            try
            {
                using (var db = new ZudelloContext())
                {
                    var queueStatus = (from p in db.Zlastsync
                                       join c in db.Zmapping on p.MappingId equals c.Id
                                       select new { p, c }).ToList();

                    foreach (var status in queueStatus)
                    {
                        try
                        {
                            HealthSync hsObj = new HealthSync();
                            hsObj.docType = status.c.DocType;
                            hsObj.type = status.c.Type;
                            hsObj.LastSync = status.p.LastSync;
                            hsObj.lastID = status.p.lastID;
                            hs.Add(hsObj);

                        }
                        catch 
                        {
                        
                        }
                    }

                    return hs;

                }
            }

            catch (Exception ex)
            {

                HealthSync hsObj = new HealthSync();
                hsObj.exMessage = ex.Message;
                return hs;

            }


        }




        public static List<QueueHealth> QueueHealthCheck()
        {
            List<QueueHealth> qh = new List<QueueHealth>();
           
            try
            {
                using (var db = new ZudelloContext())
                {
                    var queueStatus = from p in db.Zqueue
                                      group p by p.Status
                                      into
                                      g
                                      select new {status = g.Key, count = g.Count() };
                    foreach (var status in queueStatus)
                    {
                        QueueHealth statusObj = new QueueHealth();

                        

                        statusObj.status = status.status;
                        statusObj.count = status.count;
                        qh.Add(statusObj);                      

                    }

                    return qh;

                }
            }

            catch (Exception ex)
            {
                QueueHealth statusObj = new QueueHealth();
                statusObj.exMessage = ex.Message;
                qh.Add(statusObj);                
                return qh;

            }



        }

        public static ConnectionUp CheckSqlLiteConnection()
        {

            ConnectionUp up = new ConnectionUp();
            try
            {
                using (var db = new ZudelloContext())
                {
                    db.Database.CanConnect();
                    up.status = true;       // status.Add(true, "Connection Up");
                    return up;
                }
            }
            catch (Exception ex)
            {
                up.status = false;
                up.exMessage = ex.Message;
                return up;
            }

        }

        public static ConnectionUp CheckOdbcConnection()
        {

            ConnectionUp up = new ConnectionUp();
            //Dictionary<bool, string> status = new Dictionary<bool, string>();
            AttacheODBCconnection AttacheOdbc = new AttacheODBCconnection();
            using (OdbcConnection myConnection = new OdbcConnection())
            {

                myConnection.ConnectionString = AttacheOdbc.ConnectionString;
                try
                {
                    myConnection.Open();
                    myConnection.Close();

                    up.status = true;
                   // status.Add(true, "Connection Up");
                    return up;
                }
                catch (Exception ex)
                {
                    up.status = false;
                    up.exMessage = ex.Message;//status.Add(false, ex.Message);
                    return up;

                }


            }

        }

    }
}

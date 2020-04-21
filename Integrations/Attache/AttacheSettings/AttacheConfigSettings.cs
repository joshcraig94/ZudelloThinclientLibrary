using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using ZudelloApi;
using ZudelloThinClientLibary;

namespace ZudelloThinClient.Attache.AttacheSettings
{

    public class ZudelloCredentials
    {


    }
        public class ZudelloSettings
    {
        public string ZudelloSqlLite { get; set; }
        public string ZudelloEmail { get; set; }
        public string ZudelloPassword { get; set; }
        public string ZudelloPlatform { get; set; }
        public string ZudelloTeam { get; set; }
        public string ZudelloWebsite { get; set; }
        public string ZudelloMappingEndpoint { get; set; }
        public string ZudelloInvoiceEndpoint { get; set; }
        public string ZudelloConnectionUuid { get; set; }
        public string ZudelloQueue { get; set; }
        public string dollarMultiplier { get; set; }

    }


    public class AttacheConfiguration 
    {
        public string DNS { get; set; }
        public string Uid { get; set; }
        public string pwd { get; set; }
        public string AttacheInbox { get; set; }
        public string[] AttacheMonitor { get; set; }
    /*    public string AttacheHold { get; set; }
        public string AttacheNotProcessed { get; set; }
        public string AttacheSucess { get; set; } */


    }

}

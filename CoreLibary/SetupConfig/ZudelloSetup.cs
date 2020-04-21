using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZudelloApi;

namespace ZudelloThinClientLibary.SetupConfig
{
    public class ZudelloInitalSettings
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
        public string dataSource {get; set;}
        public string initialCatalog { get; set; }
        public string userId { get; set; }
        public string password { get; set; }

    #warning will need to refactor this  
        public string intergrationType { get; set; }


        public static ZudelloInitalSettings GetZudelloSettings()
        {
            ZudelloInitalSettings config = new ZudelloInitalSettings();
            using (var db = new ZudelloContext())
            {
                var myConfig = db.Zsettings.Where(s => s.Key == "ZudelloSettings").FirstOrDefault();

                ZudelloInitalSettings C = JsonConvert.DeserializeObject<ZudelloInitalSettings>(myConfig.Value);
                config = C;
            }
            return config;
        }

        public static ConnectionUuid GetZudelloQueueConnection()
        {
            ConnectionUuid config = new ConnectionUuid();
            using (var db = new ZudelloContext())
            {
                var myConfig = db.Zsettings.Where(s => s.Key == "ZudelloConnectionUuid").FirstOrDefault();

                ConnectionUuid C = JsonConvert.DeserializeObject<ConnectionUuid>(myConfig.Value);
                config = C;
            }
            return config;
        }

        public static FirstLoginBody ZudelloCredetials()
        {
            FirstLoginBody config = new FirstLoginBody();

            using (var db = new ZudelloContext())
            {
                var myConfig = db.Zsettings.Where(s => s.Key == "ZudelloCredetials").FirstOrDefault();

                FirstLoginBody C = JsonConvert.DeserializeObject<FirstLoginBody>(myConfig.Value);
                config = C;
            }



            return config;
        }


    }
}

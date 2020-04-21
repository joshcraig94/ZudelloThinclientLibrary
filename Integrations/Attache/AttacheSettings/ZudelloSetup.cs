using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using ZudelloApi;
//using ZudelloThinClient.SQLite;
using System.Configuration;
//using SetupConfig;
using ZudelloThinClientLibary;



namespace ZudelloThinClient.Attache.AttacheSettings
{
   public class ZudelloSetup 
    {

   

        public static AttacheConfiguration GetAttacheSettings()
        {
            AttacheConfiguration config = new AttacheConfiguration();
            using (var db = new ZudelloContext())
            {
                var myConfig = db.Zsettings.Where(s => s.Key == "AttacheSettings").FirstOrDefault();

                AttacheConfiguration C = JsonConvert.DeserializeObject<AttacheConfiguration>(myConfig.Value);
                config = C;
            }
            return config;
        }

     


    }




}


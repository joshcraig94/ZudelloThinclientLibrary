using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using ZudelloThinClient.Attache.AttacheODBC;
//using System.Data.Odbc;
//using ZudelloThinClient.Attache.AttacheSettings;
using System.Data;
//using ZudelloThinClient.Attache;
using System.Dynamic;
using ZudelloThinClientLibary;
using System.Globalization;
using ZudelloThinClientLibary.SetupConfig;

//using ZudelloThinClient.SetupConfig;


#warning Need to Migrate the Scriban Obj for Attache Receipting
namespace ZudelloApi
{
    /// <summary>
    /// how to nest line items now. 
    /// </summary>
    /// 

    public class MyScribianFunctions : ScriptObject
    {

#warning Add in object to loop through

        public static string connection(int connection_id)
        {
            //Method used to return the queue string
            try
            {
                using (var db = new ZudelloContext())
                {
                    var connectionID = db.Zconnections.Where(s => s.Id == connection_id).FirstOrDefault();

                    return "'" + connectionID.ConnectionUuid.ToString() + "'";
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "was an Issue with connection ID";
            }



        }

        public static string fetchconnection(int connection_id)
        {
            //Method used to return the queue string
            try
            {
                using (var db = new ZudelloContext())
                {
                    var connectionID = db.Zconnections.Where(s => s.Id == connection_id).FirstOrDefault();

                    return "'" + connectionID.ConnectionUuid.ToString() + "'";
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "was an Issue with connection ID";
            }



        }





        public static string LastSync(int mappingId, string pattern = "yyyy-MM-dd HH:mm:ss")
        {
            try
            {
                using (var db = new ZudelloContext())
                {
                    
                    var lastSync = db.Zlastsync.Where(s => s.MappingId == mappingId).FirstOrDefault();
                    DateTime dateTime = DateTime.Parse(lastSync.LastSync);

                    return "'" + dateTime.ToString(pattern) + "'";
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "was an Issue with last Sync";
            }

        }  





        public static string FormatDate(string dateValue, string pattern)
        {
            try
            {
                string s = dateValue;
                var date = DateTime.ParseExact(s, pattern,
                                                   CultureInfo.InvariantCulture);

                /*   DateTime parsed;
               DateTime.TryParse(dateValue, out parsed);
               string date = parsed.ToString(pattern);
                   return date; */
                return date.ToString();
            }
            catch (Exception ex)

            {             
                return "error trying to Parse Date";
            }

        }

        public static string value(dynamic value, int rounding = 4)
        {

            double multipler = 0.00;
            string SettingsMultiplier = ZudelloInitalSettings.GetZudelloSettings().dollarMultiplier;
            Double.TryParse(SettingsMultiplier, out multipler);
            double convertedFromCents = Math.Round(Convert.ToDouble(value)/ multipler, rounding);
            string myReturn = convertedFromCents.ToString();
            return myReturn;
        }
        public static string NormalizeLength(string value, int Max)
        {
            if (string.IsNullOrEmpty(value)) return value;
            string myVal = value.Length <= Max ? value : value.Substring(0, Max);
            return $"{(string.Join(",", myVal))}";
        }


        public static string RemoveSpaces(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            string myVal = string.Concat(value.Where(c => !char.IsWhiteSpace(c)));
            return myVal;
        }
        
        
       
    }

  
    public static class StringExtension
    {
      
        public static string RemoveEmptyTags(this string xml)
        {
         //   return new string(Regex.Replace(xml, @"<(\w+)></\1>", ""));
            string myString = (Regex.Replace(xml, @"<(\w+)></\1>", ""));
            return myString;

        }
    
     
    }


}
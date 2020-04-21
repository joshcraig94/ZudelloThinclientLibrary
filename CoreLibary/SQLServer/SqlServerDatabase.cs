using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using ZudelloThinClientLibary;

namespace ZudelloThinClientLibary
{
    public class SQLCredentials
    {

        //Gets all Table Structure and converts to JSON to Post to Zudello End Point
        public Dictionary<int, string>  ConnectionStringBuilder()

        {
            Dictionary<int,string> ConnectionStringList = new Dictionary<int, string>();

            using (var db = new ZudelloContext())
            {
                var conList = db.Zconnections.ToList();

                foreach (var c in conList)
                {
                    string connectionString = "";
               
                    // If connection is to use Integrated Security
                    if (c.UseIS == 1)
                    {                     
                        connectionString = String.Format("Data Source = {0}; Initial Catalog={1}; Integrated Security=True;", c.DataSource, c.InitialCatalog);
                    }

                    else
                    {
                         connectionString = String.Format("Data Source = {0}; Initial Catalog={1}; User id={2}; Password={3};", c.DataSource, c.InitialCatalog, c.UserId, c.Password);
                    }

                    ConnectionStringList.Add(c.Id,connectionString);

                }   


                return ConnectionStringList;

            }




               
            
       }


#warning Make sure Settings object can actually pull these down



        //Datasource should come from mappings as it can change per document 
        public string DataSource { get; set; } 

        //intital catalog can also be overridded and should come from the mappings as the database name will be different
        public string InitialCatalog { get; set;}

        //User ID and Password should be the same but if across multiple servers then will need to update the SQL lite database to store these credetials. 
        public  string UserId { get; set; }
        public string Password { get; set; }



    }
}



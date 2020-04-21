using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using ZudelloApi;
using MyobExoConnector;

namespace MyobExoConnector.EXO
{
   /* class ExoDatabaseLoader
    {
        //Class to load all databases and create zudello connections. 

        public static void GetDatadases()
        { 

        string Token = ZudelloLogin.Login();



        SQLCredentials ConnectionString = new SQLCredentials();
        Dictionary<int, string> Connection = ConnectionString.ConnectionStringBuilder();      


        string cmd = "Select * from SYS.";

        //Open SQL connection and run the SQL query 
       
        SqlConnection con = new SqlConnection(Connection[Mappings.Value.connection_id]);
        SqlCommand SelectCommand = new SqlCommand(cmd, con);
        // SqlDataReader myreader;
        con.Open();

                var jsonResult = new StringBuilder();

                var myreader = SelectCommand.ExecuteReader();

                if (!myreader.HasRows)
                {
                    //if there is no data then close connection and next loop
                    con.Close();
                    
                }
                else
                {
                    while (myreader.Read())
                    {
                        jsonResult.Append(myreader.GetValue(0).ToString());
                    
                    }

                   dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(jsonResult.ToString());

                    foreach (var ObjectType in obj)

                    {
                        foreach(var dataValues in ObjectType.Value)
                        {

                        MyQueueObject dataWrapper = new MyQueueObject();

                        dataWrapper.data = dataValues;
                        string data = JsonConvert.SerializeObject(dataWrapper);
                        Console.WriteLine(data);
                        Console.WriteLine(ZudelloLogin.SendToZudelloQueue(Token, data));
                         
                        }
                    }


                    

                    //Update last sync time for this mapping ID 
                    using (var db = new ZudelloContext())
                    {
                        var lastSync = db.Zlastsync.Where(s => s.MappingId == Mappings.Value.Id).FirstOrDefault();

                        lastSync.LastSync = DateTime.Now.ToString();
                        db.SaveChanges();
                    }

                }


                
           
               
               
               con.Close();

            }

        }

  
    } */
}

using Newtonsoft.Json;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZudelloApi;
using ZudelloThinClient.Attache.AttacheODBC;
using ZudelloThinClientLibary;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Reflection;
using ZudelloThinClient.Attache.AttacheSettings;
using System.Threading.Tasks;

namespace ZudelloThinClient.Attache
{




    public class AttacheFetchData
    {

        public static Dictionary<int, Zmapping> getBody()
        {


            Dictionary<int, Zmapping> myMappings = new Dictionary<int, Zmapping>();
            using (var db = new ZudelloContext())
            {
                var MapID = db.Zmapping.Where(i => i.IsOutgoing == 1).OrderBy(k => k.ProcessOrder).ToList();

                foreach (var mappings in MapID)
                {
                    Zmapping Data = new Zmapping()
                    {
                        DocType = mappings.DocType,
                        Body = mappings.Body,
                        Section = mappings.Section,
                        Id = mappings.Id
                    };


                    myMappings.Add(mappings.Id, Data);
                }

                return myMappings;
            }

        }


        public static async Task PushDataToZudello()
        {

            string Token = ZudelloLogin.Login();

            foreach (var Mappings in getBody())
            {
                string myReplacement = "";
                string myObjType = Mappings.Value.DocType;
                dynamic myObj = JsonConvert.DeserializeObject<ExpandoObject>(Mappings.Value.Body);
                string myLinkValue = myObj.LINK.ToString();
                string myQuery = myObj.HDR_QUERY.ToString();
                
               
                if (myQuery.Contains("{LAST_ID}"))
                {
                    try
                    {

                        using (var db = new ZudelloContext())
                        {


                            var lastSyncValue = (from a in db.Zlastsync
                                                 join c in db.Zmapping on a.MappingId equals c.Id
                                                 where c.Id == Mappings.Value.Id
                                                 select a).FirstOrDefault();


                            myQuery = myQuery.Replace("{LAST_ID}", lastSyncValue.lastID.ToString());


                        }

                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                
                }

                if (myQuery.Contains("{LAST_SYNC}"))
                {
                    try
                    {

                        using (var db = new ZudelloContext())
                        {


                            var lastSyncValue = (from a in db.Zlastsync
                                                 join c in db.Zmapping on a.MappingId equals c.Id
                                                 where c.Id == Mappings.Value.Id
                                                 select a).FirstOrDefault();

                            DateTime MyQueryDate = DateTime.Parse(lastSyncValue.LastSync);
                            myQuery = myQuery.Replace("{LAST_SYNC}", MyQueryDate.ToString("yyyy-MM-dd HHmmss"));


                        }

                    }
                    catch
                    {

                    }



                }



                var headerDs = AttacheFetchData.GetDataSet(myQuery);

                try
                {
                     myReplacement = myObj.REPLACE_ME.ToString();
                }
                catch
                { 
                }
                 
                bool LineQuery = false;
                if (myLinkValue != "")
                {
                    LineQuery = true;
                }
                
                foreach (DataTable dt in headerDs.Tables)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                  
                        Data QData = new Data();
                        Output output = new Output();

                        QData.object_type = myObjType;


                        //dynamic obj = new DynamicClass(fields);

                        Dictionary<string, object> temp = new Dictionary<string, object>();

                        foreach (DataColumn dc in dt.Columns)
                        {
                            dynamic Colvalue;
                            if (dr[dc.ColumnName] is String)
                            {
                                Colvalue = dr[dc.ColumnName].ToString();
                                Colvalue = Colvalue.Trim(); 
                                
                            }

                            else
                            {
                                Colvalue = dr[dc.ColumnName];
                            }

                            temp.Add(dc.ColumnName, Colvalue);
                            
                        }
                        QData.data = temp;
                        output.data = QData;
                        if (LineQuery == true)
                        {
                            var query = temp[myLinkValue];
                            // output.data.data = temp;
                            string myQuery2 = myObj.LINE_QUERY.ToString().Replace("{0}", query.ToString());
                            var lineDs = GetDataSet(myQuery2);
                            List<object> mArray = new List<object>();
                            foreach (DataTable lt in lineDs.Tables)
                            {

                                foreach (DataRow lr in lt.Rows)
                                {

                                    Dictionary<string, object> lstemp = new Dictionary<string, object>();
                                    foreach (DataColumn lc in lt.Columns)
                                    {
                                        dynamic lineValue;
                                        if (lr[lc.ColumnName] is String)
                                        {
                                            lineValue = lr[lc.ColumnName].ToString();
                                            lineValue = lineValue.Trim();

                                        }

                                        else
                                        {
                                            lineValue = lr[lc.ColumnName];
                                        }
                                        lstemp.Add(lc.ColumnName, lineValue);

                                    }

                                    mArray.Add(lstemp);                                   

                                }
                            }
                            Dictionary<int, object> myDic = new Dictionary<int, object>();
                          
                            object REPLACEME = new object();
                            string data = JsonConvert.SerializeObject(output);
                            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(data);
                            obj.data.data.REPLACEME = mArray;
                            string lineData = JsonConvert.SerializeObject(obj);
                            lineData = lineData.Replace("REPLACEME", myReplacement);
                            Console.WriteLine(lineData);

                            //If this record already exisits in the hashing table then continue with the loop.
                            if (HashingRecords.Hash(lineData, Mappings.Value.Id) == true) continue;                                                     
                          

                                                       
                            Console.WriteLine(ZudelloLogin.SendToZudelloQueue(Token, lineData));
                            using (var db = new ZudelloContext())
                            {


                                var updateLastSync = (from a in db.Zlastsync
                                                      join c in db.Zmapping on a.MappingId equals c.Id
                                                      where c.Id == Mappings.Value.Id
                                                      select a).FirstOrDefault();

                                updateLastSync.LastSync = DateTime.UtcNow.ToString();
                                try
                                {
                                    int myInternalId = Convert.ToInt32(temp[myLinkValue]);
                                    updateLastSync.lastID = myInternalId;


                                }
                                catch
                                {

                                }
                                db.SaveChanges();


                            }




                        }
                        else
                        {
                    
                            string data = JsonConvert.SerializeObject(output);
                            Console.WriteLine(data);
                            //If this record already exisits in the hashing table then continue with the loop.
                            if (HashingRecords.Hash(data, Mappings.Value.Id) == true) continue;
                            Console.WriteLine(ZudelloLogin.SendToZudelloQueue(Token, data));
                           
                            using (var db = new ZudelloContext())
                            {

                                var updateLastSync = (from a in db.Zlastsync
                                                      join c in db.Zmapping on a.MappingId equals c.Id
                                                      where c.Id == Mappings.Value.Id
                                                      select a).FirstOrDefault();

                                updateLastSync.LastSync = DateTime.UtcNow.ToString();
                                


                                db.SaveChanges();

                            }


                        }                      
                        
                                        

                    }
                }  
                                

            }
        }        

        public static DataSet GetDataSet(string myQuery = "")
        {

                   
            
            AttacheODBCconnection AttacheOdbc = new AttacheODBCconnection();
            using (OdbcConnection myConnection = new OdbcConnection())
            {

                myConnection.ConnectionString = AttacheOdbc.ConnectionString;
                myConnection.Open();
                using (OdbcCommand command = new OdbcCommand(myQuery, myConnection))
                {
                   
                   
                    using (var dataSet = new DataSet())
                    {try
                        {

                          

                            dataSet.Locale = System.Globalization.CultureInfo.InvariantCulture;
                            var db2SerialDt = new OdbcDataAdapter(myQuery, myConnection);
                            command.CommandText = myQuery;
                            db2SerialDt.Fill(dataSet);
                            return dataSet;
                        }

                        catch(Exception ex)
                        {
                            return dataSet;
                        }
                    };

                }

            }

        }
    }
}



using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZudelloApi;

namespace ZudelloThinClientLibary.SQLite
{
    public class Tools
    {

        public static void SaveToDB(string kfiName, string jsonBody)
        {

            using (var db = new ZudelloContext())
            {

                //db.Files.Add()
                //  db.Zfiles.Add(new Zfiles { JsonData = jsonBody });
                db.SaveChanges();

            }




        }

        public static bool CheckForMasterData()
        {
            bool masterData = false;
            using (var db = new ZudelloContext())
            {


                //var query = db.Zqueue.Where(a => a.Ztype > 10 && a.Sucess == 0 || a.Sucess == null).Count();
                var query = 1;

                if (query > 0)
                {
                    masterData = true;
                }


                return masterData;
            }

        }

  /* UPDATE ZLASTSYNC set lastSync = "23/01/2020 4:49:41 AM"
where Mapping_ID not in (SELECT ID FROM ZMAPPING WHERE TYPE IN ("API") );
DELETE FROM ZQUEUE;
*/
        public static bool RunSQLLiteCmd(string token, Zmapping cmd)
        {          
            
            using (var db = new ZudelloContext())
            {

                try
                {
                    //Exceute the custom command
                    db.Database.ExecuteSqlCommand(cmd.Body);
                    db.SaveChanges();

                    //Remove custom command from database FK  
                    var lastSyncQuery = db.Zlastsync.Where(z => z.MappingId == cmd.Id).FirstOrDefault();
                    var mappingTbl = db.Zmapping.Where(z => z.uuid == cmd.uuid);

                    foreach(Zmapping sqlQuery in mappingTbl)
                    { 
                    //Dont need to remove as its never added into sync table
                    // db.Remove(lastSyncQuery);
                    // db.SaveChanges();
                    db.Remove(sqlQuery);
                    db.SaveChanges();
                    //Send delete request to Zudello to remove from mappings table                  
                    Console.WriteLine(ZudelloLogin.DeleteMapping(token, cmd.uuid));
         
                    
                    }
                    return true;
                }

                catch (Exception ex)
                {


                    return false;
                
                }



            }


        }
    }
}



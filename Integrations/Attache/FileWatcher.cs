using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using Scriban;
using ZudelloThinClient.Attache;
using System.Configuration;
using ZudelloApi;
using ZudelloApi;
using System.Dynamic;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using ZudelloThinClient.Attache.AttacheSettings;
using ZudelloThinClient;
using ZudelloThinClientLibary;

namespace FileWatcherService.ZudelloApi
{
  

    public class MyMonitor
    {
     
        public static void watchFiles()
        {

           AttacheConfiguration folders = ZudelloSetup.GetAttacheSettings();
            
            FileSystemWatcher watcher = new FileSystemWatcher();
            string filePath = folders.AttacheInbox;
            watcher.Path = filePath;

            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.NotifyFilter = NotifyFilters.FileName;
            watcher.Filter = "*.*";

            // will track changes in sub-folders as well
            watcher.IncludeSubdirectories = true;            
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            new System.Threading.AutoResetEvent(true).WaitOne();
            watcher.EnableRaisingEvents = true;


        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
           // AttacheConfiguration folders = ZudelloSetup.GetAttacheSettings();
            FolderType(e);
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);

        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
           // FolderType(e);
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);          
        }


        public static void FolderType(FileSystemEventArgs e)
        {

            AttacheConfiguration FolderNames = ZudelloSetup.GetAttacheSettings();
            string status = "";
            foreach (var folder in FolderNames.AttacheMonitor.ToArray())
            {
                if (e.FullPath.Contains(folder)) status = folder;
            }
                //Get QueueID to update SQL
                int index1 = e.FullPath.LastIndexOf('_');
            try
            {
                string queueId = Regex.Match(e.FullPath.Substring(index1), @"\d+").Value;
                if (updateQueue(queueId, status) == true)
                {
                    Console.WriteLine("Queue ID: {0} has been updated", queueId);
                }
            }

            catch
            { 
            }

        }


        public static bool updateQueue(string queueId, string status)
        {
            try
            {
                int id = 0;

                if (Int32.TryParse(queueId, out id))
                {
                    using (var db = new ZudelloContext())
                    {
                        var queueUpdate = db.Zqueue.Where(i => i.Id == id).FirstOrDefault();
                        queueUpdate.Status = status;
                        db.SaveChanges();
                        return true;
                        
                    }

                }
                return false;
            }

            catch (Exception ex)
            {

                return false;
            }


        }

    }



}
       

   

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZudelloThinClientLibary;
using ZudelloApi;
using Microsoft.Dynamics.GP.eConnect;
using System.Xml;
using MicrosoftGPConnector.Testing;


namespace MicrosoftGPConnector
{
    class Program
    {
        static void Main()
        {
            try
            {
                Console.WriteLine("Data Added To Queue started");
                GpQueue.AddtoQueue();
                Console.WriteLine("Data Added To Queue finished");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }


            try
            {
                Console.WriteLine("Push Data Started");
                GpFetchData.PushGpDataToZudello();
                Console.WriteLine("Push Data Finished");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }


            try
            {
                Console.WriteLine("Process Records stated");
                GpProcess.ProcessRecords();
                Console.WriteLine("Process Records Finished");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }

    

    }
}

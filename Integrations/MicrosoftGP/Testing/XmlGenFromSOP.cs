using Microsoft.Dynamics.GP.eConnect;
using Microsoft.Dynamics.GP.eConnect.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MicrosoftGPConnector.Testing
{
    class XmlGenFromSOP
    {
        public static void MySer()
        {
            using (eConnectMethods eConCall = new eConnectMethods())
            {
                try
                {
                    // Create the eConnect document and store it in a file
                    SerializeObject("SalesOrder.xml");

                    // Load the eConnect XML document from the file into 
                    // and XML document object
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load("SalesOrder.xml");

                   
                    string salesOrderDocument = xmldoc.OuterXml;

                    
                    string sConnectionString = @"Data Source=LAPTOP-BUDQ9SBN\DYNAMICGP;Integrated Security = SSPI; Persist Security Info = false ; Initial Catalog = GP_DE;";

                  
                    string salesOrder = eConCall.CreateTransactionEntity(sConnectionString, salesOrderDocument);

                    Console.WriteLine(salesOrder);
                    Console.ReadLine();
                }
            
                catch (eConnectException exp)
                {
                    Console.Write(exp.ToString());
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                    Console.ReadLine();
                }
                finally
                {
                    eConCall.Dispose();
                }
            }
        }
        private static void SerializeObject(string filename)
        {
            // Create a datetime format object
            DateTimeFormatInfo dateFormat = new CultureInfo("en-US").DateTimeFormat;

            try
            {

                taPMTransactionInsert PurchInvoice = new taPMTransactionInsert();

                PurchInvoice.BACHNUMB = "SAVE";
                PurchInvoice.VENDORID = "ACETRAVE0001";
                PurchInvoice.DOCAMNT = 500m;
                PurchInvoice.DOCNUMBR = "15444445";
                PurchInvoice.DOCTYPE = 1;
                PurchInvoice.VCHNUMWK = "8914985";
                PurchInvoice.DOCDATE = "01/06/2017"; //System.DateTime.Today.ToString("MM/dd/yyyy", dateFormat);
                PurchInvoice.PRCHAMNT = 500m;
                PurchInvoice.MSCCHAMT = 0;
                PurchInvoice.TAXAMNT = 0;
                PurchInvoice.FRTAMNT = 0;
                PurchInvoice.TRDISAMT = 0;
                PurchInvoice.CHRGAMNT = 500m;
                PurchInvoice.CREATEDIST = 1;

              //  PurchInvoice.DISAMTAV = 0;


                taPMDistribution_ItemsTaPMDistribution[] LineItems = new taPMDistribution_ItemsTaPMDistribution[2];


                taPMDistribution_ItemsTaPMDistribution invLines = new taPMDistribution_ItemsTaPMDistribution();

                invLines.DOCTYPE = 1;
               // invLines.ACTNUMST = "000-1410-00";
                invLines.DEBITAMT = 500m;
                invLines.DISTTYPE = 6;
                invLines.VCHRNMBR = PurchInvoice.VCHNUMWK;
                invLines.VENDORID = PurchInvoice.VENDORID;
                invLines.ACTINDX = 10;

                LineItems[0] = invLines;

                invLines = new taPMDistribution_ItemsTaPMDistribution();
                invLines.DOCTYPE = 1;
                invLines.DISTTYPE = 2;
                invLines.VCHRNMBR = PurchInvoice.VCHNUMWK;
                invLines.VENDORID = PurchInvoice.VENDORID;
                invLines.CRDTAMNT = PurchInvoice.DOCAMNT;
                invLines.ACTINDX = 35;
                //Reset the lines


                LineItems[1] = invLines;

                PMTransactionType crInvoice = new PMTransactionType();

                crInvoice.taPMTransactionInsert = PurchInvoice;
                crInvoice.taPMDistribution_Items = LineItems;

                PMTransactionType[] myPMTrancationType = { crInvoice };

                eConnectType eConnect = new eConnectType();
                eConnect.PMTransactionType = myPMTrancationType;
                FileStream fs = new FileStream(filename, FileMode.Create);
                XmlTextWriter writer = new XmlTextWriter(fs, new UTF8Encoding());

                // Serialize the eConnect document object to the file using the XmlTextWriter.
                XmlSerializer serializer = new XmlSerializer(eConnect.GetType());
                serializer.Serialize(writer, eConnect);
                writer.Close();

            }
            //If an eConnect exception occurs, notify the user
            catch (eConnectException ex)
            {
                Console.Write(ex.ToString());
            }
        }
    }

  

}

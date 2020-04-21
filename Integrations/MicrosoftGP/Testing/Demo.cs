using Microsoft.Dynamics.GP.eConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MicrosoftGPConnector.Testing
{
    class Demo
    {

        public static void GP()
        {
            string econnectDocument;
            string sXsdSchema;
            string sConnectionString;
            


                //Generate the eConnectXml
                string xmlRendered = @"PM_Transaction-Invoice.xml";
                using (eConnectMethods e = new eConnectMethods())
                {
                    try
                    {

                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.Load(xmlRendered);
                        econnectDocument = xmldoc.OuterXml;

                        //User ID=sa;Password=sa
                        sConnectionString = @"Data Source=LAPTOP-BUDQ9SBN\DYNAMICGP;Integrated Security = SSPI; Persist Security Info = false ; Initial Catalog = GP_DE;";
                        // Create an XML Document object for the schema
                        XmlDocument XsdDoc = new XmlDocument();

                        // Create a string representing the eConnect schema
                        sXsdSchema = XsdDoc.OuterXml;

                        // Pass in xsdSchema to validate against.
                         bool response =  e.CreateEntity(sConnectionString, econnectDocument);
                        Console.WriteLine(response);
                        Console.ReadLine();
                    }

                    // The eConnectException class will catch eConnect business logic errors.
                    // display the error message on the console
                    catch (eConnectException exc)
                    {
                        Console.Write(exc.ToString());
                        e.Dispose();


                    }
                    // Catch any system error that might occurr.
                    // display the error message on the console
                    catch (System.Exception ex)
                    {
                        Console.Write(ex.ToString());
                        e.Dispose();


                    }
                }
            
        }
    }

}
            
  
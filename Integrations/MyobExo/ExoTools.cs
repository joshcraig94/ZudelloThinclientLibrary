using Newtonsoft.Json;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ZudelloApi;
using System.Web;
using ZudelloThinClientLibary;
using System.IO;

namespace MyobExoConnector.EXO
{
    public class ExoTools
    {
        public static string RenderToSql(string kfiTemplate, object obj = null)
        {

            var scribanObject = new ScriptObject();
            scribanObject.Import(typeof(MyScribianFunctions));

            scribanObject.Import("z", new Func<dynamic>(() => obj));

            var context = new TemplateContext();
            context.PushGlobal(scribanObject);
            var template = Template.Parse(kfiTemplate);
            if (template.HasErrors)
                throw new Exception(string.Join("\n",
                    template.Messages.Select(x => $"{x.Message} at {x.Span.ToStringSimple()}")));

            string ToFormat = template.Render(context);
            return ToFormat;
        }


        public static string ZudelloXMLConverter(string xml)
        {
            XmlDocument doc = new XmlDocument();
            string jsonStringBuilder = "";// new StringBuilder();
            string jsonText = "";
            string decoded = "";
            //  xml = File.ReadAllText(@"Text2.xml");
          //  xml = (File.ReadAllText(@"TextFile1.xml"));
            
            Console.WriteLine(xml);
            doc.LoadXml(xml);

            StringBuilder masterXml = new StringBuilder();
            XmlNodeList nodelist = doc.DocumentElement.GetElementsByTagName("row");
            List<string> xmlBuilderList = new List<string>();

            foreach (XmlNode node in nodelist)
            {                      
             
                StringBuilder xmlBuilder = new StringBuilder();
           //    if (nodelist.Count <= 1)
            //    {
            //        break;
            //    }

                XmlDocument convertedDoc = new XmlDocument();
                
                foreach (XmlNode childNode in node.ChildNodes)
                {
                   
                    
                    if (childNode.OuterXml.Contains("&lt;row"))
                    {
                       
                        string removedRows = HttpUtility.HtmlDecode(childNode.OuterXml);

                        removedRows = removedRows.Replace(String.Format("</{0}>", childNode.Name), "");
                        removedRows = removedRows.Replace(String.Format("<{0}>", childNode.Name), "");
                        removedRows = removedRows.Replace("</row>", String.Format("</{0}>",childNode.Name));
                        removedRows = removedRows.Replace("<row>", String.Format("<{0}>", childNode.Name));

                        if (childNode.NextSibling == null)
                        {
                            xmlBuilder.Append(removedRows.Replace("data.", "") + "</data>");
                            continue;
                        }
                        else
                        {
                            xmlBuilder.Append(removedRows);
                            continue;
                        }


                    }

                    if (childNode.NextSibling == null)
                    {
                        xmlBuilder.Append(childNode.OuterXml.Replace("data.", "") + "</data>");
                        continue;
                    }

                    if(childNode.OuterXml.Contains("data."))
                    {
                          
                           xmlBuilder.Append(childNode.OuterXml.Replace("data.", ""));
                            
                    }

                    else

                    {
                        if(childNode.Name == "connection_uuid")
                        {
                                xmlBuilder.Append(childNode.OuterXml + "<data>");
                        }
                        else
                        { 
                        xmlBuilder.Append(childNode.OuterXml);
                       }

                    }



                  
                }

                try
                {
                     decoded = xmlBuilder.ToString();
                    convertedDoc.LoadXml("<data>" + decoded + "</data>");

                    xmlBuilderList.Add(JsonConvert.SerializeXmlNode(convertedDoc));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    try
                    {
                        convertedDoc.Load("<data>" + decoded + "</data>");
                    }
                    catch
                    { 
                    }
                }


            }
            //  string jsonText = JsonConvert.SerializeXmlNode(doc);


            jsonStringBuilder = string.Join(",", xmlBuilderList);

            jsonStringBuilder = HttpUtility.HtmlDecode(jsonStringBuilder);

            jsonStringBuilder = "{\"data\": [" + jsonStringBuilder + "]}";
            jsonText = jsonStringBuilder.ToString();
            jsonText = Regex.Unescape(jsonText);
            jsonText = jsonText.Replace(":\"[", ":[");
            jsonText = jsonText.Replace("]\"}", "]}");
            return jsonText;
        }

        private static void handleNode(XmlNode node)
        {
            if (node.HasChildNodes)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    handleNode(child);
                }
            }
            else
                Console.WriteLine(node.Name);
        }
    }

}


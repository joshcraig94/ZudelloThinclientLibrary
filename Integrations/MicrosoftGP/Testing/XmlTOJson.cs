using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MicrosoftGPConnector.Testing
{
    public class XmlToJson
    {
        public static string ZudelloXMLConverter()
        {

            string xml = File.ReadAllText(@"ser.xml");
            XmlDocument doc = new XmlDocument();
            xml = xml.Replace("&lt;", "<");
            xml = xml.Replace("&gt;", ">");
            xml = xml.Replace("&amp;amp;", @"/&"); //going to cause issues.
            xml = xml.Replace(@"<row>", "");
            xml = xml.Replace("</row>", "");
            xml = xml.Replace("data.", "");
            doc.LoadXml(xml);
            string jsonText = JsonConvert.SerializeXmlNode(doc);
            jsonText = Regex.Unescape(jsonText);
            jsonText = jsonText.Replace(":\"[", ":[");
            jsonText = jsonText.Replace("]\"}", "]}");
            return jsonText;
        }
    }
}


using Newtonsoft.Json;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZudelloApi;
using ZudelloThinClientLibary;

namespace MicrosoftGPConnector
{
    public class GpValidations
    {
        //Validation to check if 
        public bool CreateSupplier { get; set; } = false;
        public bool CreateInventory { get; set; } = false;
        public List<string> InventoryToCreate { get; set; } = new List<string>();
        public static GpValidations SupplierInvoiceValidate(string Supplierinvoice, GpMappingAndDatabase mapping)
        {

            dynamic validator = JsonConvert.DeserializeObject<ExpandoObject>(Supplierinvoice);
            GpValidations validate = new GpValidations ();

            string ExoCode = validator.document.supplier.code.ToString().ToUpper();

            SQLCredentials ConnectionString = new SQLCredentials();
            Dictionary<int, string> Connection = ConnectionString.ConnectionStringBuilder();


            string SQLQuery = String.Format("SELECT Accno FROM CR_ACCS where Name = '{0}' or Alphacode = '{0}'", ExoCode);


            try
            {
                SqlConnection con = new SqlConnection(Connection[mapping.Id]);
                // SqlDataReader myreader;
                con.Open();


                using (SqlCommand command = new SqlCommand(SQLQuery, con))
                {
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        //Do nothing Item Exists in Exo
                    }
                    else
                    {
                        validate.CreateSupplier = true;

                    }
                    reader.Close();


                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }


            List<string> SkuList = new List<string>();

            foreach (var line in validator.document.lines)
            {
                string stkItem = "";
                try { stkItem = line.item.sku.ToString().ToUpper(); }
                catch (Exception ex)
                {
                    // if the sku is null will thow runtime binding error, this is fine as it is a GL invoice.
                    Console.WriteLine(ex.Message + " GL invioce");
                    continue;

                }

                string selectItems = String.Format("Select stockcode from IV00101 where stockcode = '{0}'", stkItem);
                try
                {
                    SqlConnection con = new SqlConnection(Connection[mapping.Id]);
                    // SqlDataReader myreader;
                    con.Open();

                    //Get a list of all ther table names in the Database
                    using (SqlCommand command = new SqlCommand(selectItems, con))
                    {
                        var reader = command.ExecuteReader();
                        if (reader.Read())
                        {

                            //Do nothing Item Exists in Exo
                        }
                        else
                        {
                            validate.InventoryToCreate.Add(stkItem);

                        }

                    }

                }


                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }

            if (validate.InventoryToCreate.Count() > 0)
            {
                validate.CreateInventory = true;
            }

            return validate;



        }



    }


    public class GpTools
    {
       
            public static string RenderXml(string kfiTemplate, object obj = null)
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
    }
    
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyobExoConnector.EXO;
using System.Dynamic;
using ZudelloThinClientLibary;
using System.Data.SqlClient;

namespace MyobExoConnector.EXO
{
    public class Validations
    {
        //Validation to check if 
        public bool CreateSupplier { get; set; } = false;
        public bool CreateInventory { get; set; } = false;
        public List<string> InventoryToCreate { get; set; } = new List<string>();



        public static Validations ExoSupplierInvoice(string Supplierinvoice, ExoMappingAndDB mapping)
        {

            dynamic validator = JsonConvert.DeserializeObject<ExpandoObject>(Supplierinvoice);
            Validations validate = new Validations();
            string ExoCode = "";
            string SQLQuery = "";
            try
            {
                 ExoCode = validator.document.supplier.linked.accountNumber.ToString().ToUpper().Trim();
                 SQLQuery = String.Format("SELECT Accno FROM CR_ACCS where accno = '{0}'", ExoCode);
            }
            catch(Exception ex)
            {
                try
                {
                    ExoCode = validator.document.supplier.code;
                    SQLQuery = String.Format("SELECT X_ZCode FROM CR_ACCS where X_ZCode = '{0}'", ExoCode.ToUpper().Trim());
                }
                catch (Exception ex2)
                { 
                
                
                }
            }

            SQLCredentials ConnectionString = new SQLCredentials();
            Dictionary<int, string> Connection = ConnectionString.ConnectionStringBuilder();


         


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
       
                if (validator.document.docType.ToString() == "PURCHASING::EXPENSE")
                {  //Maybe need to revise in future               
                    break;
                }
                string stkItem = "";
                try { stkItem = line.item.sku.ToString().ToUpper(); }
                catch (Exception ex)
                {
                    // if the sku is null will thow runtime binding error, this is fine as it is a GL invoice.
                    Console.WriteLine(ex.Message + " GL invioce");
                    continue;

                }

                string selectItems = String.Format("Select stockcode from stock_items where stockcode = '{0}'", stkItem);
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
}



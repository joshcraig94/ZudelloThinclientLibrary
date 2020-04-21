using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using ZudelloThinClient.Attache.AttacheODBC;
using ZudelloThinClientLibary;

namespace AttacheValidation
{


    public class AttacheValidations
    {
        //Validation to check if 
        public bool CreateSupplier { get; set; } = false;
        public bool CreateInventory { get; set; } = false;
        public List<string> InventoryToCreate { get; set; } = new List<string>();

        public static AttacheValidations AttacheSupplierInvoice(string Supplierinvoice)
        {
#warning need to add in validation if items are already in queue to be created!  
            const string quote = "\"";
            dynamic validator = JsonConvert.DeserializeObject<ExpandoObject>(Supplierinvoice);
            AttacheValidations validate = new AttacheValidations();

            string attacheCode = validator.document.supplier.code.ToString().ToUpper();

            string cmd = String.Format("Select code from admin.supplier where code = '{0}'", attacheCode);
            try
            {
                AttacheODBCconnection AttacheOdbc = new AttacheODBCconnection();
                using (OdbcConnection myConnection = new OdbcConnection())
                {

                    myConnection.ConnectionString = AttacheOdbc.ConnectionString;
                    using (OdbcCommand command = new OdbcCommand(cmd, myConnection))
                    {
                        command.Connection.Open();
                        var reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            validate.CreateSupplier = false;
                        }
                        else
                        {
                            validate.CreateSupplier = true;
                        }

                    }
                }
            }

            catch (Exception ex)
            {



            }

            //Check Attache Linked Code 
            try
            {
                attacheCode = validator.document.supplier.linked.code.ToString().ToUpper();
                cmd = String.Format("Select code from admin.supplier where code = '{0}'", attacheCode);
                AttacheODBCconnection AttacheOdbc = new AttacheODBCconnection();
                using (OdbcConnection myConnection = new OdbcConnection())
                {

                    myConnection.ConnectionString = AttacheOdbc.ConnectionString;
                    using (OdbcCommand command = new OdbcCommand(cmd, myConnection))
                    {
                        command.Connection.Open();
                        var reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            validate.CreateSupplier = false;
                        }
                        else
                        {
                            validate.CreateSupplier = true;
                        }

                    }
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
                string selectItems = "";
                if (line.item.isStock == true)
                {
                    selectItems = String.Format("Select code from admin.product where code = '{0}'", stkItem);
                }
                else
                {
                    //Attache service codes select in differnt table 
                    selectItems = String.Format("Select code from admin.service where code = '{0}'", stkItem);

                }

                try
                {
                    AttacheODBCconnection AttacheOdbc = new AttacheODBCconnection();
                    using (OdbcConnection myConnection = new OdbcConnection())
                    {

                        myConnection.ConnectionString = AttacheOdbc.ConnectionString;
                        using (OdbcCommand command = new OdbcCommand(selectItems, myConnection))
                        {
                            command.Connection.Open();
                            var reader = command.ExecuteReader();
                            if (reader.Read())
                            {
                                //Do nothing Item Exists in Attache
                            }
                            else
                            {
                                validate.InventoryToCreate.Add(stkItem);

                            }

                        }
                    }
                }


                catch
                {


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








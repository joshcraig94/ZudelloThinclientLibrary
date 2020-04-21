using Newtonsoft.Json;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using ZudelloApi;
using ZudelloThinClientLibary;

namespace ZudelloThinClient.Attache
{
    class AttacheScribanExtension : ScriptObject
    {

        public class LineReceipting
        {
            public string internalNbr { get; set; }
            public string poNbr { get; set; }
            public string lineNbr { get; set; }
            public string receiptQty { get; set; }
            public string receiptSeq { get; set; } = ",,,0<F9>";


        }


        public static string OrderSelect(dynamic data, int type = 0)
        {
            //Dictionary<int, Zmapping> orders = AttacheFetchData.getBody();
            string query = "";
            using (var db = new ZudelloContext())
            {
                var body = db.Zmapping.Where(i => i.DocType == "PURCHASING::RECEIVE:QUERY").FirstOrDefault();
                query = body.Body;
            }


            dynamic myObj = JsonConvert.DeserializeObject<ExpandoObject>(query);
            string myQuery = ""; 
            string myLineQuery = ""; 

            if (type == 0)
            {
                myQuery = myObj.HDR_QUERY.ToString();
                myLineQuery = myObj.LINE_QUERY.ToString();
            }
            else
            {
                //Invoice line type required different query

                myQuery = myObj.HDR_QUERY_INVOICE.ToString();
                myLineQuery = myObj.LINE_QUERY_INVOICE.ToString();

            }

            
           
 
            // Loop through and get the data.Receive IDs. 
            Dictionary<int, LineReceipting> lineReceipting = new Dictionary<int, LineReceipting>();

            List<string> internalDocNbr = new List<string>();

            try
            {
                int i = 0;
                foreach (var id in data.document.lines)
                {

                    foreach (var nbr in id.receive)
                    {

                        LineReceipting LineData = new LineReceipting();
                        if (!internalDocNbr.Contains(nbr.remote_order_id.ToString()))
                        {
                            internalDocNbr.Add(nbr.remote_order_id.ToString());

                            LineData.internalNbr = nbr.remote_order_id.ToString();
                            LineData.lineNbr = nbr.line.ToString();
#warning Taken out for general testing 
                            //  LineData.poNbr = nbr.number.ToString();
                            LineData.receiptQty = nbr.qty.ToString();

                        }
                        i = i + 1;
                        lineReceipting.Add(i + 1, LineData);
                    }
                }
            }

            catch (Exception ex)

            {
                Console.WriteLine(ex.Message);

            }

            string accountCode = data.document.supplier.code.ToUpper();
            string poNbr = String.Join(", ", internalDocNbr.ToArray());


            //Hdr
            myQuery = myQuery.Replace("{po_number}", poNbr);
            myQuery = myQuery.Replace("{account_code}", accountCode);

            //Lines 

            myLineQuery = myLineQuery.Replace("{po_number}", poNbr);


            //Query to get HDR
            DataSet orderData = AttacheFetchData.GetDataSet(myQuery);
            List<string> Sequence = new List<string>();
            foreach (DataTable dt in orderData.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {


                    foreach (DataColumn dc in dt.Columns)
                    {

                        Sequence.Add(dr[dc.ColumnName].ToString());

                    }


                }


            }

            //Query to get lines

            DataSet lineOrderData = AttacheFetchData.GetDataSet(myLineQuery);
            // List<string> LineSequence = new List<string>();           


            //Might need to put this as its own Function where uesr can specific the sequence ie ,,,,,,,,,20,204, or extra. 
            List<object[]> MyLineSequence = new List<object[]>();

            foreach (DataTable dt in lineOrderData.Tables)
            {


                foreach (DataRow dr in dt.Rows)
                {
                    //Add to array 
                    MyLineSequence.Add(dr.ItemArray);

                }
            }

            StringBuilder lineSequence = new StringBuilder();
            foreach (var obj in MyLineSequence)
            {

                //Get the sequence to return, looking at dictionary to comapre.
                foreach (var dic in lineReceipting)
                {
                    if (dic.Value.internalNbr == obj[1].ToString() && dic.Value.lineNbr == obj[2].ToString())
                    {
                        lineSequence.Append(dic.Value.receiptSeq.Replace("0", dic.Value.receiptQty.ToString()));
                        //not sure if this is needed, but still works? 
                        lineReceipting.Remove(dic.Key);

                        break;

                    }

                    else
                    {
                        lineSequence.Append(dic.Value.receiptSeq);
                        break;

                    }


                }



            }



            //  Console.ReadLine();
            StringBuilder upSequence = new StringBuilder();
            int lineUp = MyLineSequence.Count();

            for (int i = 0; i < lineUp; i++)
            {
                upSequence.Append("<UP>");
            }




            // string lineValues //Maybe walk object in scribian
            //or here need to now use  ,,,0<F9> if not part of receipt.
            //Maybe put in query. 



            string returnSequence = String.Join("", Sequence.ToArray());
            Console.WriteLine(returnSequence);
            Console.ReadLine();


            returnSequence = returnSequence + "<F9>" + upSequence.ToString() + lineSequence.ToString();

            Console.WriteLine(returnSequence);

            return returnSequence;


        }

    }
}

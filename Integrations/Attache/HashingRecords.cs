using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ZudelloThinClientLibary;

namespace ZudelloThinClient.Attache
{
    public class HashingRecords
    {

        public static bool Hash(string Json, int mappingId)
        {
            string sSourceData;
            byte[] tmpSource;
            byte[] tmpHash;
            bool bEqual = false;
            sSourceData = Json;
            //Create a byte array from source data
            tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);

            //Compute hash based on source data
            tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            List<byte[]> sent = new List<byte[]>();
            using (var db = new ZudelloContext())
            {


                var HashLog = (from a in db.Zhashlog
                               where a.MappingId == mappingId
                               select a);
                Zhashlog h = new Zhashlog();
                if (HashLog.Count() < 1)
                {
                    h.MappingId = mappingId;
                    h.Hash = tmpHash;
                    db.Add(h);
                    db.SaveChanges();
                }

                else
                {


                    foreach (var hash in HashLog)
                    {

                        sent.Add(hash.Hash);

                    }


                    foreach (var hsh in sent)
                    {


                        if (hsh.Length == tmpHash.Length)
                        {
                            int i = 0;
                            while ((i < hsh.Length) && (hsh[i] == tmpHash[i]))
                            {
                                i += 1;
                            }
                            if (i == hsh.Length)
                            {
                                bEqual = true;
                            }
                        }


                    }

                    if (bEqual == false)
                    {

                        Console.WriteLine("The two hash values are not the same");
                        Zhashlog ha = new Zhashlog();
                        ha.MappingId = mappingId;
                        ha.Hash = tmpHash;
                        db.Add(ha);
                        db.SaveChanges();
                        Console.WriteLine("hash Added");
                        return bEqual;

                    }

                    else
                    {
                        Console.WriteLine("Values are the same");
                        return bEqual;


                    }

                }
            }

            return bEqual;
        }
    }
}
 


 
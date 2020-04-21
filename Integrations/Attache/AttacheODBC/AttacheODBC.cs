using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.Odbc;
using ZudelloThinClient.Attache.AttacheSettings;

namespace ZudelloThinClient.Attache.AttacheODBC
{




    public class AttacheODBCconnection
    {
        //Add in config later
        private static readonly string _DNS = ZudelloSetup.GetAttacheSettings().DNS;
        private static readonly string _Uid = ZudelloSetup.GetAttacheSettings().Uid;
        private static readonly string _Pwd = ZudelloSetup.GetAttacheSettings().pwd;

        public string ConnectionString { get; set; } = String.Format(@"Dsn={0}; Uid={1};Pwd = {2};", _DNS, _Uid, _Pwd);
    }  
  
    
}

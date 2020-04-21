using System;
using System.Collections.Generic;

namespace ZudelloThinClientLibary
{
    public partial class Zconnections
    {
        public Zconnections()
        {
            Zmapping = new HashSet<Zmapping>();
            Zqueue = new HashSet<Zqueue>();
        }

        public int Id { get; set; }
        public string ConnectionUuid { get; set; }
        public string DataSource { get; set; }
        public string InitialCatalog { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string IntergrationType { get; set; }
        public string ZudelloCredentials { get; set; }
        public int UseIS { get; set; }

        public virtual ICollection<Zmapping> Zmapping { get; set; }
        public virtual ICollection<Zqueue> Zqueue { get; set; }
    }
}

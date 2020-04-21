using System;
using System.Collections.Generic;

namespace ZudelloThinClientLibary
{
    public partial class Zlastsync
    {
        public int Id { get; set; }
        public int MappingId { get; set; }

        public string LastSync { get; set; }

        public int? lastID { get; set; }
        public virtual Zmapping Mapping { get; set; }
    }
}

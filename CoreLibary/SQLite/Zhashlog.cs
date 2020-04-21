using System;
using System.Collections.Generic;

namespace ZudelloThinClientLibary
{
    public partial class Zhashlog
    {
        public int Id { get; set; }
        public int MappingId { get; set; }
        public byte[] Hash { get; set; }
        public byte[] CreatedAt { get; set; }

        public virtual Zmapping Mapping { get; set; }
    }
}

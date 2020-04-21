using System;
using System.Collections.Generic;

namespace ZudelloThinClientLibary
{
    public partial class Zqueue
    {
        public int Id { get; set; }
        public int? MappingId { get; set; }
        public string Body { get; set; }
        public string Status { get; set; }
        public int ConnectionId { get; set; }
        public string Exception { get; set; }
        public int? Queue_Id { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }

        public int ResponseSent { get; set; } 

        public virtual Zconnections Connection { get; set; }
        public virtual Zmapping Mapping { get; set; }
    }
}

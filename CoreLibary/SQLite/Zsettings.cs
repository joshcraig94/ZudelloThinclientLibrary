using System;
using System.Collections.Generic;

namespace ZudelloThinClientLibary
{
    public partial class Zsettings
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public byte[] CreatedAt { get; set; }
        public byte[] UpdatedAt { get; set; }
        public byte[] DeletedAt { get; set; }
    }
}

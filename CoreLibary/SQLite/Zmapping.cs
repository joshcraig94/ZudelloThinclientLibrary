using System.Collections.Generic;

namespace ZudelloThinClientLibary
{
    public partial class Zmapping
    {
        public Zmapping ShallowCopy()
        {
            return (Zmapping)this.MemberwiseClone();
        }

        public Zmapping()
        {
            Zhashlog = new HashSet<Zhashlog>();
            Zlastsync = new HashSet<Zlastsync>();
            Zqueue = new HashSet<Zqueue>();
            
        }

        public int Id { get; set; }
        public string Type { get; set; }
        public string DocType { get; set; }
        public string Section { get; set; }
        public string Body { get; set; }
        public long? ProcessOrder { get; set; }
        public long? IsOutgoing { get; set; }
        public long? IsMasterData { get; set; }
        public int connection_id { get; set; }//FUCKING BULLSHIT TEMP FIX  //Bug need to write procedure to populate the connection ID tables first then link to this Connection id in the get mappings method. 
        public string IntergrationUuid { get; set; }
        public string database { get; set; }
        public string CreatedAt { get; set; }     
        public string UpdatedAt { get; set; }
        public string DeletedAt { get; set; }
        public string uuid { get; set; } 
        public virtual Zconnections Connection { get; set; }
        public virtual ICollection<Zhashlog> Zhashlog { get; set; }
        public virtual ICollection<Zlastsync> Zlastsync { get; set; }
        public virtual ICollection<Zqueue> Zqueue { get; set; }
    }

    public class ZmappingJson
    {
        public Zmapping[] data { get; set; }
    }

}

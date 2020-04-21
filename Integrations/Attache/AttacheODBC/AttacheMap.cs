using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZudelloThinClient.Attache.AttacheODBC
{
    class AttacheMap
    {


    }

    public class CustomContractResolver : DefaultContractResolver
    {
        private Dictionary<string, string> PropertyMappings { get; set; }

        public CustomContractResolver()
        {
            this.PropertyMappings = new Dictionary<string, string>
        {
            {"code", "data.vendor.id"},
            {"creadate", "data.vendor.Create"},
            {"updtdate", "data.vendor.Modified" }

        };
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            string resolvedName = null;
            var resolved = this.PropertyMappings.TryGetValue(propertyName, out resolvedName);
            return (resolved) ? resolvedName : base.ResolvePropertyName(propertyName);
        }
    }
}

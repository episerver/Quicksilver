using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace EPiServer.Reference.Commerce.Shared.CatalogIndexer
{
    [JsonObject]
    public class RestSearchField
    {
        [JsonConstructor]
        public RestSearchField()
        {
            Attributes = new List<string>();
            Values = new List<string>();
        }

        public RestSearchField(string name, string value) 
            : this()
        {
            Name = name;
            Values.Add(value);
        }

        public RestSearchField(string name, string value, bool isDecimal)
            : this()
        {
            Name = name;
            Values.Add(value);
            IsDecimal = isDecimal;
        }

        public RestSearchField(string name, string value, IEnumerable<string> attribute)
            : this()
        {
            Name = name;
            Values.Add(value);
            Attributes.ToList().AddRange(attribute);
        }

        public IList<string> Attributes { get; set; }
        
        public string Name { get; set; }

        public IList<string> Values { get; set; }

        public bool IsDecimal { get; set; }
    }
}
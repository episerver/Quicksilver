using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Shared.CatalogIndexer
{
    [JsonObject]
    public class RestSearchField
    {
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

        [JsonConstructor]
        public RestSearchField(string name, string value, IEnumerable<string> attribute)
            : this()
        {
            Name = name;
            Values.Add(value);
            if (attribute != null)
            {
                Attributes = Attributes.Union(attribute).ToList();
            }
        }

        public IList<string> Attributes { get; set; }
        
        public string Name { get; set; }

        public IList<string> Values { get; set; }

        public bool IsDecimal { get; set; }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EPiServer.Reference.Commerce.Shared.CatalogIndexer
{
    [JsonObject]
    public class RestSearchDocument
    {
        public RestSearchDocument()
        {
            Fields = new List<RestSearchField>();
        }

        public IList<RestSearchField> Fields { get; set; } 
    }
}
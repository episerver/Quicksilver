using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Search.Extensions;
using Mediachase.Search.Extensions.Indexers;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Reference.Commerce.Shared.CatalogIndexer
{
    public class RestIndexer : CatalogIndexBuilder
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly string _url;

        public RestIndexer()
        {
            _url = VirtualPathUtility.AppendTrailingSlash(ConfigurationManager.AppSettings["SearchApiUrl"]);
        }

        public RestIndexer(string url)
        {
            _url = url;
        }
        
        protected override void OnCatalogEntryIndex(ref SearchDocument document, CatalogEntryDto.CatalogEntryRow entry, string language)
        {
            if (entry.ClassTypeId == EntryType.Variation)
            {
                return;
            }

            var result = GetDocument(language, entry.Code).Result;
            if (result == null)
            {
                throw new Exception(String.Format("could not connect to {0}, please make sure site is active",
                    _url + String.Format("referenceapi/searchdocuments/{0}/{1}", language, entry.Code)));
            }
                
            foreach (var field in result.Fields.Where(field => field.Values.Any()))
            {
                document.Add
                (
                    field.IsDecimal ?
                        new SearchField(field.Name, Decimal.Parse(field.Values.First(), CultureInfo.InvariantCulture), field.Attributes.ToArray()) :
                        new SearchField(field.Name, field.Values.First(), field.Attributes.ToArray())
                );
            }
        }

        private async Task<RestSearchDocument> GetDocument(string language, string code)
        {
            var response = await _client.GetAsync(String.Format("{0}referenceapi/searchdocuments/{1}/{2}", _url, language, code));
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RestSearchDocument>(result);
        }
    }
}
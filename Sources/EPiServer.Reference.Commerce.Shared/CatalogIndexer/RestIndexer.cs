using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Pricing;
using Mediachase.MetaDataPlus;
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
    public class RestIndexer : CatalogIndexBuilder, IDisposable
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly string _url;

        public RestIndexer()
            : this(
                ServiceLocator.Current.GetInstance<ICatalogSystem>(),
                ServiceLocator.Current.GetInstance<IPriceService>(),
                ServiceLocator.Current.GetInstance<IInventoryService>(),
                ServiceLocator.Current.GetInstance<MetaDataContext>(),
                ServiceLocator.Current.GetInstance<CatalogItemChangeManager>(),
                ServiceLocator.Current.GetInstance<NodeIdentityResolver>())
        {
        }

        public RestIndexer(
            ICatalogSystem catalogSystem,
            IPriceService priceService,
            IInventoryService inventoryService,
            MetaDataContext metaDataContext,
            CatalogItemChangeManager catalogItemChangeManager,
            NodeIdentityResolver nodeIdentityResolver)
            : base(
                catalogSystem,
                priceService,
                inventoryService,
                metaDataContext,
                catalogItemChangeManager,
                nodeIdentityResolver)
        {
            _url = VirtualPathUtility.AppendTrailingSlash(ConfigurationManager.AppSettings["SearchApiUrl"]);
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
                throw new Exception($"could not connect to {_url}referenceapi/searchdocuments/{language}/{entry.Code}, please make sure site is active");
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
            var response = await _client.GetAsync($"{_url}referenceapi/searchdocuments/{language}/{code}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RestSearchDocument>(result);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
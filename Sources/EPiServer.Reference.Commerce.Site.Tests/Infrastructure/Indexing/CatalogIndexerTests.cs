using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Indexing;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using EPiServer.Web.Routing;
using FluentAssertions;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Pricing;
using Mediachase.MetaDataPlus;
using Mediachase.Search.Extensions;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Infrastructure.Indexing
{
    public class CatalogIndexerTests
    {
        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddOriginalUSDPrice()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");
            document[IndexingHelper.GetOriginalPriceField(MarketId.Default, Currency.USD)].Value.ToString()
                .Should()
                .Equals((1000m).ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddOriginalGBPPrice()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document[IndexingHelper.GetOriginalPriceField(MarketId.Default, Currency.GBP)].Value.ToString()
                .Should()
                .Equals((2000m).ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddDicountUSDPrice()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document[IndexingHelper.GetPriceField(MarketId.Default, Currency.USD)].Value.ToString()
                .Should()
                .Equals((1000m).ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddDiscountGBPPrice()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document[IndexingHelper.GetPriceField(MarketId.Default, Currency.GBP)].Value.ToString()
                .Should()
                .Equals((2000m).ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddColor()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document["color"].Should().Equals("Green");
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddSize()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document["size"].Should().Equals("Small");
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddCode()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document["code"].Should().Equals("Variant 1");
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddDisplayName()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document["displayname"].Should().Equals("DisplayName");
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddContentLink()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document["content_link"].Should().Equals(GetContentReference(444, CatalogContentType.CatalogEntry).ToString());
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddCreated()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document["created"].Should().Equals(new DateTime(2012, 4, 4).ToString("yyyyMMddhhmmss"));
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddBrand()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document["brand"].Should().Equals("Brand");
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddTopCategory()
        {
            var entry = GetCatalogEntryRow("Product");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document["top_category_name"].Should().Equals("Category");
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocumentUnderCatalog_ShouldAddTopCategory()
        {
            var entry = GetCatalogEntryRow("Product", "catalogProductCode");
            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document["top_category_name"].Should().Equals("Catalog");
        }

        [Fact]
        public void UpdateSearchDocument_WhenPopulatingDocument_ShouldAddImageUrl()
        {
            var entry = GetCatalogEntryRow("Product");

            _urlResolverMock.Setup(x => x.GetUrl(new ContentReference(5, 0)))
                .Returns("http://myimage");

            _assetUrlConventionsMock.Setup(x => x.GetDefaultGroup(It.IsAny<IAssetContainer>()))
                .Returns("default");

            var document = new SearchDocument();
            _subject.UpdateSearchDocument(ref document, entry.Code, "en");

            document["image_url"].Should().Equals("http://myimage");
        }

        private Mock<IPromotionService> _promotionServiceMock;
        private Mock<IContentLoader> _contentLoaderMock;
        private Mock<IPriceService> _priceServiceMock;
        private Mock<ReferenceConverter> _referenceConverterMock;
        private CatalogIndexer _subject;
        private Mock<AssetUrlResolver> _assetUrlResolverMock;
        private Mock<IRelationRepository> _relationRepositoryMock;
        private Mock<UrlResolver> _urlResolverMock;
        private Mock<AssetUrlConventions> _assetUrlConventionsMock;
        private FakeAppContext _fakeAppContext;
        private Money _cheapPriceUSD;
        private Money _expensivePriceGBP;
        private Money _discountPriceUSD;
        private Money _discountPriceGBP;
        private FashionProduct _fashionProduct;
        private FashionProduct _catalogProduct;


        public CatalogIndexerTests()
        {
            var synchronizedObjectInstanceCache = new Mock<ISynchronizedObjectInstanceCache>();

            _cheapPriceUSD = new Money(1000, "USD");
            _expensivePriceGBP = new Money(2000, "GBP");
            _discountPriceUSD = new Money(500, "USD");
            _discountPriceGBP = new Money(500, "GBP");
            var catalogSystemMock = new Mock<ICatalogSystem>();
            _promotionServiceMock = new Mock<IPromotionService>();
            _contentLoaderMock = new Mock<IContentLoader>();
            _priceServiceMock = new Mock<IPriceService>();
            _relationRepositoryMock = new Mock<IRelationRepository>();
            _promotionServiceMock = new Mock<IPromotionService>();
            _fakeAppContext = new FakeAppContext();
            _referenceConverterMock = new Mock<ReferenceConverter>(
                new EntryIdentityResolver(synchronizedObjectInstanceCache.Object),
                new NodeIdentityResolver(synchronizedObjectInstanceCache.Object))
            {
                CallBase = true
            };

            _urlResolverMock = new Mock<UrlResolver>();
            _assetUrlConventionsMock = new Mock<AssetUrlConventions>();

            _assetUrlResolverMock = new Mock<AssetUrlResolver>(_urlResolverMock.Object,
                _assetUrlConventionsMock.Object,
                _contentLoaderMock.Object);

            _subject = new CatalogIndexer(catalogSystemMock.Object,
                _priceServiceMock.Object,
                new Mock<IInventoryService>().Object,
                new MetaDataContext(),
                _contentLoaderMock.Object,
                _promotionServiceMock.Object,
                _referenceConverterMock.Object,
                _assetUrlResolverMock.Object,
                _relationRepositoryMock.Object,
                _fakeAppContext,
                new Mock<ILogger>().Object);
            var productReference = GetContentReference(444, CatalogContentType.CatalogEntry);
            var catalogProductReference = GetContentReference(888, CatalogContentType.CatalogEntry);
            var greenVariantReference = GetContentReference(445, CatalogContentType.CatalogEntry);
            var bluevariantReference = GetContentReference(446, CatalogContentType.CatalogEntry);
            var rootNodeReference = GetContentReference(10, CatalogContentType.CatalogNode);
            var catalogReference = GetContentReference(4, CatalogContentType.Catalog);
            var variants = new[] { bluevariantReference, greenVariantReference };
            var greenCatalogKey = new CatalogKey(_fakeAppContext.ApplicationId, "Variant 1");
            var blueCatalogKey = new CatalogKey(_fakeAppContext.ApplicationId, "Variant 2");

            _fashionProduct = new FashionProduct();
            CreateFashionProduct(productReference, rootNodeReference, _fashionProduct, "ProductCode");

            _catalogProduct = new FashionProduct();
            CreateFashionProduct(catalogProductReference, catalogReference, _catalogProduct, "CatalogProductCode");

            SetupGetContentLink("productCode", productReference);
            SetupGetContentLink("catalogProductCode", catalogProductReference);

            var enCultureInfo = CultureInfo.GetCultureInfo("en");

            SetupGetFashionProduct(productReference, enCultureInfo, _fashionProduct);
            SetupGetFashionProduct(catalogProductReference, enCultureInfo, _catalogProduct);
            SetupGetVariants(productReference, variants);
            SetupGetRootNode(rootNodeReference, catalogReference);
            SetupGetCatalog(catalogReference);

            SetupGetDiscountPrice(blueCatalogKey, MarketId.Default, _discountPriceGBP);
            SetupGetDiscountPrice(blueCatalogKey, MarketId.Default, _discountPriceUSD);
            SetupGetDiscountPrice(greenCatalogKey, MarketId.Default, _discountPriceGBP);
            SetupGetDiscountPrice(greenCatalogKey, MarketId.Default, _discountPriceUSD); 

            SetupGetItems(variants, enCultureInfo, new List<FashionVariant>
            {
                new FashionVariant
                {
                    Code = "Variant 1",
                    Color = "Green",
                },
                new FashionVariant
                {
                    Code = "Variant 2",
                    Color = "Blue",
                }
            });

            SetupGetCatalogEntryPrices(new[]
            {
                greenCatalogKey,
                blueCatalogKey
            });
        }

        private void CreateFashionProduct(ContentReference productReference, ContentReference parentReference, FashionProduct fashionProduct, string code)
        {
            fashionProduct.Code = code;
            fashionProduct.DisplayName = "DisplayName";
            fashionProduct.ParentLink = parentReference;
            fashionProduct.ContentLink = productReference;
            fashionProduct.Created = new DateTime(2012, 4, 4);
            fashionProduct.Brand = "Brand";
            fashionProduct.CommerceMediaCollection = new ItemCollection<CommerceMedia>()
            {
                new CommerceMedia(new ContentReference(5, 0), "episerver.core.icontentimage", "default", 0)
            };
        }

        private CatalogEntryDto.CatalogEntryRow GetCatalogEntryRow(string classTypeId, string code = "productCode")
        {
            var dataTable = new CatalogEntryDto.CatalogEntryDataTable();
            var newEntryRow = dataTable.NewCatalogEntryRow();
            newEntryRow.ApplicationId = Guid.NewGuid();
            newEntryRow.CatalogId = 1;
            newEntryRow.ClassTypeId = classTypeId;
            newEntryRow.Code = code;
            newEntryRow.EndDate = DateTime.Now.AddYears(2).ToUniversalTime();
            newEntryRow.IsActive = true;
            newEntryRow.MetaClassId = 700;
            newEntryRow.Name = "Name";
            newEntryRow.StartDate = DateTime.UtcNow.AddHours(-1);
            newEntryRow.TemplateName = "Template";
            dataTable.AddCatalogEntryRow(newEntryRow);
            return newEntryRow;
        }

        private ContentReference GetContentReference(int contentId, CatalogContentType catalogContentType)
        {
            return _referenceConverterMock.Object.GetContentLink(contentId, catalogContentType, 0);
        }

        private void SetupGetContentLink(String code, ContentReference productReference)
        {
            _referenceConverterMock.Setup(x => x.GetContentLink(code))
                .Returns(productReference);
        }

        private void SetupGetFashionProduct(ContentReference productReference, CultureInfo cultureInfo, FashionProduct fashionProduct)
        {
            _contentLoaderMock.Setup(
                x =>
                    x.Get<ProductContent>(productReference))
                .Returns(fashionProduct);

            _contentLoaderMock.Setup(x => x.Get<EntryContentBase>(productReference, cultureInfo)).Returns(fashionProduct);
        }

        private void SetupGetVariants(ContentReference productReference, IEnumerable<ContentReference> variants)
        {
            _relationRepositoryMock.Setup(
                x =>
                    x.GetRelationsBySource<ProductVariation>(productReference))
                .Returns(variants.Select(x =>
                    new ProductVariation
                    {
                        GroupName = "Default",
                        Quantity = 1,
                        SortOrder = 1,
                        Source = productReference,
                        Target = x
                    }));
        }

        private void SetupGetItems(IEnumerable<ContentReference> variantContentReferences, CultureInfo cultureInfo, IEnumerable<FashionVariant> variants)
        {
            _contentLoaderMock.Setup(
                x =>
                    x.GetItems(
                    variantContentReferences,
                   cultureInfo))
                .Returns(variants);
        }

        private void SetupGetRootNode(ContentReference rootNodeReference, ContentReference catalogReference)
        {
            _contentLoaderMock.Setup(x
                =>
                    x.Get<CatalogContentBase>(rootNodeReference))
                .Returns(new NodeContent
                {
                    Code = "Category",
                    DisplayName = "Category",
                    ParentLink = catalogReference
                });
        }

        private void SetupGetCatalog(ContentReference catalogReference)
        {
            _contentLoaderMock.Setup(x
                =>
                    x.Get<CatalogContentBase>(catalogReference))
                .Returns(new CatalogContent() { Name = "Catalog"});
        }

        private void SetupGetCatalogEntryPrices(IEnumerable<CatalogKey> catalogKeys)
        {
            _priceServiceMock.Setup(
                x =>
                    x.GetCatalogEntryPrices(catalogKeys))
                .Returns(catalogKeys.Select(x => new PriceValue
                {
                    CatalogKey = x,
                    ValidFrom = DateTime.Now.AddDays(-5).ToUniversalTime(),
                    MarketId = MarketId.Default,
                    UnitPrice = _cheapPriceUSD,
                    CustomerPricing = CustomerPricing.AllCustomers,
                    MinQuantity = 1
                })
                    .Union(catalogKeys.Select(x => new PriceValue
                    {
                        CatalogKey = x,
                        ValidFrom = DateTime.Now.AddDays(-5).ToUniversalTime(),
                        MarketId = MarketId.Default,
                        UnitPrice = _expensivePriceGBP,
                        CustomerPricing = CustomerPricing.AllCustomers,
                        MinQuantity = 1
                    })));
        }

        private void SetupGetDiscountPrice(CatalogKey catalogKey, MarketId marketId, Money money)
        {
            _promotionServiceMock.Setup(
               x =>
                   x.GetDiscountPrice(
                   catalogKey,
                   marketId,
                   money.Currency))
               .Returns(new PriceValue
               {
                   UnitPrice = money
               });
        }
    }
}

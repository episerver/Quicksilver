using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Reference.Commerce.Shared.CatalogIndexer;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Indexing;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using EPiServer.Web.Routing;
using FluentAssertions;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;

using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Infrastructure.Indexing
{

    public class SearchDocumentControllerTests
    {
        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddOriginalUSDPrice()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.First(x => x.Name.Equals(IndexingHelper.GetOriginalPriceField(MarketId.Default, Currency.USD))).Values.First()
                .Should()
                .Equals((1000m).ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddOriginalGBPPrice()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.First(x => x.Name.Equals(IndexingHelper.GetOriginalPriceField(MarketId.Default, Currency.GBP))).Values.First()
                .Should()
                .Equals((2000m).ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddDicountUSDPrice()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.First(x => x.Name.Equals(IndexingHelper.GetPriceField(MarketId.Default, Currency.USD))).Values.First()
                .Should()
                .Equals((1000m).ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddUSDPriceForPackage()
        {
            var result = _subject.PopulateSearchDocument("en", "Package");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.First(x => x.Name.Equals(IndexingHelper.GetPriceField(MarketId.Default, Currency.USD))).Values.First()
                .Should()
                .Equals((1000m).ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddDiscountGBPPrice()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.First(x => x.Name.Equals(IndexingHelper.GetPriceField(MarketId.Default, Currency.GBP))).Values.First()
                .Should()
                .Equals((2000m).ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddDiscountUSDPrice()
        {
            var result = _subject.PopulateSearchDocument("en", "Package");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.First(x => x.Name.Equals(IndexingHelper.GetPriceField(MarketId.Default, Currency.GBP))).Values.First()
                .Should()
                .Equals((2000m).ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddColor()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.First(x => x.Name.Equals("color")).Values.First().Should().Equals("Green");
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddSize()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.First(x => x.Name.Equals("size")).Values.First().Should().Equals("Small");
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddCode()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.First(x => x.Name.Equals("code")).Values.First().Should().Equals("Variant 1");
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddDisplayName()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.Single(x => x.Name.Equals("displayname")).Values.First().Should().Equals("DisplayName");
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddContentLink()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.Single(x => x.Name.Equals("content_link")).Values.First().Should().Equals(GetContentReference(444, CatalogContentType.CatalogEntry).ToString());
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddCreated()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.Single(x => x.Name.Equals("created")).Values.First().Should().Equals(new DateTime(2012, 4, 4).ToString("yyyyMMddhhmmss"));
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddBrand()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.Single(x => x.Name.Equals("brand")).Values.First().Should().Equals("Brand");
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddTopCategory()
        {
            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;

            document.Content.Fields.Single(x => x.Name.Equals("top_category_name")).Values.First().Should().Equals("Category");
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocumentUnderCatalog_ShouldAddTopCategory()
        {
            var result = _subject.PopulateSearchDocument("en", "CatalogProduct");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;

            document.Content.Fields.Single(x => x.Name.Equals("top_category_name")).Values.First().ShouldBeEquivalentTo("Catalog");
        }

        [Fact]
        public void PopulateSearchDocument_WhenPopulatingDocument_ShouldAddImageUrl()
        {
            _urlResolverMock.Setup(x => x.GetUrl(new ContentReference(5, 0)))
                .Returns("http://myimage");

            _assetUrlConventionsMock.Setup(x => x.GetDefaultGroup(It.IsAny<IAssetContainer>()))
                .Returns("default");

            var result = _subject.PopulateSearchDocument("en", "Product");
            var document = result as OkNegotiatedContentResult<RestSearchDocument>;
            document.Content.Fields.Single(x => x.Name.Equals("image_url")).Values.First().Should().Equals("http://myimage");
        }
        
        private Mock<IPromotionService> _promotionServiceMock;
        private Mock<IContentLoader> _contentLoaderMock;
        private Mock<IPriceService> _priceServiceMock;
        private Mock<ReferenceConverter> _referenceConverterMock;
        private SearchDocumentController _subject;
        private Mock<AssetUrlResolver> _assetUrlResolverMock;
        private Mock<IRelationRepository> _relationRepositoryMock;
        private Mock<UrlResolver> _urlResolverMock;
        private Mock<AssetUrlConventions> _assetUrlConventionsMock;
        private FakeAppContext _fakeAppContext;
        private Money _cheapPriceUSD;
        private Money _expensivePriceGBP;
        private Money _discountPriceUSD;
        private Money _discountPriceGBP;

        public SearchDocumentControllerTests()
        {
            var synchronizedObjectInstanceCache = new Mock<ISynchronizedObjectInstanceCache>();

            _cheapPriceUSD = new Money(1000, "USD");
            _expensivePriceGBP = new Money(2000, "GBP");
            _discountPriceUSD = new Money(500, "USD");
            _discountPriceGBP = new Money(500, "GBP");
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

            _subject = new SearchDocumentController(_priceServiceMock.Object,
                _promotionServiceMock.Object,
                _contentLoaderMock.Object,
                _referenceConverterMock.Object,
                _assetUrlResolverMock.Object,
                _relationRepositoryMock.Object,
                _fakeAppContext)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            var productReference = GetContentReference(444, CatalogContentType.CatalogEntry);
            var catalogProductReference = GetContentReference(888, CatalogContentType.CatalogEntry);
            var packageReference = GetContentReference(666, CatalogContentType.CatalogEntry);
            var greenVariantReference = GetContentReference(445, CatalogContentType.CatalogEntry);
            var bluevariantReference = GetContentReference(446, CatalogContentType.CatalogEntry);
            var rootNodeReference = GetContentReference(10, CatalogContentType.CatalogNode);
            var catalogReference = GetContentReference(4, CatalogContentType.Catalog);
            var variants = new[] { bluevariantReference, greenVariantReference };
            var greenCatalogKey = new CatalogKey(_fakeAppContext.ApplicationId, "Variant 1");
            var blueCatalogKey = new CatalogKey(_fakeAppContext.ApplicationId, "Variant 2");
            var packageCatalogKey = new CatalogKey(_fakeAppContext.ApplicationId, "Package");

            var productCode = "Product";
            SetupGetContentLink(productCode, productReference);
            SetupGetFashionProduct(productCode, productReference, rootNodeReference);

            var catalogProductCode = "CatalogProduct";
            SetupGetContentLink(catalogProductCode, catalogProductReference);
            SetupGetFashionProduct(catalogProductCode, catalogProductReference, catalogReference);

            SetupGetContentLink("Package", packageReference);
            SetupGetFashionPackage(packageReference, rootNodeReference);

            SetupGetVariants(productReference, variants);
            SetupGetRootNode(rootNodeReference, catalogReference);
            SetupGetCatalog(catalogReference);

            SetupGetDiscountPrice(blueCatalogKey, MarketId.Default, _discountPriceGBP);
            SetupGetDiscountPrice(blueCatalogKey, MarketId.Default, _discountPriceUSD);
            SetupGetDiscountPrice(greenCatalogKey, MarketId.Default, _discountPriceGBP);
            SetupGetDiscountPrice(greenCatalogKey, MarketId.Default, _discountPriceUSD);

            SetupGetItems(variants, CultureInfo.GetCultureInfo("en"), new List<FashionVariant>
            {
                new FashionVariant
                {
                    Code = "Variant 1",
                    Color = "Green",
                    Size = "Small"
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

            SetupGetCatalogEntryPrices(new[] { packageCatalogKey});
            SetupGetDiscountPrice(packageCatalogKey, MarketId.Default, _discountPriceGBP);
            SetupGetDiscountPrice(packageCatalogKey, MarketId.Default, _discountPriceUSD);
        }

        private ContentReference GetContentReference(int contentId, CatalogContentType catalogContentType)
        {
            return _referenceConverterMock.Object.GetContentLink(contentId, catalogContentType, 0);
        }

        private void SetupGetContentLink(string code, ContentReference productReference)
        {
            _referenceConverterMock.Setup(x => x.GetContentLink(code))
                .Returns(productReference);
        }

        private void SetupGetFashionProduct(string productCode, ContentReference productReference, ContentReference rootNodeReference)
        {
            _contentLoaderMock.Setup(
                x =>
                    x.Get<EntryContentBase>(productReference))
                .Returns(new FashionProduct
                {
                    Code = productCode,
                    DisplayName = "DisplayName",
                    ParentLink = rootNodeReference,
                    ContentLink = productReference,
                    Created = new DateTime(2012, 4, 4),
                    Brand = "Brand",
                    CommerceMediaCollection = new ItemCollection<CommerceMedia>()
                    {
                        new CommerceMedia(new ContentReference(5, 0), "episerver.core.icontentimage", "default", 0)
                    }
                });
        }

        private void SetupGetFashionPackage(ContentReference packageReference, ContentReference rootNodeReference)
        {
            _contentLoaderMock.Setup(
                x =>
                    x.Get<EntryContentBase>(packageReference))
                .Returns(new FashionPackage
                {
                    Code = "Package",
                    DisplayName = "DisplayName",
                    ParentLink = rootNodeReference,
                    ContentLink = packageReference,
                    Created = new DateTime(2012, 4, 4),
                    CommerceMediaCollection = new ItemCollection<CommerceMedia>()
                    {
                        new CommerceMedia(new ContentReference(5, 0), "episerver.core.icontentimage", "default", 0)
                    }
                });
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

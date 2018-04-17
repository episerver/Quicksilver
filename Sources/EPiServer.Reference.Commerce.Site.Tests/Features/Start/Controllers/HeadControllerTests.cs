using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Start.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Routing;
using Moq;
using System.Web.Mvc;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Features.Start.Controllers
{
    public class HeadControllerTests
    {
        [Fact]
        public void Title_WhenCalledWithNoContentFromContentRouteHelper_ShouldReturnEmpty()
        {
            _contentRouteHelperMock.Setup(c=>c.Content).Returns( () => null);
            var subject = new HeadController(null, _contentRouteHelperMock.Object);
            var result = subject.Title();

            Assert.Equal(string.Empty, ((ContentResult)result).Content);
        }

        [Fact]
        public void Title_WhenCalledWithStartPageWithTitle_ShouldReturnTitle()
        {
            _startPageMock.Setup(c => c.Title).Returns("Title");

            _contentRouteHelperMock.Setup(c => c.Content).Returns(() => _startPageMock.Object);
            var subject = new HeadController(null, _contentRouteHelperMock.Object);
            var result = subject.Title();

            Assert.Equal("Title", ((ContentResult)result).Content);
        }

        [Fact]
        public void Title_WhenCalledWithStartPageWithNoTitle_ShouldReturnName()
        {
            _startPageMock.Setup(c => c.Title).Returns(() => null);
            _startPageMock.Setup(c => c.Name).Returns("Name");

            _contentRouteHelperMock.Setup(c => c.Content).Returns(() => _startPageMock.Object);
            var subject = new HeadController(null, _contentRouteHelperMock.Object);
            var result = subject.Title();

            Assert.Equal("Name", ((ContentResult)result).Content);
        }

        [Fact]
        public void Title_WhenCalledWithNodeContentWithSeoTitle_ShouldReturnFormatSeoTitle()
        {
            var seoInformation = new SeoInformation()
            {
                Title = "Seo Tittle"
            };

            _nodeContentMock.Setup(c => c.SeoInformation).Returns(seoInformation);
            _contentRouteHelperMock.Setup(c => c.Content).Returns(() => _nodeContentMock.Object);

            var subject = new HeadController(SetupContentLoader().Object, _contentRouteHelperMock.Object);
            var result = subject.Title();

            Assert.Equal("Seo Tittle-Quicksilver", ((ContentResult)result).Content);
        }

        [Fact]
        public void Title_WhenCalledWithNodeContentWithoutSeoTitle_ShouldReturnFormatName()
        {
            var seoInformation = new SeoInformation()
            {
                Description = "Without seo title"
            };

            _nodeContentMock.Setup(c => c.SeoInformation).Returns(seoInformation);
            _nodeContentMock.Setup(c => c.DisplayName).Returns("Display Name");
            _contentRouteHelperMock.Setup(c => c.Content).Returns(() => _nodeContentMock.Object);

            var subject = new HeadController(SetupContentLoader().Object, _contentRouteHelperMock.Object);
            var result = subject.Title();

            Assert.Equal("Display Name-Quicksilver", ((ContentResult)result).Content);
        }

        [Fact]
        public void Title_WhenCalledWithEntryContentBaseWithSeoTitle_ShouldReturnFormatTitleOfNodeAndEntry()
        {
            var seoInformation = new SeoInformation()
            {
                Title = "Entry Seo Tittle"
            };
            _entryContentMock.Setup(c => c.SeoInformation).Returns(seoInformation);


            var nodeSeoInformation = new SeoInformation()
            {
                Title = "Node Seo Tittle"
            };

            _nodeContentMock.Setup(c => c.SeoInformation).Returns(nodeSeoInformation);
            var contentLoaderMock = SetupContentLoader();
            contentLoaderMock.Setup(c => c.Get<CatalogContentBase>(It.IsAny<ContentReference>())).Returns(_nodeContentMock.Object);
            _contentRouteHelperMock.Setup(c => c.Content).Returns(() => _entryContentMock.Object);

            var subject = new HeadController(contentLoaderMock.Object, _contentRouteHelperMock.Object);
            var result = subject.Title();

            Assert.Equal("Entry Seo Tittle - Node Seo Tittle-Quicksilver", ((ContentResult)result).Content);
        }

        [Fact]
        public void Title_WhenCalledWithEntryContentBaseWithoutSeoTitle_ShouldReturnFormatDisplayNameOfNodeAndEntry()
        {
            var seoInformation = new SeoInformation()
            {
                Description = "Entry Seo"
            };
            
            _entryContentMock.Setup(c => c.SeoInformation).Returns(seoInformation);
            _entryContentMock.Setup(c => c.DisplayName).Returns("Entry");

            var nodeSeoInformation = new SeoInformation()
            {
                Description = "Node Seo"
            };

            
            _nodeContentMock.Setup(c => c.SeoInformation).Returns(nodeSeoInformation);
            _nodeContentMock.Setup(c => c.DisplayName).Returns("Node");

            var contentLoaderMock = SetupContentLoader();
            contentLoaderMock.Setup(c => c.Get<CatalogContentBase>(It.IsAny<ContentReference>())).Returns(_nodeContentMock.Object);
            _contentRouteHelperMock.Setup(c => c.Content).Returns(() => _entryContentMock.Object);

            var subject = new HeadController(contentLoaderMock.Object, _contentRouteHelperMock.Object);
            var result = subject.Title();

            Assert.Equal("Entry - Node-Quicksilver", ((ContentResult)result).Content);
        }
        [Fact]
        public void Title_WhenCalledWithEntryContentBaseBeneathCatalogEntry_ShouldReturnFormatContainsCatalogName()
        {
            var expectedTitle = "Fashion Catalog";
            
            var seoInformation = new SeoInformation()
            {
                Description = "Entry Seo"
            };

            _entryContentMock.Setup(c => c.SeoInformation).Returns(seoInformation);
            _entryContentMock.Setup(c => c.DisplayName).Returns("Entry");
            _catalogContentMock.Setup(c => c.Name).Returns("Fashion Catalog");

            var contentLoaderMock = SetupContentLoader();
            contentLoaderMock.Setup(c => c.Get<CatalogContentBase>(It.IsAny<ContentReference>())).Returns(_catalogContentMock.Object);
            _contentRouteHelperMock.Setup(c => c.Content).Returns(() => _entryContentMock.Object);
            
            var subject = new HeadController(contentLoaderMock.Object, _contentRouteHelperMock.Object);
            var result = subject.Title();
            Assert.Contains(expectedTitle, ((ContentResult)result).Content);
        }
        private Mock<IContentLoader> SetupContentLoader()
        {
            _startPageMock.Setup(c => c.TitleFormat).Returns("{title}-Quicksilver");
            _contentLoaderMock.Setup(c => c.Get<StartPage>(It.IsAny<ContentReference>())).Returns(_startPageMock.Object);
            return _contentLoaderMock;
        }

        Mock<IContentRouteHelper> _contentRouteHelperMock;
        Mock<StartPage> _startPageMock;
        Mock<NodeContent> _nodeContentMock;
        Mock<EntryContentBase> _entryContentMock;
        Mock<IContentLoader> _contentLoaderMock;
        Mock<CatalogContentBase> _catalogContentMock;


        public HeadControllerTests()
        {
            _contentRouteHelperMock = new Mock<IContentRouteHelper>();
            _startPageMock = new Mock<StartPage>();
            _entryContentMock = new Mock<EntryContentBase>();
            _nodeContentMock = new Mock<NodeContent>();
            _contentLoaderMock = new Mock<IContentLoader>();
            _catalogContentMock = new Mock<CatalogContentBase>();
        }
    }
}

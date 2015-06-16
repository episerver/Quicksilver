using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Navigation.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Navigation.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Navigation.Controllers
{
    [TestClass]
    public class HeaderControllerTests
    {
        [TestMethod]
        public void Index_WhenContentIsCatalogItem_ShouldUseParentCategoryAsCurrentContentLink()
        {
            var result = (PartialViewResult)_subject.Index(new Mock<VariationContent>().Object);

            Assert.AreEqual<ContentReference>(_nodeContentLink, ((HeaderViewModel)result.Model).CurrentContentLink);
        }

        [TestMethod]
        public void Index_WhenContentIsPage_ShouldUseItAsCurrentContentLink()
        {
            var pageMock = new Mock<PageData>();
            pageMock.Setup(p => p.ContentLink).Returns(new ContentReference(123));
            
            var result = (PartialViewResult)_subject.Index(pageMock.Object);

            Assert.AreEqual<ContentReference>(pageMock.Object.ContentLink, ((HeaderViewModel)result.Model).CurrentContentLink);
        }

        [TestMethod]
        public void Index_WhenCreatingViewModel_ShouldSetStartPage()
        {
            var result = (PartialViewResult)_subject.Index(null);

            Assert.AreEqual<StartPage>(_startPage, ((HeaderViewModel)result.Model).StartPage);
        }

        [TestMethod]
        public void RightMenu_WhenContentIsNull_ShouldHaveNullAsCurrentContentLink()
        {
            var result = (PartialViewResult)_subject.RightMenu(null);

            Assert.AreEqual<ContentReference>(null, ((HeaderViewModel)result.Model).CurrentContentLink);
        }

        [TestMethod]
        public void RightMenu_WhenContentIsNotNull_ShouldUseItAsCurrentContentLink()
        {
            var variationContent = new VariationContent() { ContentLink = new ContentReference(456) };
            
            var result = (PartialViewResult)_subject.RightMenu(variationContent);

            Assert.AreEqual<ContentReference>(variationContent.ContentLink, ((HeaderViewModel)result.Model).CurrentContentLink);
        }

        [TestMethod]
        public void RightMenu_WhenCreatingViewModel_ShouldSetStartPage()
        {
            var result = (PartialViewResult)_subject.RightMenu(null);

            Assert.AreEqual<StartPage>(_startPage, ((HeaderViewModel)result.Model).StartPage);
        }

        HeaderController _subject;
        Mock<IContentLoader> _contentLoaderMock;
        Mock<CurrentContactFacade> _currentContactFacadeMock;
        ContentReference _nodeContentLink = new ContentReference(666);
        ContentReference _productContentLink = new ContentReference(667);
        StartPage _startPage = new Mock<StartPage>().Object;

        [TestInitialize]
        public void Setup()
        {
            _currentContactFacadeMock = new Mock<CurrentContactFacade>();
            _contentLoaderMock = new Mock<IContentLoader>();
            _contentLoaderMock.Setup(c => c.Get<StartPage>(ContentReference.StartPage))
                .Returns(_startPage);
            
            _contentLoaderMock.Setup(c => c.GetAncestors(It.IsAny<ContentReference>()))
                .Returns(new CatalogContentBase [] 
                { 
                    new NodeContent() { ContentLink = _nodeContentLink },
                    new ProductContent() { ContentLink = _productContentLink }
                });

            _subject = new HeaderController(_currentContactFacadeMock.Object, _contentLoaderMock.Object);
        }
    }
}

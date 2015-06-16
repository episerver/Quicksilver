using System.Globalization;
using System.Threading;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Features.WishList.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.WishList.Controllers
{
    [TestClass]
    public class WishListControllerTests
    {
        [TestMethod]
        public void AddToWishList_WhenCodeIsSupplied_ShouldCallServiceWithCode()
        {
            var productCode = "123456";
            var message = "";
            _subject.AddToWishList(productCode);

            _wishListServiceMock.Verify(w => w.AddItem(productCode, out message));
        }

        [TestMethod]
        public void RemoveFromWishList_WhenCodeIsSupplied_ShouldCallServiceWithCode()
        {
            var productCode = "123456";
            
            _subject.RemoveFromWishList(productCode);

            _wishListServiceMock.Verify(w => w.RemoveItem(productCode));
        }

        [TestMethod]
        public void DeleteWishList_WhenCreatingViewModel_ShouldSetNodeToWishListPage()
        {
            var result = (RedirectToRouteResult)_subject.DeleteWishList();

            Assert.AreEqual<PageReference>(_wishListPage, (PageReference)result.RouteValues["Node"]);
        }

        [TestMethod]
        public void DeleteWishList_ShouldCallService()
        {
            _subject.DeleteWishList();

            _wishListServiceMock.Verify(w => w.Delete());
        }

        WishListController _subject;
        Mock<IContentLoader> _contentLoaderMock;
        Mock<IWishListService> _wishListServiceMock;
        Mock<StartPage> _startPageMock;
        PageReference _wishListPage = new PageReference(666);
        private CultureInfo _cultureInfo;

        [TestInitialize]
        public void Setup()
        {
            _contentLoaderMock = new Mock<IContentLoader>();
            _wishListServiceMock = new Mock<IWishListService>();
            _cultureInfo = CultureInfo.CurrentUICulture;
            var english = CultureInfo.CreateSpecificCulture("en");
            Thread.CurrentThread.CurrentUICulture = english;
            var localizationService = new MemoryLocalizationService
            {
                FallbackBehavior = FallbackBehaviors.MissingMessage
            };
            localizationService.AddString(english, "/ProductPage/AddedToWishList", "AddedToWishList");
            localizationService.AddString(english, "/ProductPage/NotAddedToWishList", "NotAddedToWishList");
            _startPageMock = new Mock<StartPage>();
            
            _startPageMock
                .Setup(s => s.WishListPage)
                .Returns(_wishListPage);

            _contentLoaderMock
                .Setup(c => c.Get<StartPage>(ContentReference.StartPage))
                .Returns(_startPageMock.Object);

            _subject = new WishListController(_contentLoaderMock.Object, _wishListServiceMock.Object, localizationService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Thread.CurrentThread.CurrentUICulture = _cultureInfo;
        }
    }
}

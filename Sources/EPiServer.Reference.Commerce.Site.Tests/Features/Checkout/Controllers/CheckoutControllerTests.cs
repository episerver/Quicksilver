using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Checkout.Controllers
{
    public class CheckoutControllerTests
    {
        [Fact]
        public void AddCouponCode()
        {
            string couponCode = "SPECIALOFFER";
            _subject.AddCouponCode(new CheckoutPage(), couponCode);
        }

        [Fact]
        public void RemoveCouponCode()
        {
            string couponCode = "SPECIALOFFER";
            _subject.RemoveCouponCode(new CheckoutPage(), couponCode);
        }

        [Fact]
        public void ChangeAddress()
        {
            var viewModel = new UpdateAddressViewModel
            {
                CurrentPage = new CheckoutPage(),
                BillingAddress = new AddressModel(),
                Shipments = _cart.GetFirstForm().Shipments.Select(x => new ShipmentViewModel
                {
                    Address = new AddressModel()
                }).ToList(),
                ShippingAddressIndex = 0,
                UseBillingAddressForShipment = true
            };

            _subject.ChangeAddress(viewModel);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Sample billing address")]
        public void ChangeAddress_ShouldReturnModelWithSameBillingAddress(string addressId)
        {
            var viewModel = new UpdateAddressViewModel
            {
                CurrentPage = new CheckoutPage(),
                BillingAddress = new AddressModel() { AddressId = addressId },
                Shipments = _cart.GetFirstForm().Shipments.Select(x => new ShipmentViewModel()).ToList(),
                ShippingAddressIndex = -1,
                UseBillingAddressForShipment = true
            };

            var model = ((PartialViewResult)_subject.ChangeAddress(viewModel)).Model as CheckoutViewModel;
            Assert.Equal(model.BillingAddress.AddressId, addressId);
        }

        [Fact]
        public void Index()
        {
            _subject.Index(new CheckoutPage());
        }

        [Fact]
        public void Index_NoPromotionAndUseBillingAddressForShipment_ShouldSaveCart()
        {
            _cart.GetFirstForm().Shipments.First().ShippingMethodId = Guid.NewGuid();
            _cart.GetFirstForm().Promotions.Clear();

            var fulfilledRewardDesciption = new RewardDescription(FulfillmentStatus.Fulfilled, null, null, 0, 0, RewardType.Money, string.Empty);
            _promotionEngineMock.Setup(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>())).Returns(new[] { fulfilledRewardDesciption });
            _promotionEngineMock.Setup(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>())).Callback(() => _cart.GetFirstForm().Promotions.Clear());

            _checkoutViewModelFactoryMock
                .Setup(x => x.CreateCheckoutViewModel(It.IsAny<ICart>(), It.IsAny<CheckoutPage>(), It.IsAny<PaymentMethodViewModel<PaymentMethodBase>>()))
                .Returns((ICart cart, CheckoutPage currentPage, PaymentMethodViewModel<PaymentMethodBase> paymentMethodViewModel) => CreateCheckoutViewModelWithAddressIds(currentPage, paymentMethodViewModel, true));

            _subject.Index(new CheckoutPage());
            _orderRepositoryMock.Verify(s => s.Save(_cart), Times.Once);
        }

        [Fact]
        public void Index_NoPromotionAndUseShippingAddressForShipment_ShouldSaveCart()
        {
            _cart.GetFirstForm().Shipments.First().ShippingMethodId = Guid.NewGuid();
            _cart.GetFirstForm().Promotions.Clear();

            var fulfilledRewardDesciption = new RewardDescription(FulfillmentStatus.Fulfilled, null, null, 0, 0, RewardType.Money, string.Empty);
            _promotionEngineMock.Setup(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>())).Returns(new[] { fulfilledRewardDesciption });
            _promotionEngineMock.Setup(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>())).Callback(() => _cart.GetFirstForm().Promotions.Clear());

            _checkoutViewModelFactoryMock
                .Setup(x => x.CreateCheckoutViewModel(It.IsAny<ICart>(), It.IsAny<CheckoutPage>(), It.IsAny<PaymentMethodViewModel<PaymentMethodBase>>()))
                .Returns((ICart cart, CheckoutPage currentPage, PaymentMethodViewModel<PaymentMethodBase> paymentMethodViewModel) => CreateCheckoutViewModelWithAddressIds(currentPage, paymentMethodViewModel, false));

            _subject.Index(new CheckoutPage());
            _orderRepositoryMock.Verify(s => s.Save(_cart), Times.Once);
        }

        [Fact]
        public void Index_AppliablePromotionsChangeFromDisableToEnable_ShouldSaveCart()
        {
            _cart.GetFirstForm().Shipments.First().ShippingMethodId = Guid.NewGuid();
            _cart.GetFirstForm().Promotions.Clear();

            var fulfilledRewardDesciption = new RewardDescription(FulfillmentStatus.Fulfilled, null, null, 0, 0, RewardType.Money, string.Empty);
            _promotionEngineMock.Setup(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>())).Returns(new[] { fulfilledRewardDesciption });
            _promotionEngineMock.Setup(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>())).Callback(() => _cart.GetFirstForm().Promotions.Add(new PromotionInformation()));

            _subject.Index(new CheckoutPage());
            _orderRepositoryMock.Verify(s => s.Save(_cart), Times.Once);
        }

        public void Index_AppliablePromotionsChangeFromEnableToDisable_ShouldSaveCart()
        {
            _cart.GetFirstForm().Shipments.First().ShippingMethodId = Guid.NewGuid();
            _cart.GetFirstForm().Promotions.Add(new PromotionInformation());

            _promotionEngineMock.Setup(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>())).Returns(Enumerable.Empty<RewardDescription>());
            _promotionEngineMock.Setup(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>())).Callback(() => _cart.GetFirstForm().Promotions.Clear());

            _subject.Index(new CheckoutPage());
            _orderRepositoryMock.Verify(s => s.Save(_cart), Times.Once);
        }

        [Fact]
        public void Index_AppliedPromotionsAlwaysEnable_ShouldNotSaveCart()
        {
            _cart.GetFirstForm().Shipments.First().ShippingMethodId = Guid.NewGuid();
            _cart.GetFirstForm().Promotions.Add(new PromotionInformation());

            var fulfilledRewardDesciption = new RewardDescription(FulfillmentStatus.Fulfilled, null, null, 0, 0, RewardType.Money, string.Empty);
            _promotionEngineMock.Setup(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>())).Returns(new[] { fulfilledRewardDesciption });

            _subject.Index(new CheckoutPage());
            _orderRepositoryMock.Verify(s => s.Save(_cart), Times.Never);
        }

        [Fact]
        public void Index_AppliablePromotionsAlwaysDisable_ShouldNotSaveCart()
        {
            _cart.GetFirstForm().Shipments.First().ShippingMethodId = Guid.NewGuid();
            _cart.GetFirstForm().Promotions.Clear();

            var notFulfilledRewardDesciption = new RewardDescription(FulfillmentStatus.NotFulfilled, null, null, 0, 0, RewardType.Money, string.Empty);
            _promotionEngineMock.Setup(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>())).Returns(new[] { notFulfilledRewardDesciption });

            _subject.Index(new CheckoutPage());
            _orderRepositoryMock.Verify(s => s.Save(_cart), Times.Never);
        }

        [Fact]
        public void OrderSummary()
        {
            _subject.OrderSummary();
        }

        [Fact]
        public void Purchase()
        {
            Mock<ControllerContext> controllerContextMock = new Mock<ControllerContext>();
            controllerContextMock.Setup(
                x => x.HttpContext.User.Identity.IsAuthenticated).Returns(true);
            _subject.ControllerContext = controllerContextMock.Object;

            var paymentMethodViewModel = CreatePaymentMethodViewModel();
            var checkoutViewModel = CreateCheckoutViewModel(new CheckoutPage(), paymentMethodViewModel);
            _subject.Purchase(checkoutViewModel, checkoutViewModel.Payment);
        }

        [Fact]
        public void SingleShipment()
        {
            _subject.SingleShipment(new CheckoutPage());
        }

        [Fact]
        public void Update()
        {
            var paymentViewModel = CreatePaymentMethodViewModel();
            var shipmentViewModel = new UpdateShippingMethodViewModel
            {
                Shipments = _cart.GetFirstForm().Shipments.Select(x => new ShipmentViewModel
                {
                    Address = new AddressModel()
                }).ToList(),
            };
            _subject.Update(new CheckoutPage(), shipmentViewModel, paymentViewModel);
        }

        [Fact]
        public void OnException_ShouldDelegateToExceptionHandler()
        {
            _subject.CallOnException(_exceptionContext);

            _controllerExceptionHandlerMock.Verify(x => x.HandleRequestValidationException(_exceptionContext, "purchase", _subject.OnPurchaseException));
        }

        [Fact]
        public void OnPurchaseException_WhenRoutedDataIsntSet_ShouldReturnEmptyResult()
        {
            Setup_RequestContext_to_contain_routed_data(null);
            Setup_exception(new HttpRequestValidationException());

            var result = _subject.OnPurchaseException(_exceptionContext);
            Assert.IsType(typeof(EmptyResult), result);
        }

        private readonly Mock<HttpRequestBase> _httpRequestBaseMock;
        private readonly Mock<HttpContextBase> _httpContextBaseMock;
        private readonly Mock<RequestContext> _requestContextMock;
        private readonly ExceptionContext _exceptionContext;
        private readonly Mock<ControllerExceptionHandler> _controllerExceptionHandlerMock;
        private readonly CheckoutControllerForTest _subject;
        private readonly Mock<IContentRepository> _contentRepositoryMock;
        private readonly Mock<IMailService> _mailServiceMock;
        private readonly MemoryLocalizationService _localizationService;
        private readonly Mock<ICurrencyService> _currencyServiceMock;
        private readonly Mock<CustomerContextFacade> _customerContextFacadeMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<CheckoutViewModelFactory> _checkoutViewModelFactoryMock;
        private readonly Mock<IOrderGroupCalculator> _orderGroupCalculatorMock;
        private readonly Mock<IPaymentProcessor> _paymentProcessorMock;
        private readonly Mock<IPromotionEngine> _promotionEngineMock;
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<IAddressBookService> _addressBookServiceMock;
        private readonly Mock<OrderSummaryViewModelFactory> _orderSummaryViewModelFactoryMock;
        private readonly Mock<IOrderFactory> _orderFactoryMock;
        private readonly ICart _cart;

        public CheckoutControllerTests()
        {
            _controllerExceptionHandlerMock = new Mock<ControllerExceptionHandler>();
            _requestContextMock = new Mock<RequestContext>();
            _httpRequestBaseMock = new Mock<HttpRequestBase>();
            _httpContextBaseMock = new Mock<HttpContextBase>();
            _contentRepositoryMock = new Mock<IContentRepository>();
            _mailServiceMock = new Mock<IMailService>();
            _localizationService = new MemoryLocalizationService();
            _currencyServiceMock = new Mock<ICurrencyService>();
            _customerContextFacadeMock = new Mock<CustomerContextFacade>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderGroupCalculatorMock = new Mock<IOrderGroupCalculator>();
            _paymentProcessorMock = new Mock<IPaymentProcessor>();
            _promotionEngineMock = new Mock<IPromotionEngine>();
            _cartServiceMock = new Mock<ICartService>();
            _addressBookServiceMock = new Mock<IAddressBookService>();
            _orderSummaryViewModelFactoryMock = new Mock<OrderSummaryViewModelFactory>(null, null, null, null);
            _checkoutViewModelFactoryMock = new Mock<CheckoutViewModelFactory>(null, null, null, null, null, null, null, null);
            _orderFactoryMock = new Mock<IOrderFactory>();
            _cart = new FakeCart(null, new Currency("USD"));
            _exceptionContext = new ExceptionContext
            {
                HttpContext = _httpContextBaseMock.Object,
                RequestContext = _requestContextMock.Object
            };
            var customerId = Guid.NewGuid();
            var orderReference = new OrderReference(1, "PurchaseOrder", customerId, typeof(InMemoryPurchaseOrder));
            var purchaseOrder = new InMemoryPurchaseOrder { Name = orderReference.Name, Currency = _cart.Currency, CustomerId = customerId, OrderLink = orderReference };

            var paymentMock = new Mock<ICreditCardPayment>();
            paymentMock.SetupGet(x => x.CreditCardNumber).Returns("423465654");
            paymentMock.SetupGet(x => x.Status).Returns(PaymentStatus.Pending.ToString());

            _httpContextBaseMock.Setup(x => x.Request).Returns(_httpRequestBaseMock.Object);

            _subject = new CheckoutControllerForTest(_contentRepositoryMock.Object, _mailServiceMock.Object, _localizationService,
                _currencyServiceMock.Object, _controllerExceptionHandlerMock.Object, _customerContextFacadeMock.Object, _orderRepositoryMock.Object,
                _checkoutViewModelFactoryMock.Object, _orderGroupCalculatorMock.Object, _paymentProcessorMock.Object, _promotionEngineMock.Object,
                _cartServiceMock.Object, _addressBookServiceMock.Object, _orderSummaryViewModelFactoryMock.Object, _orderFactoryMock.Object);

            _checkoutViewModelFactoryMock
                .Setup(x => x.CreateCheckoutViewModel(It.IsAny<ICart>(), It.IsAny<CheckoutPage>(), It.IsAny<PaymentMethodViewModel<PaymentMethodBase>>()))
                .Returns((ICart cart, CheckoutPage currentPage, PaymentMethodViewModel<PaymentMethodBase> paymentMethodViewModel) => CreateCheckoutViewModel(currentPage, paymentMethodViewModel));

            _orderFactoryMock.Setup(x => x.CreateCardPayment()).Returns(paymentMock.Object);
            _orderRepositoryMock.Setup(x => x.SaveAsPurchaseOrder(_cart)).Returns(orderReference);
            _orderRepositoryMock.Setup(x => x.Load<IPurchaseOrder>(orderReference.OrderGroupId)).Returns(purchaseOrder);

            _contentRepositoryMock.Setup(x => x.Get<StartPage>(It.IsAny<ContentReference>())).Returns(new StartPage());
            _contentRepositoryMock.Setup(x => x.GetChildren<OrderConfirmationPage>(It.IsAny<ContentReference>())).Returns(new[] { new OrderConfirmationPageForTest() { Language = new CultureInfo("en-US") } });

            _cartServiceMock.Setup(x => x.LoadCart(It.IsAny<string>())).Returns(_cart);
            _cartServiceMock.Setup(x => x.ValidateCart(It.IsAny<ICart>())).Returns(new Dictionary<ILineItem, List<ValidationIssue>>());
            _cartServiceMock.Setup(x => x.RequestInventory(It.IsAny<ICart>())).Returns(new Dictionary<ILineItem, List<ValidationIssue>>());

            _cart.AddLineItem(new InMemoryLineItem(), _orderFactoryMock.Object);
        }

        private CheckoutViewModel CreateCheckoutViewModel(CheckoutPage currentPage, IPaymentMethodViewModel<PaymentMethodBase> paymentMethodViewModel)
        {
            return new CheckoutViewModel
            {
                CurrentPage = currentPage,
                Payment = paymentMethodViewModel,
                UseBillingAddressForShipment = true,
                Shipments = _cart.Forms.SelectMany(x => x.Shipments).Select(shipment => new ShipmentViewModel
                {
                    ShipmentId = shipment.ShipmentId,
                    ShippingMethodId = shipment.ShippingMethodId
                }).ToList(),
                BillingAddress = new AddressModel
                {
                    Email = "unit.test@quicksilver.com"
                }
            };
        }

        private CheckoutViewModel CreateCheckoutViewModelWithAddressIds(CheckoutPage currentPage, IPaymentMethodViewModel<PaymentMethodBase> paymentMethodViewModel, bool useBillingAddressForShipment)
        {
            return new CheckoutViewModel
            {
                CurrentPage = currentPage,
                Payment = paymentMethodViewModel,
                UseBillingAddressForShipment = useBillingAddressForShipment,
                Shipments = _cart.Forms.SelectMany(x => x.Shipments).Select(shipment => new ShipmentViewModel
                {
                    ShipmentId = shipment.ShipmentId,
                    ShippingMethodId = shipment.ShippingMethodId,
                    Address = new AddressModel()
                    {
                        AddressId = Guid.NewGuid().ToString()                        
                    }
                }).ToList(),
                BillingAddress = new AddressModel
                {
                    AddressId = Guid.NewGuid().ToString(),
                    Email = "unit.test@quicksilver.com"
                }
            };
        }

        private IPaymentMethodViewModel<PaymentMethodBase> CreatePaymentMethodViewModel()
        {
            return new PaymentMethodViewModel<GenericCreditCardPaymentMethod>
            {
                PaymentMethodId = Guid.NewGuid(),
                PaymentMethod = new GenericCreditCardPaymentMethod(_localizationService, _orderFactoryMock.Object)
                {
                    CardType = "VISA",
                    CreditCardName = "Card Owner",
                    CreditCardNumber = "123456",
                    CreditCardSecurityCode = "888",
                    ExpirationYear = 2016,
                    ExpirationMonth = 8
                }
            };
        }

        private void Setup_RequestContext_to_contain_routed_data(object rotedData)
        {
            var routeData = new RouteData();
            routeData.DataTokens.Add(RoutingConstants.RoutedDataKey, rotedData);

            _requestContextMock.Setup(x => x.RouteData).Returns(routeData);
        }

        private void Setup_exception(Exception exception)
        {
            _exceptionContext.Exception = exception;
        }

        private class CheckoutControllerForTest : CheckoutController
        {
            public CheckoutControllerForTest(
                IContentRepository contentRepository,
                IMailService mailService,
                LocalizationService localizationService,
                ICurrencyService currencyService,
                ControllerExceptionHandler controllerExceptionHandler,
                CustomerContextFacade customerContextFacade,
                IOrderRepository orderRepository,
                CheckoutViewModelFactory checkoutViewModelFactory,
                IOrderGroupCalculator orderGroupCalculator,
                IPaymentProcessor paymentProcessor,
                IPromotionEngine promotionEngine,
                ICartService cartService,
                IAddressBookService addressBookService,
                OrderSummaryViewModelFactory orderSummaryViewModelFactory,
                IOrderFactory orderFactory
                )
                : base(contentRepository,
                      mailService,
                      localizationService,
                      currencyService,
                      controllerExceptionHandler,
                      customerContextFacade,
                      orderRepository,
                      checkoutViewModelFactory,
                      orderGroupCalculator,
                      paymentProcessor,
                      promotionEngine,
                      cartService,
                      addressBookService,
                      orderSummaryViewModelFactory,
                      orderFactory)
            {
            }

            public void CallOnException(ExceptionContext filterContext)
            {
                OnException(filterContext);
            }
        }

        class OrderConfirmationPageForTest : OrderConfirmationPage
        {
            private CultureInfo _language;

            public override CultureInfo Language
            {
                get { return _language; }
                set { _language = value; }
            }
        }
    }
}

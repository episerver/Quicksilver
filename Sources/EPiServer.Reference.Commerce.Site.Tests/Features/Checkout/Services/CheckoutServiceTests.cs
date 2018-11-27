using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using FluentAssertions;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Checkout.Services
{
    public class CheckoutServiceTests
    {
        [Fact]
        public void UpdateShippingMethods_ShouldUpdateCartShippingMethods()
        {
            var cart = new FakeCart(new MarketImpl(MarketId.Empty), Currency.SEK);
            var viewModel = new List<ShipmentViewModel>
            {
                new ShipmentViewModel { ShippingMethodId = Guid.NewGuid() }
            };

            _subject.UpdateShippingMethods(cart, viewModel);

            var allShippingMethodIdsInCart = cart.GetFirstForm().Shipments.Select(x => x.ShippingMethodId);
            var allShippingMethodIdsInViewModel = viewModel.Select(x => x.ShippingMethodId);

            allShippingMethodIdsInCart.Should().BeEquivalentTo(allShippingMethodIdsInViewModel);
        }

        [Fact]
        public void UpdateShippingAddresses_ShouldUpdateAllShippingAddresses()
        {
            var cart = new FakeCart(new MarketImpl(MarketId.Empty), Currency.SEK);
            cart.GetFirstForm().Shipments.Add(new FakeShipment());

            var viewModel = new CheckoutViewModel
            {
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel { Address = new AddressModel { AddressId = "addressId1" } },
                    new ShipmentViewModel { Address = new AddressModel { AddressId = "addressId2" } }
                }
            };

            _subject.UpdateShippingAddresses(cart, viewModel);

            Assert.Equal("addressId1", cart.GetFirstForm().Shipments.First().ShippingAddress.Id);
            Assert.Equal("addressId2", cart.GetFirstForm().Shipments.Last().ShippingAddress.Id);
        }

        [Fact]
        public void UpdateShippingAddresses_WhenBillingAddressIsUsedAsShippingAddress_ShouldSetShippingAddressAsBillingAddress()
        {
            var cart = new FakeCart(new MarketImpl(MarketId.Empty), Currency.SEK);
            cart.GetFirstForm().Shipments.Add(new FakeShipment());

            var viewModel = new CheckoutViewModel
            {
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel { Address = new AddressModel { AddressId = "addressId1" } },
                    new ShipmentViewModel { Address = new AddressModel { AddressId = "addressId2" } }
                },
                UseBillingAddressForShipment = true,
                BillingAddress = new AddressModel
                {
                    AddressId = "addressId3"
                }
            };

            _subject.UpdateShippingAddresses(cart, viewModel);

            Assert.Equal("addressId3", cart.GetFirstShipment().ShippingAddress.Id);
        }

        [Fact]
        public void CreateAndAddPaymentToCart_ShouldUpdateCartPayment()
        {
            var cart = new FakeCart(new MarketImpl(MarketId.Empty), Currency.SEK);

            _orderGroupCalculatorMock.Setup(x => x.GetTotal(It.IsAny<IOrderGroup>())).Returns(() => new Money(1, Currency.USD));

            var paymentMethodMock = new Mock<FakePaymentMethod>("PaymentMethod");
            paymentMethodMock.Setup(x => x.CreatePayment(It.IsAny<decimal>(), It.IsAny<IOrderGroup>()))
                .Returns(() => new FakePayment { Amount = 2 });

            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = "billingAddress" },
                Payment = paymentMethodMock.Object
            };

            _subject.CreateAndAddPaymentToCart(cart, viewModel);

            Assert.Equal(1, cart.GetFirstForm().Payments.Count);
            Assert.Equal(2, cart.GetFirstForm().Payments.Single().Amount);
            Assert.Equal(viewModel.BillingAddress.AddressId, cart.GetFirstForm().Payments.Single().BillingAddress.Id);
        }

        [Fact]
        public void CreateAndAddSomePaymentToCart_CartShouldContainOnlyOnePayment()
        {
            var cart = new FakeCart(new MarketImpl(MarketId.Empty), Currency.SEK);

            _orderGroupCalculatorMock.Setup(x => x.GetTotal(It.IsAny<IOrderGroup>())).Returns(() => new Money(1, Currency.USD));

            var paymentMethodMock = new Mock<FakePaymentMethod>("PaymentMethod");
            paymentMethodMock.Setup(x => x.CreatePayment(It.IsAny<decimal>(), It.IsAny<IOrderGroup>()))
                .Returns(() => new FakePayment { Amount = 2 });

            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = "billingAddress" },
                Payment = paymentMethodMock.Object
            };

            _subject.CreateAndAddPaymentToCart(cart, viewModel);
            _subject.CreateAndAddPaymentToCart(cart, viewModel);
            _subject.CreateAndAddPaymentToCart(cart, viewModel);
            _subject.CreateAndAddPaymentToCart(cart, viewModel);

            Assert.Equal(1, cart.GetFirstForm().Payments.Count);
        }

        [Fact]
        public void PlaceOrder_WhenPaymentProcessingSucceeds_ShouldCreatePurchaseOrder()
        {
            var cartTotal = new Money(1, Currency.USD);
            _orderGroupCalculatorMock.Setup(x => x.GetTotal(It.IsAny<IOrderGroup>())).Returns(() => cartTotal);

            var purchaseOrderMock = Mock.Of<IPurchaseOrder>(x => x.OrderNumber == "123");
            _orderRepositoryMock.Setup(x => x.Load<IPurchaseOrder>(It.IsAny<int>())).Returns(purchaseOrderMock);

            _orderRepositoryMock.Setup(x => x.SaveAsPurchaseOrder(It.IsAny<ICart>()))
                .Returns(() => new OrderReference(0, "", Guid.Empty, null));

            var cart = new FakeCart(new MarketImpl(MarketId.Empty), Currency.SEK);
            cart.GetFirstForm().Payments.Add(new FakePayment { Status = PaymentStatus.Processed.ToString(), Amount = cartTotal.Amount });

            var modelState = new ModelStateDictionary();

            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = "billingAddress" },
                Payment = new Mock<FakePaymentMethod>("PaymentMethod").Object
            };

            var result = _subject.PlaceOrder(cart, modelState, viewModel);

            Assert.Equal(purchaseOrderMock, result);
            Assert.Equal(0, modelState.Count(x => x.Value.Errors.Count > 0));
        }

        [Fact]
        public void PlaceOrder_WhenPaymentProcessingSucceeds_ShouldDeletedCart()
        {
            var cartTotal = new Money(1, Currency.USD);
            _orderGroupCalculatorMock.Setup(x => x.GetTotal(It.IsAny<IOrderGroup>())).Returns(() => cartTotal);

            _orderRepositoryMock.Setup(x => x.Load<IPurchaseOrder>(It.IsAny<int>())).Returns(new Mock<IPurchaseOrder>().Object);

            _orderRepositoryMock.Setup(x => x.SaveAsPurchaseOrder(It.IsAny<ICart>()))
                .Returns(() => new OrderReference(0, "", Guid.Empty, null));

            var cart = new FakeCart(new MarketImpl(MarketId.Empty), Currency.SEK)
            {
                OrderLink = new OrderReference(1, "", Guid.Empty, null)
            };
            cart.GetFirstForm().Payments.Add(new FakePayment { Status = PaymentStatus.Processed.ToString(), Amount = cartTotal.Amount });

            var modelState = new ModelStateDictionary();

            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = "billingAddress" },
                Payment = new Mock<FakePaymentMethod>("PaymentMethod").Object
            };

            _subject.PlaceOrder(cart, modelState, viewModel);

            _orderRepositoryMock.Verify(x => x.Delete(cart.OrderLink), Times.Once);
        }

        [Fact]
        public void PlaceOrder_WhenRequestInventoryHasAnIssue_ShouldReturnNull()
        {
            var cartTotal = new Money(1, Currency.USD);
            _orderGroupCalculatorMock.Setup(x => x.GetTotal(It.IsAny<IOrderGroup>())).Returns(() => cartTotal);

            _orderRepositoryMock.Setup(x => x.Load<IPurchaseOrder>(It.IsAny<int>())).Returns(new Mock<IPurchaseOrder>().Object);

            _orderRepositoryMock.Setup(x => x.SaveAsPurchaseOrder(It.IsAny<ICart>()))
                .Returns(() => new OrderReference(0, "", Guid.Empty, null));

            var returnObject = new Mock<Dictionary<ILineItem, IList<ValidationIssue>>>();
            returnObject.Object.Add(new Mock<ILineItem>().Object, new List<ValidationIssue>() {
                ValidationIssue.AdjustedQuantityByAvailableQuantity
            });

            _cartServiceMock
                .Setup(x => x.RequestInventory(It.IsAny<ICart>()))
                .Returns(returnObject.Object);

            var cart = new FakeCart(new MarketImpl(MarketId.Empty), Currency.SEK)
            {
                OrderLink = new OrderReference(1, "", Guid.Empty, null)
            };
            cart.GetFirstForm().Payments.Add(new FakePayment { Status = PaymentStatus.Processed.ToString(), Amount = cartTotal.Amount });

            var modelState = new ModelStateDictionary();

            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = "billingAddress" },
                Payment = new Mock<FakePaymentMethod>("PaymentMethod").Object
            };

            var result = _subject.PlaceOrder(cart, modelState, viewModel);

            Assert.Null(result);
            Assert.Equal(1, modelState.Count(x => x.Value.Errors.Count > 0));
        }

        [Fact]
        public void PlaceOrder_WhenPaymentProcessingFails_ShouldReturnNullAndAddModelError()
        {
            var cartTotal = new Money(1, Currency.USD);
            _orderGroupCalculatorMock.Setup(x => x.GetTotal(It.IsAny<IOrderGroup>())).Returns(() => cartTotal);

            _orderRepositoryMock.Setup(x => x.Load<IPurchaseOrder>(It.IsAny<int>())).Returns(new Mock<IPurchaseOrder>().Object);

            var orderReference = new OrderReference(0, "", Guid.Empty, null);
            _orderRepositoryMock.Setup(x => x.SaveAsPurchaseOrder(It.IsAny<ICart>())).Returns(() => orderReference);

            var cart = new FakeCart(new MarketImpl(MarketId.Empty), Currency.SEK);
            cart.GetFirstForm().Payments.Add(new FakePayment { Status = PaymentStatus.Processed.ToString(), Amount = 1 });

            var modelState = new ModelStateDictionary();

            _orderGroupCalculatorMock.Setup(p => p.GetTotal(It.IsAny<IOrderGroup>())).Throws(new PaymentException("", "", ""));

            var PaymentMethodMock = new Mock<FakePaymentMethod>(String.Empty);

            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = "billingAddress" },
                Payment = PaymentMethodMock.Object
            };

            var result = _subject.PlaceOrder(cart, modelState, viewModel);

            Assert.Null(result);
            Assert.Equal(1, modelState.Count(x => x.Value.Errors.Count > 0));
        }

        [Fact]
        public void PlaceOrder_WhenProcessingPaymentNotSuccess_ShouldReturnNullAndAddModelError()
        {
            var cartTotal = new Money(1, Currency.USD);
            _orderGroupCalculatorMock.Setup(x => x.GetTotal(It.IsAny<IOrderGroup>())).Returns(() => cartTotal);

            _orderRepositoryMock.Setup(x => x.Load<IPurchaseOrder>(It.IsAny<int>())).Returns(new Mock<IPurchaseOrder>().Object);

            _orderRepositoryMock.Setup(x => x.SaveAsPurchaseOrder(It.IsAny<ICart>()))
                .Returns(() => new OrderReference(0, "", Guid.Empty, null));

            var cart = new FakeCart(new MarketImpl(MarketId.Empty), Currency.SEK)
            {
                OrderLink = new OrderReference(1, "", Guid.Empty, null)
            };
            cart.GetFirstForm().Payments.Add(new FakePayment { Status = PaymentStatus.Processed.ToString(), Amount = cartTotal.Amount });

            var modelState = new ModelStateDictionary();

            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = "billingAddress" },
                Payment = new Mock<FakePaymentMethod>("PaymentMethod").Object
            };

            _paymentProcessorMock.Setup(x => x.ProcessPayment(It.IsAny<IOrderGroup>(), It.IsAny<IPayment>(), It.IsAny<IShipment>()))
                .Returns((IOrderGroup orderGroup, IPayment payment, IShipment shipment) => PaymentProcessingResult.CreateUnsuccessfulResult("Payment was failed."));

            var result = _subject.PlaceOrder(cart, modelState, viewModel);

            Assert.Null(result);
            Assert.Equal(1, modelState.Count(x => x.Value.Errors.Count > 0));
        }

        [Fact]
        public void SendConfirmation_WhenMailSent_ShouldReturnTrue()
        {
            var customerId = Guid.NewGuid();
            _customerContextFacadeMock.Setup(x => x.CurrentContactId).Returns(customerId);

            var startPage = new StartPage { OrderConfirmationMail = new ContentReference(1) };
            _contentRepositoryMock.Setup(x => x.Get<StartPage>(It.IsAny<PageReference>())).Returns(startPage);

            var confirmationPageMock = Mock.Of<OrderConfirmationPage>(x => x.Language == CultureInfo.CurrentCulture);
            _contentRepositoryMock.Setup(x => x.GetChildren<OrderConfirmationPage>(It.IsAny<ContentReference>()))
                .Returns(new[] { confirmationPageMock });

            var viewModel = new CheckoutViewModel
            {
                CurrentPage = new CheckoutPage(),
                BillingAddress = new AddressModel { Email = "email" },
            };

            var queryCollection = new NameValueCollection
            {
                {"contactId", customerId.ToString()},
                {"orderNumber", "1"}
            };

            var purchaseOrderMock = Mock.Of<IPurchaseOrder>(x => x.OrderLink == new OrderReference(1, "", Guid.Empty, null));

            var result = _subject.SendConfirmation(viewModel, purchaseOrderMock);

            Assert.True(result);

            _mailServiceMock.Verify(x => x.Send(startPage.OrderConfirmationMail, queryCollection, viewModel.BillingAddress.Email,
               CultureInfo.CurrentCulture.Name), Times.Once);
        }

        [Fact]
        public void SendConfirmation_WhenFails_ShouldReturnFalse()
        {
            var customerId = Guid.NewGuid();
            _customerContextFacadeMock.Setup(x => x.CurrentContactId).Returns(customerId);

            var startPage = new StartPage { OrderConfirmationMail = new ContentReference(1) };
            _contentRepositoryMock.Setup(x => x.Get<StartPage>(It.IsAny<PageReference>())).Returns(startPage);

            var confirmationPageMock = Mock.Of<OrderConfirmationPage>(x => x.Language == CultureInfo.CurrentCulture);
            _contentRepositoryMock.Setup(x => x.GetChildren<OrderConfirmationPage>(It.IsAny<ContentReference>()))
                .Returns(new[] { confirmationPageMock });

            var viewModel = new CheckoutViewModel
            {
                CurrentPage = new CheckoutPage(),
                BillingAddress = new AddressModel { Email = "email" },
            };

            var purchaseOrderMock = Mock.Of<IPurchaseOrder>(x => x.OrderLink == new OrderReference(1, "", Guid.Empty, typeof(object)));

            _mailServiceMock.Setup(x => x.Send(It.IsAny<ContentReference>(), It.IsAny<NameValueCollection>(),
              It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();

            var result = _subject.SendConfirmation(viewModel, purchaseOrderMock);

            Assert.False(result);
        }

        private readonly CheckoutService _subject;
        private readonly Mock<IOrderGroupCalculator> _orderGroupCalculatorMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IMailService> _mailServiceMock;
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<IContentRepository> _contentRepositoryMock;
        private readonly Mock<CustomerContextFacade> _customerContextFacadeMock;
        private readonly Mock<IPaymentProcessor> _paymentProcessorMock;
        private readonly Mock<OrderValidationService> _localizedOrderValidationServiceMock;
        public CheckoutServiceTests()
        {
            var addressBookServiceMock = new Mock<IAddressBookService>();
            addressBookServiceMock.Setup(x => x.ConvertToAddress(It.IsAny<AddressModel>(), It.IsAny<IOrderGroup>()))
               .Returns((AddressModel address, IOrderGroup orderGroup) => new FakeOrderAddress { Id = address.AddressId });

            _orderGroupCalculatorMock = new Mock<IOrderGroupCalculator>();
            _orderRepositoryMock = new Mock<IOrderRepository>();

            _mailServiceMock = new Mock<IMailService>();
            _contentRepositoryMock = new Mock<IContentRepository>();
            _customerContextFacadeMock = new Mock<CustomerContextFacade>(null);

            _paymentProcessorMock = new Mock<IPaymentProcessor>();
            _paymentProcessorMock.Setup(x => x.ProcessPayment(It.IsAny<IOrderGroup>(), It.IsAny<IPayment>(), It.IsAny<IShipment>()))
                .Returns((IOrderGroup orderGroup, IPayment payment, IShipment shipment) => PaymentProcessingResult.CreateSuccessfulResult("Payment was processed."));
            _cartServiceMock = new Mock<ICartService>();
            _cartServiceMock.Setup(x => x.RequestInventory(It.IsAny<ICart>())).Returns(new Dictionary<ILineItem, IList<ValidationIssue>>());

            _localizedOrderValidationServiceMock =
                new Mock<OrderValidationService>(null, null, null, null, null);
            _localizedOrderValidationServiceMock.Setup(x => x.ValidateOrder(It.IsAny<IOrderGroup>()))
                .Returns(new Dictionary<ILineItem, IList<ValidationIssue>>());

            _subject = new CheckoutService(
              addressBookServiceMock.Object,
              null,
              _orderGroupCalculatorMock.Object,
              _paymentProcessorMock.Object,
              _orderRepositoryMock.Object,
              _contentRepositoryMock.Object,
              _customerContextFacadeMock.Object,
              new MemoryLocalizationService(),
              _mailServiceMock.Object,
              _cartServiceMock.Object);
        }
    }
}

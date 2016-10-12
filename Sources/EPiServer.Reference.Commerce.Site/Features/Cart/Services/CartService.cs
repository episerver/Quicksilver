using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Services
{
    [ServiceConfiguration(typeof(ICartService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CartService : ICartService
    {
        private readonly IProductService _productService;
        private readonly IPricingService _pricingService;
        private readonly IOrderFactory _orderFactory;
        private readonly CustomerContextFacade _customerContext;
        private readonly IPlacedPriceProcessor _placedPriceProcessor;
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly ILineItemValidator _lineItemValidator;
        private readonly IPromotionEngine _promotionEngine;
        private readonly IOrderRepository _orderRepository;
        private readonly IAddressBookService _addressBookService;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyService;

        public CartService(
            IProductService productService,
            IPricingService pricingService,
            IOrderFactory orderFactory,
            CustomerContextFacade customerContext,
            IPlacedPriceProcessor placedPriceProcessor,
            IInventoryProcessor inventoryProcessor,
            ILineItemValidator lineItemValidator,
            IOrderRepository orderRepository,
            IPromotionEngine promotionEngine,
            IAddressBookService addressBookService,
            ICurrentMarket currentMarket,
            ICurrencyService currencyService)
        {
            _productService = productService;
            _pricingService = pricingService;
            _orderFactory = orderFactory;
            _customerContext = customerContext;
            _placedPriceProcessor = placedPriceProcessor;
            _inventoryProcessor = inventoryProcessor;
            _lineItemValidator = lineItemValidator;
            _promotionEngine = promotionEngine;
            _orderRepository = orderRepository;
            _addressBookService = addressBookService;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
        }

        public void ChangeCartItem(ICart cart, int shipmentId, string code, decimal quantity, string size, string newSize)
        {
            if (quantity > 0)
            {
                if (size == newSize)
                {
                    ChangeQuantity(cart, shipmentId, code, quantity);
                }
                else
                {
                    var newCode = _productService.GetSiblingVariantCodeBySize(code, newSize);
                    UpdateLineItemSku(cart, shipmentId, code, newCode, quantity);
                }
            }
            else
            {
                RemoveLineItem(cart, shipmentId, code);
            }
        }

        public string DefaultCartName
        {
            get { return "Default"; }
        }

        public string DefaultWishListName
        {
            get { return "WishList"; }
        }

        public void RecreateLineItemsBasedOnShipments(ICart cart, IEnumerable<CartItemViewModel> cartItems, IEnumerable<AddressModel> addresses)
        {
            var form = cart.GetFirstForm();
            var items = cartItems
                .GroupBy(x => new { x.AddressId, x.Code, x.IsGift })
                .Select(x => new
                {
                    Code = x.Key.Code,
                    AddressId = x.Key.AddressId,
                    Quantity = x.Count(),
                    IsGift = x.Key.IsGift
                });

            foreach (var shipment in form.Shipments)
            {
                shipment.LineItems.Clear();
            }

            form.Shipments.Clear();

            foreach (var address in addresses)
            {
                IShipment shipment = _orderFactory.CreateShipment();
                form.Shipments.Add(shipment);
                shipment.ShippingAddress = _addressBookService.ConvertToAddress(address);

                foreach (var item in items.Where(x => x.AddressId == address.AddressId))
                {
                    var lineItem = _orderFactory.CreateLineItem(item.Code);
                    lineItem.IsGift = item.IsGift;
                    lineItem.Quantity = item.Quantity;
                    shipment.LineItems.Add(lineItem);
                }
            }

            ValidateCart(cart);
        }

        public void MergeShipments(ICart cart)
        {
            if (cart == null || !cart.GetAllLineItems().Any())
            {
                return;
            }

            var form = cart.GetFirstForm();
            var keptShipment = cart.GetFirstShipment();
            var removedShipments = form.Shipments.Skip(1).ToList();
            var movedLineItems = removedShipments.SelectMany(x => x.LineItems).ToList();
            removedShipments.ForEach(x => x.LineItems.Clear());
            removedShipments.ForEach(x => cart.GetFirstForm().Shipments.Remove(x));

            foreach (var item in movedLineItems)
            {
                var existingLineItem = keptShipment.LineItems.SingleOrDefault(x => x.Code == item.Code);
                if (existingLineItem != null)
                {
                    existingLineItem.Quantity += item.Quantity;
                    continue;
                }

                keptShipment.LineItems.Add(item);
            }

            ValidateCart(cart);
        }


        public bool AddToCart(ICart cart, string code, out string warningMessage)
        {
            warningMessage = string.Empty;

            var lineItem = cart.GetAllLineItems().FirstOrDefault(x => x.Code == code);

            if (lineItem == null)
            {
                lineItem = _orderFactory.CreateLineItem(code);
                lineItem.Quantity = 1;
                cart.AddLineItem(lineItem, _orderFactory);
            }
            else
            {
                var shipment = cart.GetFirstShipment();
                cart.UpdateLineItemQuantity(shipment, lineItem, lineItem.Quantity + 1);
            }

            var validationIssues = ValidateCart(cart);

            foreach (var validationIssue in validationIssues)
            {
                warningMessage += String.Format("Line Item with code {0} ", lineItem.Code);
                warningMessage = validationIssue.Value.Aggregate(warningMessage, (current, issue) => current + String.Format("{0}, ", issue));
                warningMessage = warningMessage.Substring(0, warningMessage.Length - 2);
            }

            if (validationIssues.HasItemBeenRemoved(lineItem))
            {
                return false;
            }

            return GetFirstLineItem(cart, code) != null;
        }

        public void SetCartCurrency(ICart cart, Currency currency)
        {
            if (currency.IsEmpty || currency == cart.Currency)
            {
                return;
            }

            cart.Currency = currency;
            foreach (var lineItem in cart.GetAllLineItems())
            {
                //If there is an item which has no price in the new currency, a NullReference exception will be thrown.
                //Mixing currencies in cart is not allowed.
                //It's up to site's managers to ensure that all items have prices in allowed currency.
                lineItem.PlacedPrice = _pricingService.GetPrice(lineItem.Code, cart.Market.MarketId, currency).Value.Amount;
            }

            ValidateCart(cart);
        }

        public Dictionary<ILineItem, List<ValidationIssue>> ValidateCart(ICart cart)
        {
            if (cart.Name.Equals(DefaultWishListName))
            {
                return new Dictionary<ILineItem, List<ValidationIssue>>();
            }

            var validationIssues = new Dictionary<ILineItem, List<ValidationIssue>>();
            cart.ValidateOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _lineItemValidator);
            cart.UpdatePlacedPriceOrRemoveLineItems(_customerContext.GetContactById(cart.CustomerId), (item, issue) => validationIssues.AddValidationIssues(item, issue), _placedPriceProcessor);
            cart.UpdateInventoryOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _inventoryProcessor);

            cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());

            return validationIssues;
        }

        public Dictionary<ILineItem, List<ValidationIssue>> RequestInventory(ICart cart)
        {
            var validationIssues = new Dictionary<ILineItem, List<ValidationIssue>>();
            cart.AdjustInventoryOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _inventoryProcessor);
            return validationIssues;
        }

        public ICart LoadCart(string name)
        {
            var cart = _orderRepository.LoadCart<ICart>(_customerContext.CurrentContactId, name, _currentMarket);
            if (cart != null)
            {
                SetCartCurrency(cart, _currencyService.GetCurrentCurrency());

                var validationIssues = ValidateCart(cart);
                // After validate, if there is any change in cart, saving cart.
                if (validationIssues.Any())
                {
                    _orderRepository.Save(cart);
                }
            }

            return cart;
        }

        public ICart LoadOrCreateCart(string name)
        {
            var cart = _orderRepository.LoadOrCreateCart<ICart>(_customerContext.CurrentContactId, name, _currentMarket);
            if (cart != null)
            {
                SetCartCurrency(cart, _currencyService.GetCurrentCurrency());
            }

            return cart;
        }

        public bool AddCouponCode(ICart cart, string couponCode)
        {
            var couponCodes = cart.GetFirstForm().CouponCodes;
            if (couponCodes.Any(c => c.Equals(couponCode, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            couponCodes.Add(couponCode);
            var rewardDescriptions = cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());
            var appliedCoupons = rewardDescriptions.Where(r => r.Status == FulfillmentStatus.Fulfilled && !string.IsNullOrEmpty(r.Promotion.Coupon.Code))
                                                   .Select(c => c.Promotion.Coupon.Code);
            var couponApplied = appliedCoupons.Any(c => c.Equals(couponCode, StringComparison.OrdinalIgnoreCase));
            if (!couponApplied)
            {
                couponCodes.Remove(couponCode);
            }
            return couponApplied;
        }

        public void RemoveCouponCode(ICart cart, string couponCode)
        {
            cart.GetFirstForm().CouponCodes.Remove(couponCode);
            cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());
        }

        private void RemoveLineItem(ICart cart, int shipmentId, string code)
        {
            var shipment = cart.GetFirstForm().Shipments.First(s => s.ShipmentId == shipmentId || shipmentId <= 0);

            var lineItem = shipment.LineItems.FirstOrDefault(l => l.Code == code);
            if (lineItem != null)
            {
                shipment.LineItems.Remove(lineItem);
            }

            if (!shipment.LineItems.Any())
            {
                cart.GetFirstForm().Shipments.Remove(shipment);
            }

            ValidateCart(cart);
        }

        private void UpdateLineItemSku(ICart cart, int shipmentId, string oldCode, string newCode, decimal quantity)
        {
            RemoveLineItem(cart, shipmentId, oldCode);

            //merge same sku's
            var newLineItem = GetFirstLineItem(cart, newCode);
            if (newLineItem != null)
            {
                var shipment = cart.GetFirstForm().Shipments.First(s => s.ShipmentId == shipmentId || shipmentId <= 0);
                cart.UpdateLineItemQuantity(shipment, newLineItem, newLineItem.Quantity + quantity);
            }
            else
            {
                newLineItem = _orderFactory.CreateLineItem(newCode);
                newLineItem.Quantity = quantity;
                cart.AddLineItem(newLineItem, _orderFactory);

                var price = _pricingService.GetCurrentPrice(newCode);
                if (price.HasValue)
                {
                    newLineItem.PlacedPrice = price.Value.Amount;
                }
            }

            ValidateCart(cart);
        }

        private void ChangeQuantity(ICart cart, int shipmentId, string code, decimal quantity)
        {
            if (quantity == 0)
            {
                RemoveLineItem(cart, shipmentId, code);
            }
            var shipment = cart.GetFirstForm().Shipments.First(s => s.ShipmentId == shipmentId || shipmentId <= 0);
            var lineItem = shipment.LineItems.FirstOrDefault(x => x.Code == code);
            if (lineItem == null)
            {
                return;
            }

            cart.UpdateLineItemQuantity(shipment, lineItem, quantity);
            ValidateCart(cart);
        }

        private ILineItem GetFirstLineItem(IOrderGroup cart, string code)
        {
            return cart.GetAllLineItems().FirstOrDefault(x => x.Code == code);
        }
    }
}
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
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
using EPiServer.Tracking.Commerce;
using EPiServer.Tracking.Commerce.Data;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Services
{
    [ServiceConfiguration(typeof(ICartService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CartService : ICartService
    {
        private readonly IProductService _productService;
        private readonly IPricingService _pricingService;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly CustomerContextFacade _customerContext;
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly IPromotionEngine _promotionEngine;
        private readonly IOrderRepository _orderRepository;
        private readonly IAddressBookService _addressBookService;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IContentLoader _contentLoader;
        private readonly IRelationRepository _relationRepository;
        private readonly OrderValidationService _orderValidationService;

        public CartService(
            IProductService productService,
            IPricingService pricingService,
            IOrderGroupFactory orderGroupFactory,
            CustomerContextFacade customerContext,
            IInventoryProcessor inventoryProcessor,
            IOrderRepository orderRepository,
            IPromotionEngine promotionEngine,
            IAddressBookService addressBookService,
            ICurrentMarket currentMarket,
            ICurrencyService currencyService,
            ReferenceConverter referenceConverter,
            IContentLoader contentLoader,
            IRelationRepository relationRepository,
            OrderValidationService orderValidationService)
        {
            _productService = productService;
            _pricingService = pricingService;
            _orderGroupFactory = orderGroupFactory;
            _customerContext = customerContext;
            _inventoryProcessor = inventoryProcessor;
            _promotionEngine = promotionEngine;
            _orderRepository = orderRepository;
            _addressBookService = addressBookService;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
            _relationRepository = relationRepository;
            _orderValidationService = orderValidationService;
        }

        public CartChangeData ChangeCartItem(ICart cart, int shipmentId, string code, 
            decimal quantity, string size, string newSize, string displayName)
        {
            CartChangeData cartChange = null;
            if (quantity > 0)
            {
                if (size == newSize)
                {
                    // Custom cart change type: quantityChanged.
                    cartChange = new CartChangeData("quantityChanged", code);
                    cartChange.SetChange("oldQuantity", cart.GetAllLineItems().FirstOrDefault(x => x.Code == code).Quantity);

                    ChangeQuantity(cart, shipmentId, code, quantity);

                    return cartChange;
                }

                // Custom cart change type: variantChanged.
                cartChange = new CartChangeData("variantChanged", code);
                cartChange.SetChange("oldSize", size);
                cartChange.SetChange("oldCode", code);
                cartChange.SetChange("oldPrice", cart.GetAllLineItems().FirstOrDefault(x => x.Code == code).PlacedPrice);

                var newCode = _productService.GetSiblingVariantCodeBySize(code, newSize);
                UpdateLineItemSku(cart, shipmentId, code, newCode, quantity, displayName);

                return cartChange;
            }

            RemoveLineItem(cart, shipmentId, code);
            cartChange = new CartChangeData(CartChangeType.ItemRemoved, code);
            return cartChange;
        }

        public IDictionary<ILineItem, IList<ValidationIssue>> ValidateCart(ICart cart)
        {
            return _orderValidationService.ValidateOrder(cart);
        }

        public string DefaultCartName => "Default";

        public string DefaultWishListName => "WishList";

        public void RecreateLineItemsBasedOnShipments(ICart cart, IEnumerable<CartItemViewModel> cartItems, IEnumerable<AddressModel> addresses)
        {
            var form = cart.GetFirstForm();
            var items = cartItems
                .GroupBy(x => new { x.AddressId, x.Code, x.DisplayName, x.IsGift })
                .Select(x => new
                {
                    Code = x.Key.Code,
                    DisplayName = x.Key.DisplayName,
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
                var shipment = cart.CreateShipment(_orderGroupFactory);
                form.Shipments.Add(shipment);
                shipment.ShippingAddress = _addressBookService.ConvertToAddress(address, cart);

                foreach (var item in items.Where(x => x.AddressId == address.AddressId))
                {
                    var lineItem = cart.CreateLineItem(item.Code, _orderGroupFactory);
                    lineItem.DisplayName = item.DisplayName;
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

        public void UpdateShippingMethod(ICart cart, int shipmentId, Guid shippingMethodId)
        {
            var shipment = cart.GetFirstForm().Shipments.First(x => x.ShipmentId == shipmentId);
            shipment.ShippingMethodId = shippingMethodId;

            ValidateCart(cart);
        }

        public AddToCartResult AddToCart(ICart cart, string code, decimal quantity)
        {
            var result = new AddToCartResult();
            var contentLink = _referenceConverter.GetContentLink(code);
            var entryContent = _contentLoader.Get<EntryContentBase>(contentLink);

            if (entryContent is BundleContent)
            {
                foreach (var relation in _relationRepository.GetChildren<BundleEntry>(contentLink))
                {
                    var entry = _contentLoader.Get<EntryContentBase>(relation.Child);
                    var recursiveResult = AddToCart(cart, entry.Code, relation.Quantity ?? 1);
                    if (recursiveResult.EntriesAddedToCart)
                    {
                        result.EntriesAddedToCart = true;
                    }

                    foreach (var message in recursiveResult.ValidationMessages)
                    {
                        result.ValidationMessages.Add(message);
                    }
                }

                return result;
            }

            var lineItem = cart.GetAllLineItems().FirstOrDefault(x => x.Code == code && !x.IsGift);

            if (lineItem == null)
            {
                lineItem = AddNewLineItem(cart, code, quantity, entryContent.DisplayName);
            }
            else
            {
                var shipment = cart.GetFirstShipment();
                cart.UpdateLineItemQuantity(shipment, lineItem, lineItem.Quantity + quantity);
            }

            var validationIssues = ValidateCart(cart);

            AddValidationMessagesToResult(result, lineItem, validationIssues);

            return result;
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
                // If there is an item which has no price in the new currency, a NullReference exception will be thrown.
                // Mixing currencies in cart is not allowed.
                // It's up to site's managers to ensure that all items have prices in allowed currency.
                lineItem.PlacedPrice = _pricingService.GetPrice(lineItem.Code, cart.MarketId, currency).UnitPrice.Amount;
            }

            ValidateCart(cart);
        }

        public IDictionary<ILineItem, IList<ValidationIssue>> RequestInventory(ICart cart)
        {
            var validationIssues = new Dictionary<ILineItem, IList<ValidationIssue>>();
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
            var rewardDescriptions = ApplyDiscounts(cart);
            var appliedCoupons = rewardDescriptions
                .Where(r => r.AppliedCoupon != null)
                .Select(r => r.AppliedCoupon);

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
            ApplyDiscounts(cart);
        }

        public IEnumerable<RewardDescription> ApplyDiscounts(ICart cart)
        {
            return cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());
        }

        private void RemoveLineItem(ICart cart, int shipmentId, string code)
        {
            // Gets the shipment for shipment id or for wish list shipment id as a parameter is always equal zero (wish list).
            var shipment = cart.GetFirstForm().Shipments.First(s => s.ShipmentId == shipmentId || shipmentId == 0);

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

        private static void AddValidationMessagesToResult(AddToCartResult result, ILineItem lineItem, IDictionary<ILineItem, IList<ValidationIssue>> validationIssues)
        {
            foreach (var validationIssue in validationIssues)
            {
                var warning = new StringBuilder();
                warning.Append($"Line Item with code {lineItem.Code} ");
                validationIssue.Value.Aggregate(warning, (current, issue) => current.Append(issue).Append(", "));

                result.ValidationMessages.Add(warning.ToString().TrimEnd(',', ' '));
            }

            if (!validationIssues.HasItemBeenRemoved(lineItem))
            {
                result.EntriesAddedToCart = true;
            }
        }

        private void UpdateLineItemSku(ICart cart, int shipmentId, string oldCode, string newCode, decimal quantity, string displayName)
        {
            RemoveLineItem(cart, shipmentId, oldCode);

            // Merge same sku's.
            var newLineItem = GetFirstLineItem(cart, newCode);
            if (newLineItem != null)
            {
                var shipment = cart.GetFirstForm().Shipments.First(s => s.ShipmentId == shipmentId || shipmentId == 0);
                cart.UpdateLineItemQuantity(shipment, newLineItem, newLineItem.Quantity + quantity);
            }
            else
            {
                AddNewLineItem(cart, newCode, quantity, displayName);
            }

            ValidateCart(cart);
        }

        private ILineItem AddNewLineItem(ICart cart, string newCode, decimal quantity, string displayName)
        {
            var newLineItem = cart.CreateLineItem(newCode, _orderGroupFactory);
            newLineItem.Quantity = quantity;
            newLineItem.DisplayName = displayName;
            cart.AddLineItem(newLineItem, _orderGroupFactory);

            var price = _pricingService.GetPrice(newCode);
            if (price != null)
            {
                newLineItem.PlacedPrice = price.UnitPrice.Amount;
            }

            return newLineItem;
        }

        private void ChangeQuantity(ICart cart, int shipmentId, string code, decimal quantity)
        {
            var shipment = cart.GetFirstForm().Shipments.First(s => s.ShipmentId == shipmentId);
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
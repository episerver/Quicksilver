using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Cart.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Services
{
    [ServiceConfiguration(typeof(ICartService), Lifecycle = ServiceInstanceScope.Unique)]
    public class CartService : ICartService
    {
        private readonly Func<string, CartHelper> _cartHelper;
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;
        private readonly CultureInfo _preferredCulture;
        private string _cartName = Mediachase.Commerce.Orders.Cart.DefaultName;
        private readonly UrlResolver _urlResolver;
        private readonly IProductService _productService;
        private readonly IPricingService _pricingService;

        private readonly IPromotionService _promotionService;
        private AppContextFacade _appContext;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyService;

        public CartService(Func<string, CartHelper> cartHelper,
            IContentLoader contentLoader,
            ReferenceConverter referenceConverter,
            UrlResolver urlResolver,
            IProductService productService,
            IPricingService pricingService,
            IPromotionService promotionService,
            AppContextFacade appContext,
            ICurrentMarket currentMarket,
            ICurrencyService currencyService)
        {
            _cartHelper = cartHelper;
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _preferredCulture = ContentLanguage.PreferredCulture;
            _urlResolver = urlResolver;
            _productService = productService;
            _pricingService = pricingService;
            _promotionService = promotionService;
            _appContext = appContext;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
        }

        public void InitializeAsWishList()
        {
            _cartName = CartHelper.WishListName;
        }

        public decimal GetLineItemsTotalQuantity()
        {
            return CartHelper.Cart.GetAllLineItems().Sum(x => x.Quantity);
        }

        public CartItem[] GetCartItems()
        {
            if (CartHelper.IsEmpty)
            {
                return new CartItem[0];
            }

            var cartItems = new List<CartItem>();
            var lineItems = CartHelper.Cart.GetAllLineItems();

            // In order to show the images for the items in the cart, we need to load the variants
            var variants = _contentLoader.GetItems(lineItems.Select(x => _referenceConverter.GetContentLink(x.Code)),
                _preferredCulture).OfType<VariationContent>();

            var marketId = _currentMarket.GetCurrentMarket().MarketId;
            var currency = _currencyService.GetCurrentCurrency();

            foreach (var lineItem in lineItems)
            {
                VariationContent variant = variants.FirstOrDefault(x => x.Code == lineItem.Code);

                if (variant == null)
                {
                    RemoveLineItem(lineItem.Code);
                    continue;
                }

                CartItem item = new CartItem
                {
                    Code = lineItem.Code,
                    DisplayName = lineItem.DisplayName,
                    ImageUrl = variant.GetAssets<IContentImage>(_contentLoader, _urlResolver).FirstOrDefault() ?? "",
                    Currency = currency,
                    ExtendedPrice = GetExtendedPrice(lineItem, marketId, currency),
                    PlacedPrice = lineItem.PlacedPrice,
                    Quantity = lineItem.Quantity,
                    Url = lineItem.GetUrl(),
                    Variant = variant,
                    Discounts = lineItem.Discounts.Select(x => new OrderDiscountModel
                    {
                        Discount = new Money(x.DiscountAmount, new Currency(CartHelper.Cart.BillingCurrency)),
                        Displayname = x.DisplayMessage
                    }),
                    IsAvailable = _pricingService.GetCurrentPrice(variant.Code).HasValue
                };

                var discountAmount = currency.Round(lineItem.Discounts.Sum(x => x.DiscountValue) / lineItem.Quantity);
                if (discountAmount > 0)
                {
                    item.DiscountPrice = lineItem.ToMoney(lineItem.PlacedPrice - discountAmount);
                }

                if (!string.IsNullOrEmpty(lineItem.ShippingAddressId))
                {
                    item.AddressId = new Guid(lineItem.ShippingAddressId);
                }

                ProductContent product = _contentLoader.Get<ProductContent>(variant.GetParentProducts().FirstOrDefault());
                if (product is FashionProduct)
                {
                    var fashionProduct = (FashionProduct)product;
                    var fashionVariant = (FashionVariant)variant;
                    item.Brand = fashionProduct.Brand;
                    var variations = _productService.GetVariations(fashionProduct);
                    item.AvailableSizes = variations.Where(x => x.Color == fashionVariant.Color).Select(x => x.Size);
                }

                cartItems.Add(item);
            }

            return cartItems.ToArray();
        }

        private Money? GetExtendedPrice(LineItem lineItem, MarketId marketId, Currency currency)
        {
            if (_cartName.Equals(CartHelper.WishListName))
            {
                var discountPrice = _promotionService.GetDiscountPrice(new CatalogKey(_appContext.ApplicationId, lineItem.Code), marketId, currency);

                if (discountPrice == null)
                {
                    return null;
                }

                return discountPrice.UnitPrice;
            }

            return lineItem.ToMoney(lineItem.ExtendedPrice + lineItem.OrderLevelDiscountAmount);
        }

        public Money GetTotal()
        {
            if (CartHelper.IsEmpty)
            {
                return ConvertToMoney(0);
            }

            return ConvertToMoney(CartHelper.Cart.Total);
        }

        public Money GetTotalDiscount()
        {
            decimal amount = 0;

            if (CartHelper.IsEmpty)
            {
                return ConvertToMoney(amount);
            }

            amount = CartHelper.Cart.GetAllLineItems().Sum(x => x.LineItemDiscountAmount);

            return ConvertToMoney(amount);
        }

        public bool AddToCart(string code, out string warningMessage)
        {
            var entry = CatalogContext.Current.GetCatalogEntry(code);
            CartHelper.AddEntry(entry);
            CartHelper.Cart.ProviderId = "frontend"; // if this is not set explicitly, place price does not get updated by workflow
            ValidateCart(out warningMessage);

            return CartHelper.LineItems.Select(x => x.Code).Contains(code);
        }

        public void UpdateLineItemSku(string oldCode, string newCode, decimal quantity)
        {
            //merge same sku's
            var newLineItem = CartHelper.Cart.GetLineItem(newCode);
            if (newLineItem != null)
            {
                newLineItem.Quantity += quantity;
                RemoveLineItem(oldCode);
                newLineItem.AcceptChanges();
                ValidateCart();
                return;
            }

            var lineItem = CartHelper.Cart.GetLineItem(oldCode);
            var entry = CatalogContext.Current.GetCatalogEntry(newCode,
                new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Variations));

            lineItem.Code = entry.ID;
            lineItem.MaxQuantity = entry.ItemAttributes.MaxQuantity;
            lineItem.MinQuantity = entry.ItemAttributes.MinQuantity;
            lineItem.InventoryStatus = (int)entry.InventoryStatus;

            var price = _pricingService.GetCurrentPrice(newCode);

            if (price.HasValue)
            {
                lineItem.ListPrice = price.Value.Amount;
                lineItem.PlacedPrice = price.Value.Amount;
            }

            ValidateCart();
            lineItem.AcceptChanges();
        }

        public void ChangeQuantity(string code, decimal quantity)
        {
            if (quantity == 0)
            {
                RemoveLineItem(code);
            }
            var lineItem = CartHelper.Cart.GetLineItem(code);
            if (lineItem != null)
            {
                lineItem.Quantity = quantity;
                UpdateLineItemQuantityToShipments(lineItem);
                ValidateCart();
                CartHelper.Cart.AcceptChanges();
            }
        }

        private void UpdateLineItemQuantityToShipments(LineItem lineItem)
        {
            var shipment = lineItem.Parent.Shipments.FirstOrDefault(); //It look not nice but should be OK for now because quicksilver only able to updated quantity of line items with single shipment even checkout with multi shipments.
            int lineItemIndex = lineItem.Parent.LineItems.IndexOf(lineItem);
            if (shipment != null && shipment.LineItemIndexes.Contains(lineItemIndex.ToString()))
            {
                shipment.SetLineItemQuantity(lineItemIndex, lineItem.Quantity);
            }
        }

        public void SetCartCurrency(Currency currency)
        {
            if (currency != CartHelper.Cart.BillingCurrency)
            {
                CartHelper.Cart.BillingCurrency = currency;
                ValidateCart();
                CartHelper.Cart.AcceptChanges();
            }
        }

        public void RemoveLineItem(string code)
        {
            var lineItem = CartHelper.Cart.GetLineItem(code);
            if (lineItem != null)
            {
                PurchaseOrderManager.RemoveLineItemFromOrder(CartHelper.Cart, lineItem.LineItemId);
                ValidateCart();
                CartHelper.Cart.AcceptChanges();
            }
        }

        private void ValidateCart()
        {
            string warningMessage = null;
            ValidateCart(out warningMessage);
        }

        private void ValidateCart(out string warningMessage)
        {
            if (_cartName == Mediachase.Commerce.Website.Helpers.CartHelper.WishListName)
            {
                warningMessage = null;
                return;
            }

            var workflowResult = OrderGroupWorkflowManager.RunWorkflow(CartHelper.Cart, OrderGroupWorkflowManager.CartValidateWorkflowName);
            var warnings = OrderGroupWorkflowManager.GetWarningsFromWorkflowResult(workflowResult).ToArray();
            warningMessage = warnings.Any() ? String.Join(" ", warnings) : null;
        }

        public Money ConvertToMoney(decimal amount)
        {
            return new Money(amount, new Currency(CartHelper.Cart.BillingCurrency));
        }

        public Money GetSubTotal()
        {
            decimal amount = CartHelper.Cart.SubTotal + CartHelper.Cart.OrderForms.SelectMany(x => x.Discounts).Sum(x => x.DiscountAmount);

            return ConvertToMoney(amount);
        }

        public Money GetShippingSubTotal()
        {
            decimal shippingTotal = CartHelper.Cart.OrderForms.SelectMany(x => x.Shipments).Sum(x => x.ShippingSubTotal);

            return ConvertToMoney(shippingTotal);
        }

        public Money GetShippingTotal()
        {
            return ConvertToMoney(CartHelper.Cart.ShippingTotal);
        }

        public Money GetTaxTotal()
        {
            return ConvertToMoney(CartHelper.Cart.TaxTotal);
        }

        public Money GetShippingTaxTotal()
        {
            decimal amount = CartHelper.Cart.ShippingTotal + CartHelper.Cart.TaxTotal;

            return ConvertToMoney(amount);
        }

        public Money GetOrderDiscountTotal()
        {
            decimal amount = GetOrderForms().SelectMany(x => x.Discounts).Sum(x => x.DiscountValue);

            return ConvertToMoney(amount);
        }

        public Money GetShippingDiscountTotal()
        {
            decimal amount = GetOrderForms().SelectMany(x => x.Shipments).SelectMany(x => x.Discounts).Sum(x => x.DiscountValue);

            return ConvertToMoney(amount);
        }

        public IEnumerable<OrderForm> GetOrderForms()
        {
            return CartHelper.Cart.OrderForms.Count == 0 ? new[] { new OrderForm() } : CartHelper.Cart.OrderForms.ToArray();
        }

        public IEnumerable<Shipment> GetShipments()
        {
            return CartHelper.Cart.OrderForms.SelectMany(x => x.Shipments);
        }

        public void RunWorkflow(string workFlowName)
        {
            RunWorkflow(workFlowName, null);
        }

        public void RunWorkflow(string workFlowName, Dictionary<string, object> context)
        {
            if (_cartName == CartHelper.WishListName)
            {
                throw new ArgumentException("Running workflows are not supported for wishlist carts.");
            }

            CartHelper.RunWorkflow(workFlowName, context);
        }

        public void SaveCart()
        {
            CartHelper.Cart.AcceptChanges();
        }

        public void DeleteCart()
        {
            CartHelper.Cart.Delete();
            CartHelper.Cart.AcceptChanges();
        }

        public void ResetLineItemAddresses()
        {
            foreach (var item in CartHelper.Cart.GetAllLineItems())
            {
                item.ShippingAddressId = string.Empty;
            }

            //clear shipments and recreate it when recreating line items
            GetOrderForms().First().Shipments.Clear();

            RecreateLineItemsBasedOnAddresses(GetCartItems().ToList());
        }

        public void RecreateLineItemsBasedOnAddresses(IEnumerable<CartItem> cartItems)
        {
            var form = GetOrderForms().First();
            foreach (var item in form.LineItems.ToArray())
            {
                item.Delete();
            }

            var newCartItems = cartItems
                .GroupBy(x => new { x.AddressId, x.Code })
                .Select(x => new
                {
                    Code = x.Key.Code,
                    AddressId = x.Key.AddressId,
                    Quantity = x.Sum(item => item.Quantity),
                    DisplayName = x.First().DisplayName,
                    PlacedPrice = x.First().PlacedPrice
                });

            foreach (var item in newCartItems)
            {
                LineItem lineItem = form.LineItems.AddNew();
                lineItem.Code = item.Code;
                lineItem.Quantity = item.Quantity;
                lineItem.PlacedPrice = item.PlacedPrice;
                lineItem.DisplayName = item.DisplayName;
                lineItem.ShippingAddressId = item.AddressId.ToString();
            }

            RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            SaveCart();
        }

        public OrderAddress GetOrderAddress(Guid addressId)
        {
            return CartHelper.Cart.OrderAddresses.FirstOrDefault(a => a.Name == addressId.ToString());
        }

        private CartHelper CartHelper
        {
            get { return _cartHelper(_cartName); }
        }
    }
}
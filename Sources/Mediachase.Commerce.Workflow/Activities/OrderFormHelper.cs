using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Customization
{
    internal static class OrderFormHelper
    {
        private static Injected<ReferenceConverter> _referenceConverter;
        private static Injected<IContentRepository> _contentRepository;
        private static Injected<IMarketService> _marketService;

        /// <summary>
        /// Gets the taxes for order form
        /// </summary>
        /// <param name="form">The order form.</param>
        /// <param name="marketId">The order market Id.</param>
        /// <param name="currency">The order currency.</param>
        /// <returns>The total taxes for the order form.</returns>
        public static void CalculateTaxes(OrderForm form, MarketId marketId, Currency currency)
        {
            var market = _marketService.Service.GetMarket(marketId);
            var totalTaxes = 0m;
            foreach (Shipment shipment in form.Shipments)
            {
                var shippingTax = 0m;
                var shippingCost = shipment.ShippingSubTotal - shipment.ShippingDiscountAmount;
                var lineItems = Shipment.GetShipmentLineItems(shipment);
                // Calculate sales and shipping taxes per items
                foreach (LineItem lineItem in lineItems)
                {
                    var taxes = GetTaxValues(market, shipment, lineItem);
                    if (!taxes.Any())
                    {
                        continue;
                    }

                    // calculate quantity of item in current shipment
                    var quantity = OrderForm.IsReturnOrderForm(form) ? lineItem.ReturnQuantity : Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId);
                    // price exclude tax for 1 line item
                    var lineItemPricesExcTax = GetPriceExcludingTax(lineItem, quantity);
                    var totalShipmentLineItemsQuantity = lineItems.Sum(l => Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId));
                    var itemShippingCost = shipment.SubTotal == 0 ? quantity / totalShipmentLineItemsQuantity * shippingCost : lineItemPricesExcTax / shipment.SubTotal * shippingCost;

                    shippingTax += GetTaxesAmount(taxes, TaxType.ShippingTax, itemShippingCost);
                    totalTaxes += GetTaxesAmount(taxes, TaxType.SalesTax, lineItemPricesExcTax);
                }

                shipment.ShippingTax = currency.Round(shippingTax);
                totalTaxes += shipment.ShippingTax;
            }

            form.TaxTotal = currency.Round(totalTaxes);
        }

        /// <summary>
        /// Gets the sales tax total for the shipment.
        /// </summary>
        /// <param name="marketId">The order market Id.</param>
        /// <param name="shipment">The shipment.</param>
        /// <param name="currency">The currency to be used in the calculation.</param>
        /// <returns>The sales tax amount for the shipment.</returns>
        public static Money CalculateSalesTaxTotal(MarketId marketId, IShipment shipment, Currency currency)
        {
            var market = _marketService.Service.GetMarket(marketId);
            var salesTax = 0m;
            foreach (var lineItem in shipment.LineItems)
            {
                salesTax += GetTaxesAmount(GetTaxValues(market, shipment, lineItem), TaxType.SalesTax, GetPriceExcludingTax(lineItem, lineItem.Quantity));
            }

            return new Money(currency.Round(salesTax), currency);
        }

        private static IEnumerable<ITaxValue> GetTaxValues(IMarket market, IShipment shipment, ILineItem lineItem)
        {
            var emptyResult = Enumerable.Empty<ITaxValue>();

            int taxCategoryId;
            if (!TryGetTaxCategoryId(lineItem, out taxCategoryId))
            {
                return emptyResult;
            }

            ITaxValue[] taxes;
            if (!TryGetTaxValues(market, shipment, taxCategoryId, out taxes))
            {
                return emptyResult;
            }

            return taxes;
        }

        private static bool TryGetTaxCategoryId(ILineItem item, out int taxCategoryId)
        {
            var reference = _referenceConverter.Service.GetContentLink(item.Code);
            if (ContentReference.IsNullOrEmpty(reference))
            {
                taxCategoryId = 0;

                return false;
            }

            var entry = _contentRepository.Service.Get<EntryContentBase>(reference);
            var pricingContent = entry as IPricing;
            if (pricingContent == null || !pricingContent.TaxCategoryId.HasValue)
            {
                taxCategoryId = 0;

                return false;
            }

            taxCategoryId = pricingContent.TaxCategoryId.Value;

            return true;
        }

        private static bool TryGetTaxValues(IMarket market, IShipment shipment, int taxCategoryId, out ITaxValue[] taxValues)
        {
            if (shipment.ShippingAddress == null)
            {
                taxValues = Enumerable.Empty<ITaxValue>().ToArray();

                return false;
            }

            var taxValueCollection = OrderContext.Current.GetTaxes(Guid.Empty, CatalogTaxManager.GetTaxCategoryNameById(taxCategoryId), market.DefaultLanguage.Name, shipment.ShippingAddress);
            if (taxValueCollection == null)
            {
                taxValues = Enumerable.Empty<ITaxValue>().ToArray();

                return false;
            }

            taxValues = taxValueCollection.ToArray();

            return taxValues.Any();
        }

        /// <summary>
        /// Get Item Price Excluding Tax
        /// </summary>
        /// <param name="item">The line item</param>
        /// <param name="quantity">The line item quantity</param>
        /// <returns>Item price excluding tax</returns>
        private static decimal GetPriceExcludingTax(ILineItem item, decimal quantity)
        {
            return (item.PlacedPrice - (item.TryGetDiscountValue(x => x.OrderAmount) + item.TryGetDiscountValue(x => x.EntryAmount)) / item.Quantity) * quantity;
        }

        /// <summary>
        /// Calculate the tax for specific tax type
        /// </summary>
        /// <param name="taxes">The taxes</param>
        /// <param name="taxType">The tax type</param>
        /// <param name="unitPrice">The item price excluding taxes or the shipping cost</param>        
        /// <returns>The tax value</returns>
        private static decimal GetTaxesAmount(IEnumerable<ITaxValue> taxes, TaxType taxType, decimal unitPrice)
        {
            return taxes.Where(x => x.TaxType == taxType).Sum(x => unitPrice * (decimal)x.Percentage * 0.01m);
        }

        /// <summary>
        /// Gets the name of the address by name.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static OrderAddress GetAddressByName(OrderForm form, string name)
        {
            foreach (OrderAddress address in form.Parent.OrderAddresses)
            {
                if (address.Name.Equals(name))
                {
                    return address;
                }
            }

            return null;
        }
    }
}
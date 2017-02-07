using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Orders;
using System;
using System.Linq;
using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;

namespace EPiServer.Reference.Customization
{
    public class CustomizedTaxCalculator : ITaxCalculator
    {
        private readonly IContentRepository _contentRepository;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IShippingCalculator _shippingCalculator;

        public CustomizedTaxCalculator(IContentRepository contentRepository,
            ReferenceConverter referenceConverter,
            IShippingCalculator shippingCalculator)
        {
            _contentRepository = contentRepository;
            _referenceConverter = referenceConverter;
            _shippingCalculator = shippingCalculator;
        }

        public Money GetShippingTaxTotal(IShipment shipment, IMarket market, Currency currency)
        {
            if (shipment.ShippingAddress == null)
            {
                return new Money(0m, currency);
            }

            var shippingTaxes = new List<ITaxValue>();
            foreach (var item in shipment.LineItems)
            {
                //get the variation entry that the item is associated with
                var reference = _referenceConverter.GetContentLink(item.Code);
                var entry = _contentRepository.Get<VariationContent>(reference);
                if (entry == null || !entry.TaxCategoryId.HasValue)
                {
                    continue;
                }

                //get the tax values applicable for the entry. If not taxes found then continue with next line item.
                var taxCategory = CatalogTaxManager.GetTaxCategoryNameById(entry.TaxCategoryId.Value);

                var taxes = GetTaxValues(taxCategory, market.DefaultLanguage.Name, shipment.ShippingAddress).ToList();
                if (taxes.Count <= 0)
                {
                    continue;
                }

                shippingTaxes.AddRange(
                        taxes.Where(x => x.TaxType == TaxType.ShippingTax &&
                                            !shippingTaxes.Any(y => y.Name.Equals(x.Name))));
            }

            return new Money(CalculateShippingTax(shippingTaxes, shipment, market, currency), currency);
        }

        public Money GetTaxTotal(IOrderGroup orderGroup, IMarket market, Currency currency)
        {
            //iterate over all order forms in this order group and sum the results.
            return new Money(orderGroup.Forms.Sum(form => GetTaxTotal(form, market, currency).Amount), currency);
        }

        public Money GetTaxTotal(IOrderForm orderForm, IMarket market, Currency currency)
        {
            var formTaxes = 0m;

            //calculate tax for each shipment
            foreach (var shipment in orderForm.Shipments.Where(x => x.ShippingAddress != null))
            {
                var shippingTaxes = new List<ITaxValue>();
                foreach (var item in shipment.LineItems)
                {
                    //get the variation entry that the item is associated with
                    var reference = _referenceConverter.GetContentLink(item.Code);
                    var entry = _contentRepository.Get<VariationContent>(reference);
                    if (entry == null || !entry.TaxCategoryId.HasValue)
                    {
                        continue;
                    }

                    //get the tax values applicable for the entry. If not taxes found then continue with next line item.
                    var taxCategory = CatalogTaxManager.GetTaxCategoryNameById(entry.TaxCategoryId.Value);
                    var taxes = GetTaxValues(taxCategory, market.DefaultLanguage.Name, shipment.ShippingAddress).ToList();
                    if (taxes.Count <= 0)
                    {
                        continue;
                    }

                    //calculate the line item price, excluding tax
                    var lineItem = (LineItem)item;
                    var lineItemPricesExcludingTax = item.PlacedPrice - (lineItem.OrderLevelDiscountAmount + lineItem.LineItemDiscountAmount) / item.Quantity;
                    var quantity = 0m;

                    if (orderForm.Name.Equals(OrderForm.ReturnName))
                    {
                        quantity = item.ReturnQuantity;
                    }
                    else
                    {
                        quantity = item.Quantity;
                    }

                    formTaxes += taxes.Where(x => x.TaxType == TaxType.SalesTax).Sum(x => lineItemPricesExcludingTax * (decimal)x.Percentage * 0.01m) * quantity;

                    //add shipping taxes for later tax calculation
                    shippingTaxes.AddRange(
                        taxes.Where(x => x.TaxType == TaxType.ShippingTax &&
                                            !shippingTaxes.Any(y => y.Name.Equals(x.Name))));
                }

                //sum the calculated tax for each shipment
                formTaxes += CalculateShippingTax(shippingTaxes, shipment, market, currency);
            }

            return new Money(currency.Round(formTaxes), currency);
        }

        private static IEnumerable<ITaxValue> GetTaxValues(string taxCategory, string languageCode, IOrderAddress orderAddress)
        {
            return OrderContext.Current.GetTaxes(Guid.Empty, taxCategory, languageCode, orderAddress);
        }

        private decimal CalculateShippingTax(IEnumerable<ITaxValue> taxes, IShipment shipment, IMarket market, Currency currency)
        {
            //calculate shipping cost for the shipment, for specified market and currency.
            var shippingCost = _shippingCalculator.GetShippingCost(shipment, market, currency).Amount;

            return taxes.Where(x => x.TaxType == TaxType.ShippingTax).Sum(x => shippingCost * (decimal)x.Percentage * 0.01m);
        }
    }
}
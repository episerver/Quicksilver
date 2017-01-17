using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Customization
{
    /// <summary>
    /// Calculate discounts using the new promotion engine
    /// </summary>
    public class CalculateDiscountsVNextActivity : OrderGroupActivityBase
    {
        /// <summary>
        /// Executes the specified execution context.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <returns></returns>
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            ValidateRuntime();
            ServiceLocator.Current.GetInstance<IPromotionEngine>().Run(OrderGroup);

            foreach (OrderForm form in OrderGroup.OrderForms)
            {
                ProcessManualDiscounts(form);
            }

            var referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var order = OrderGroup as IOrderGroup;
            var entryCodes = GetAffectedEntryCodes(order);
            var codeContentLinkMap = referenceConverter.GetContentLinks(entryCodes);

            UpdateGiftLineItems();

            foreach (var contentLink in codeContentLinkMap.Values)
            {
                var item = contentLoader.Get<EntryContentBase>(contentLink);
                if (!(item is IPricing))
                {
                    continue;
                }

                foreach (var lineItem in order.Forms.SelectMany(x => x.Shipments).SelectMany(x => x.LineItems).Where(x => x.Code.Equals(item.Code)))
                {
                    var metaItem = OrderGroup.OrderForms.SelectMany(x => x.LineItems).FirstOrDefault(x => x.LineItemId == lineItem.LineItemId);
                    if (metaItem == null)
                    {
                        continue;
                    }

                    metaItem.TrySetDiscountValue(x => x.EntryAmount, lineItem.TryGetDiscountValue(y => y.EntryAmount));
                    metaItem.TrySetDiscountValue(x => x.OrderAmount, lineItem.TryGetDiscountValue(y => y.OrderAmount));
                }
            }

            return ActivityExecutionStatus.Closed;
        }

        private void ProcessManualDiscounts(OrderForm orderForm)
        {
            var form = (IOrderForm)orderForm;
            var promotionInfos = new List<PromotionInformation>();

            foreach (LineItem lineItem in orderForm.LineItems)
            {
                var lineItemManualDiscountAmount = 0m;

                foreach (var discount in lineItem.Discounts)
                {
                    if (discount.DiscountName.StartsWith("@"))
                    {
                        if (discount.DiscountName.EndsWith(":PercentageBased", StringComparison.OrdinalIgnoreCase))
                        {
                            discount.DiscountValue = discount.DiscountAmount * 0.01m * lineItem.PlacedPrice * lineItem.Quantity;
                        }
                        else
                        {
                            discount.DiscountValue = discount.DiscountAmount * lineItem.Quantity;
                        }

                        lineItemManualDiscountAmount += discount.DiscountValue;

                        var promotionInfoEntries = new List<PromotionInformationEntry>()
                        {
                            new PromotionInformationEntry()
                            {
                                EntryCode = lineItem.Code,
                                SavedAmount = discount.DiscountValue
                            }
                        };
                        promotionInfos.Add(new PromotionInformation()
                        {
                            Name = discount.DiscountName.Substring(1),
                            Description = discount.DiscountName.Substring(1),
                            DiscountType = DiscountType.Manual,
                            Entries = promotionInfoEntries,
                            OrderForm = new PromotionInformationOrderForm() { SavedAmount = 0 }
                        });
                    }
                }

                var metaItem = form.Shipments.SelectMany(x => x.LineItems).FirstOrDefault(x => x.LineItemId == lineItem.LineItemId);
                if (metaItem == null)
                {
                    continue;
                }

                var extendedPrice = metaItem.GetExtendedPrice(orderForm.Parent.BillingCurrency).Amount;
                lineItemManualDiscountAmount = Math.Min(extendedPrice, lineItemManualDiscountAmount);
                metaItem.TrySetDiscountValue(x => x.EntryAmount, metaItem.TryGetDiscountValue(x => x.EntryAmount) + lineItemManualDiscountAmount);
            }

            promotionInfos.ForEach(p => form.Promotions.Add(p));
        }

        private void UpdateGiftLineItems()
        {
            if (!OrderGroup.GetAllLineItems().Any(item => item.IsGift))
            {
                return;
            }

            UpdateGiftLineItemWarehouse();
            UpdateGiftLineItemIndexQuantity();
        }

        private void UpdateGiftLineItemWarehouse()
        {
            var warehouseCode = WarehouseRepository.GetDefaultWarehouse().Code;

            foreach (OrderForm orderForm in OrderGroup.OrderForms)
            {
                foreach (Shipment shipment in orderForm.Shipments)
                {
                    var fulfillmentWarehouse = FulfillmentWarehouseProcessor.GetFulfillmentWarehouse(shipment);
                    if (fulfillmentWarehouse != null)
                    {
                        warehouseCode = fulfillmentWarehouse.Code;
                    }

                    var giftItemCollection = Shipment.GetShipmentLineItems(shipment).Where(x => x.IsGift);
                    foreach (var giftItem in giftItemCollection)
                    {
                        giftItem.WarehouseCode = warehouseCode;
                    }
                }
            }
        }

        private void UpdateGiftLineItemIndexQuantity()
        {
            foreach (OrderForm orderForm in OrderGroup.OrderForms)
            {
                foreach (Shipment shipment in orderForm.Shipments)
                {
                    foreach (var giftItem in shipment.LineItems.Where(item => item.IsGift))
                    {
                        var index = GetLineItemIndex(orderForm, giftItem);
                        if (index > -1 && shipment.LineItemIndexes.Contains(index.ToString()))
                        {
                            shipment.SetLineItemQuantity(index, giftItem.Quantity);
                        }
                    }
                }
            }
        }

        private int GetLineItemIndex(OrderForm form, ILineItem lineItem)
        {
            for (int i = 0; i < form.LineItems.Count; i++)
            {
                if (form.LineItems[i].LineItemId == lineItem.LineItemId)
                {
                    return i;
                }
            }
            return -1;
        }

        private static IEnumerable<string> GetAffectedEntryCodes(IOrderGroup order)
        {
            if (HasOrderPromotionWithSavedAmount(order))
            {
                return GetAllEntryCodesFromOrder(order);
            }

            return GetAffectedEntryCodesFromEntryPromotions(order);
        }

        private static bool HasOrderPromotionWithSavedAmount(IOrderGroup order)
        {
            return order.Forms.SelectMany(f => f.Promotions).Any(x => x.OrderForm.SavedAmount > 0);
        }

        private static IEnumerable<string> GetAffectedEntryCodesFromEntryPromotions(IOrderGroup order)
        {
            return order.Forms.SelectMany(f => f.Promotions).SelectMany(p => p.Entries).Select(i => i.EntryCode);
        }

        private static IEnumerable<string> GetAllEntryCodesFromOrder(IOrderGroup order)
        {
            return order.Forms.SelectMany(x => x.Shipments).SelectMany(x => x.LineItems).Select(x => x.Code);
        }
    }
}

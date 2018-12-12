using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Commerce.Orders.Cloud.Client.OrderModel;
using Mediachase.Commerce.Orders;
using System.Linq;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace EPiServer.Commerce.Orders.Cloud.Client
{
    public class CloudyPurchaseOrderFactory : IPurchaseOrderFactory
    {
        private readonly IReturnOrderNumberGenerator _returnOrderNumberGenerator;

        public CloudyPurchaseOrderFactory(IReturnOrderNumberGenerator returnOrderNumberGenerator)
        {
            _returnOrderNumberGenerator = returnOrderNumberGenerator;
        }

        public IReturnOrderForm CreateReturnOrderForm(IPurchaseOrder purchaseOrder)
        {
            return new CloudyReturnOrderForm
            {
                Name = OrderForm.ReturnName,
                OriginalOrderFormId = purchaseOrder.Forms.FirstOrDefault()?.OrderFormId,
                ReturnType = ReturnType.Refund,
                Status = ReturnFormStatus.AwaitingStockReturn,
                RMANumber = _returnOrderNumberGenerator.GenerateReturnOrderFormNumber(purchaseOrder)
            };
        }

        public IShipment CreateReturnShipment(IShipment originalShipment)
        {
            var shipment = new CloudyShipment
            {
                ShippingMethodId = originalShipment.ShippingMethodId,
                ShippingMethodName = originalShipment.ShippingMethodName,
                ShipmentTrackingNumber = originalShipment.ShipmentId.ToString()
            };

            ((IShipment)shipment).ShippingAddress = originalShipment.ShippingAddress;
            return shipment;
        }

        public IReturnLineItem CreateReturnLineItem(ILineItem originalLineItem, decimal returnQuantity, string returnReason)
        {
            var returnItem = new CloudyReturnLineItem
            {
                Code = originalLineItem.Code,
                DisplayName = originalLineItem.DisplayName,
                PlacedPrice = originalLineItem.PlacedPrice,
                OriginalLineItemId = originalLineItem.LineItemId,
                Quantity = originalLineItem.Quantity,
                ReturnQuantity = returnQuantity,
                ReturnReason = returnReason,
                IsGift = originalLineItem.IsGift
            };

            returnItem.SetEntryDiscountValue(originalLineItem.GetEntryDiscountValue());
            returnItem.SetOrderDiscountValue(originalLineItem.GetOrderDiscountValue());
            ((IReturnLineItem)returnItem).InventoryTrackingStatus = originalLineItem.InventoryTrackingStatus;
            ((ILineItem)returnItem).TaxCategoryId = originalLineItem.TaxCategoryId;
            return returnItem;
        }

        public IPayment CreateExchangePayment()
        {
            var payment = new ExchangePayment();
            var paymentMethods = PaymentManager.GetPaymentMethods("en", true);

            foreach (PaymentMethodDto.PaymentMethodRow row in paymentMethods.PaymentMethod.Rows)
            {
                if (row.SystemKeyword == ExchangePayment.PaymentMethodSystemKeyword)
                {
                    payment.PaymentMethodId = row.PaymentMethodId;
                    payment.PaymentMethodName = row.Name;
                    break;
                }
            }

            return payment;
        }
    }
}

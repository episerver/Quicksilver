using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.InventoryService.BusinessLogic;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Customization
{
    public abstract class OrderGroupActivityBase : Activity
    {
        [NonSerialized]
        private readonly MapUserKey _mapUserKey = new MapUserKey();

        protected IInventoryService InventoryService
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IInventoryService>();
            }
        }

        protected IWarehouseRepository WarehouseRepository
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IWarehouseRepository>();
            }
        }

        protected IFulfillmentWarehouseProcessor FulfillmentWarehouseProcessor
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IFulfillmentWarehouseProcessor>();
            }
        }

        protected DateTime SafeBeginningOfTime
        {
            get
            {
                return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Gets or sets the order group.
        /// </summary>
        /// <value>The order group.</value>
        [ActivityFlowContextProperty]
        public OrderGroup OrderGroup { get; set; }

        /// <summary>
        /// Gets or sets the warnings.
        /// </summary>
        /// <value>The warnings.</value>
        [ActivityFlowContextProperty]
        public StringDictionary Warnings { get; set; }

        /// <summary>
        /// Validates the runtime.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ValidateRuntime()
        {
            // Create a new collection for storing the validation errors
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            // Validate the Order Properties
            ValidateOrderProperties(validationErrors);

            // Validate properties
            ValidateProperties(validationErrors);

            // Raise an exception if we have ValidationErrors
            if (validationErrors.HasErrors)
            {
                string validationErrorsMessage = String.Empty;

                foreach (ValidationError error in validationErrors)
                {
                    validationErrorsMessage +=
                        string.Format("Validation Error:  Number {0} - '{1}' \n",
                        error.ErrorNumber, error.ErrorText);
                }

                // Throw a new exception with the validation errors.
                throw new WorkflowValidationFailedException(validationErrorsMessage, validationErrors);

            }
            // If we made it this far, then the data must be valid.
            return true;
        }

        protected virtual IWarehouse CheckMultiWarehouse()
        {
            var warehouses = WarehouseRepository.List()
                .Where(w => (OrderGroup.ApplicationId == w.ApplicationId) && w.IsActive && w.IsFulfillmentCenter);
            if (warehouses.Count() > 1)
            {
                throw new NotSupportedException("Multiple fulfillment centers without custom fulfillment process.");
            }
            return warehouses.SingleOrDefault();
        }

        /// <summary>
        /// Validates the order properties.
        /// </summary>
        /// <param name="validationErrors">The validation errors.</param>
        protected virtual void ValidateOrderProperties(ValidationErrorCollection validationErrors)
        {
            // Validate the To property
            if (OrderGroup == null)
            {
                ValidationError validationError = ValidationError.GetNotSetValidationError("OrderGroup");
                validationErrors.Add(validationError);
            }
        }

        protected Money? GetItemPrice(CatalogEntryDto.CatalogEntryRow entry, LineItem lineItem, CustomerContact customerContact)
        {
            Currency currency = new Currency(lineItem.Parent.Parent.BillingCurrency);
            List<CustomerPricing> customerPricing = new List<CustomerPricing>();
            customerPricing.Add(CustomerPricing.AllCustomers);
            if (customerContact != null)
            {
                var userKey = _mapUserKey.ToUserKey(customerContact.UserId);
                if (userKey != null && !string.IsNullOrWhiteSpace(userKey.ToString()))
                {
                    customerPricing.Add(new CustomerPricing(CustomerPricing.PriceType.UserName, userKey.ToString()));
                }

                if (!string.IsNullOrEmpty(customerContact.EffectiveCustomerGroup))
                {
                    customerPricing.Add(new CustomerPricing(CustomerPricing.PriceType.PriceGroup, customerContact.EffectiveCustomerGroup));
                }
            }

            IPriceService priceService = ServiceLocator.Current.GetInstance<IPriceService>();

            PriceFilter priceFilter = new PriceFilter()
            {
                Currencies = new List<Currency>() { currency },
                Quantity = lineItem.Quantity,
                CustomerPricing = customerPricing,
                ReturnCustomerPricing = false // just want one value
            };
            // Get the lowest price among all the prices matching the parameters
            IPriceValue priceValue = priceService
                .GetPrices(lineItem.Parent.Parent.MarketId, FrameworkContext.Current.CurrentDateTime, new CatalogKey(entry), priceFilter)
                .OrderBy(pv => pv.UnitPrice)
                .FirstOrDefault();

            if (priceValue != null)
            {
                return priceValue.UnitPrice;
            }

            if (lineItem.PlacedPrice != 0)
            {
                return new Money(lineItem.PlacedPrice, currency);
            }

            return null;
        }

        [Obsolete("Use PopulateInventoryInfo(InventoryRecord, LineItem) instead. Will remain at least until July 2017.")]
        protected void PopulateInventoryInfo(IWarehouseInventory inv, LineItem lineItem)
        {
            PopulateInventoryInfo(InventoryToRecord(inv), lineItem);
        }

        protected void PopulateInventoryInfo(InventoryRecord inventoryRecord, LineItem lineItem)
        {
            if (inventoryRecord != null)
            {
                lineItem.AllowBackordersAndPreorders = inventoryRecord.BackorderAvailableQuantity > 0 | inventoryRecord.PreorderAvailableUtc > SafeBeginningOfTime;
                // Init quantities once
                lineItem.BackorderQuantity = inventoryRecord.BackorderAvailableQuantity;
                lineItem.InStockQuantity = inventoryRecord.PurchaseAvailableQuantity;
                lineItem.PreorderQuantity = inventoryRecord.PreorderAvailableQuantity;
                lineItem.InventoryStatus = (int)(inventoryRecord.IsTracked ? InventoryTrackingStatus.Enabled : InventoryTrackingStatus.Disabled);
            }
            else
            {
                var baseEntry = CatalogContext.Current.GetCatalogEntry(lineItem.Code,
                    new CatalogEntryResponseGroup(
                        CatalogEntryResponseGroup.ResponseGroup.CatalogEntryInfo |
                        CatalogEntryResponseGroup.ResponseGroup.Variations));
                lineItem.AllowBackordersAndPreorders = false;
                lineItem.InStockQuantity = 0;
                lineItem.PreorderQuantity = 0;
                lineItem.InventoryStatus = (int)baseEntry.InventoryStatus;
            }
        }

        protected void AddWarningSafe(StringDictionary warnings, string key, string value)
        {
            string uniqueKey, uniqueKeyPrefix = key + '-';

            int counter = 1;
            do
            {
                string suffix = counter.ToString(CultureInfo.InvariantCulture);
                uniqueKey = uniqueKeyPrefix + suffix;
                ++counter;
            }
            while (warnings.ContainsKey(uniqueKey));

            warnings.Add(uniqueKey, value);
        }

        /// <summary>
        /// Gets the type of the resulting payments by transaction.
        /// </summary>
        /// <param name="payments">The payments.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        [Obsolete("Use IOrderForm.GetPaymentsByTransactionType(payments, type) - extension method of IOrderForm instead. Will remain at least until November 2017.")]
        protected IEnumerable<Payment> GetResultingPaymentsByTransactionType(IEnumerable<Payment> payments, TransactionType type)
        {
            List<Payment> retVal = new List<Payment>();
            var paymentsWithSameTranType = GetPaymentsByTransactionType(payments, new TransactionType[] { type });
            foreach (var payment in paymentsWithSameTranType)
            {
                //Get all related payments only in Processing status
                var allRelatedPayments = GetAllRelatedPayments(payment).Where(x => PaymentStatusManager.GetPaymentStatus(x) == PaymentStatus.Processed);
                //do not return authorization payments which have Void related payments, or it has been Captured all amount.
                if (type == TransactionType.Authorization)
                {
                    var anyVoidTransaction = allRelatedPayments.Any(x => GetPaymentTransactionType(x) == TransactionType.Void);
                    var anyCaptureTransaction = allRelatedPayments.Any(x => GetPaymentTransactionType(x) == TransactionType.Capture);
                    var capturedAmount = allRelatedPayments.Where(x => GetPaymentTransactionType(x) == TransactionType.Capture).Sum(x => x.Amount);
                    if (!anyVoidTransaction)
                    {
                        if (anyCaptureTransaction)
                        {
                            // get total captured amount, then check if it is lower than the authorized amount to return the payment.
                            // in other word, we will not return the payment if all of authorized amount has been captured, which means no need to capture anymore.
                            if (capturedAmount < payment.Amount)
                            {
                                yield return payment;
                            }
                        }
                        else
                        {
                            yield return payment;
                        }
                    }
                }
                else
                {
                    //do not return other payments with haved Void related payments
                    if (!allRelatedPayments.Any(x => GetPaymentTransactionType(x) == TransactionType.Void))
                    {
                        yield return payment;
                    }
                }
            }
        }

        private IEnumerable<Payment> GetPaymentsByTransactionType(IEnumerable<Payment> payments, TransactionType[] types)
        {
            return payments.Where(x => types.Any(type => GetPaymentTransactionType(x) == type));
        }

        /// <summary>
        /// Gets the type of the payment transaction.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <returns></returns>
        [Obsolete("Use IPayment.GetPaymentTransactionType() - extension method of IPayment instead. Will remain at least until November 2017.")]
        protected TransactionType GetPaymentTransactionType(Payment payment)
        {
            return payment.GetPaymentTransactionType();
        }


        /// <summary>
        /// Gets all related payments. On Order.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <returns></returns>
        [Obsolete("Use IPayment.GetAllRelatedPayments() - extension method of IPayment instead. Will remain at least until November 2017.")]
        protected IEnumerable<Payment> GetAllRelatedPayments(Payment payment)
        {
            return payment.GetAllRelatedPayments() as IEnumerable<Payment>;
        }

        /// <summary>
        /// Adjust item stock.
        /// </summary>
        /// <param name="lineItem">The line item.</param>
        /// <returns></returns>
        protected void AdjustStockItemQuantity(LineItem lineItem)
        {
            AdjustStockItemQuantity(null, lineItem);
        }

        /// <summary>
        /// Adjust item stock.
        /// </summary>
        /// <param name="shipment">The shipment.</param>
        /// <param name="lineItem">The line item.</param>
        /// <returns></returns>
        protected InventoryRequest AdjustStockItemQuantity(Shipment shipment, LineItem lineItem)
        {
            var entryDto = CatalogContext.Current.GetCatalogEntryDto(lineItem.Code,
                                                                    new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Variations));
            if (entryDto == null)
            {
                return null;
            }

            var catalogEntry = entryDto.CatalogEntry.FirstOrDefault();
            if (catalogEntry != null && InventoryTrackingEnabled(catalogEntry))
            {
                decimal delta = GetLineItemAdjustedQuantity(shipment, lineItem);

                string warehouseCode = shipment != null && !string.IsNullOrEmpty(shipment.WarehouseCode) ? shipment.WarehouseCode : lineItem.WarehouseCode;
                var inventoryRecord = InventoryService.Get(catalogEntry.Code, warehouseCode);
                if (inventoryRecord == null)
                {
                    return null;
                }

                var requestItems = GetInventoryRequestItems(lineItem, inventoryRecord, delta, warehouseCode);
                if (requestItems.Any())
                {
                    return new InventoryRequest(DateTime.UtcNow, requestItems, null);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets list of InventoryRequestItem for adjusting inventory of specific line item .
        /// </summary>
        /// <param name="lineItem">Line item object in cart.</param>
        /// <param name="inventoryRecord">Inventory record associated with the line item's catalog entry.</param>
        /// <param name="delta">The change in inventory.</param>
        /// <param name="warehouseCode">The warehouse code.</param>
        /// <returns></returns>
        private IList<InventoryRequestItem> GetInventoryRequestItems(LineItem lineItem, InventoryRecord inventoryRecord, decimal delta, string warehouseCode)
        {
            var requestItems = new List<InventoryRequestItem>();

            var entryCode = lineItem.Code;

            //arrival
            if (delta > 0)
            {
                // TODO: that is impossible to request a negative quantity with new inventory API, so need to find another way in this case.
                // need distribute delta between InStock, Backorder, Preorder.
                if (lineItem.InStockQuantity > 0)
                {
                    var backorderDelta = Math.Min(delta, lineItem.BackorderQuantity - inventoryRecord.BackorderAvailableQuantity - inventoryRecord.BackorderRequestedQuantity);
                    var preorderdelta = Math.Min(delta, lineItem.PreorderQuantity - inventoryRecord.PreorderAvailableQuantity - inventoryRecord.PreorderRequestedQuantity);

                    // In this case, need to add more backorder or preorder quantity
                    requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Backorder, entryCode, warehouseCode, -backorderDelta));
                    requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Preorder, entryCode, warehouseCode, -preorderdelta));
                    requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Purchase, entryCode, warehouseCode, -(delta - backorderDelta - preorderdelta)));

                } //need distribute delta between Preorder and Backorder
                else if (lineItem.InStockQuantity == 0)
                {
                    if (lineItem.PreorderQuantity > 0)
                    {
                        requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Preorder, entryCode, warehouseCode, delta));
                    }
                    else if (lineItem.BackorderQuantity > 0)
                    {
                        requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Backorder, entryCode, warehouseCode, delta));
                    }
                }
            }//consumption
            else
            {
                delta = Math.Abs(delta);
                var requestDate = FrameworkContext.Current.CurrentDateTime;
                var allowPreorder = inventoryRecord.PreorderAvailableUtc > SafeBeginningOfTime && requestDate >= inventoryRecord.PreorderAvailableUtc && delta <= inventoryRecord.PreorderAvailableQuantity;
                var allowBackOrder = inventoryRecord.BackorderAvailableQuantity > 0 && inventoryRecord.BackorderAvailableUtc <= requestDate;

                if (requestDate >= inventoryRecord.PurchaseAvailableUtc)
                {
                    // In case inventory status is Disable or enough purchase quantity, always do Purchase request.
                    if (!inventoryRecord.IsTracked || inventoryRecord.PurchaseAvailableQuantity >= delta)
                    {
                        requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Purchase, entryCode, warehouseCode, delta));
                    }
                    else
                    {
                        if (inventoryRecord.PurchaseAvailableQuantity > 0)
                        {
                            var backOrderDelta = delta - inventoryRecord.PurchaseAvailableQuantity;
                            if (allowBackOrder && backOrderDelta <= inventoryRecord.BackorderAvailableQuantity)
                            {
                                // purchase remain items and backorder other
                                requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Purchase, entryCode, warehouseCode, inventoryRecord.PurchaseAvailableQuantity));
                                requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Backorder, entryCode, warehouseCode, backOrderDelta));
                            }
                        }
                        else if (allowBackOrder && delta <= inventoryRecord.BackorderAvailableQuantity)
                        {
                            // Backorder request
                            requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Backorder, entryCode, warehouseCode, delta));
                        }
                    }
                }
                else if (allowPreorder)
                {
                    // Preorder request
                    requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Preorder, entryCode, warehouseCode, delta));
                }
            }
            return requestItems;
        }

        private InventoryRequestItem CreateRequestItem(int index, InventoryRequestType requestType, string entryCode, string warehouseCode, decimal delta)
        {
            //We currently do not use operation key and context
            return new InventoryRequestItem(index, requestType, entryCode, warehouseCode, delta, string.Empty, null);
        }

        private decimal GetLineItemAdjustedQuantity(Shipment shipment, LineItem lineItem)
        {
            return -(shipment != null ? Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId) : lineItem.Quantity);
        }

        /// <summary>
        /// Get entry row from a line item
        /// </summary>
        /// <param name="lineItem">line item</param>
        protected static CatalogEntryDto.CatalogEntryRow GetEntryRowForLineItem(LineItem lineItem)
        {
            var responseGroup = new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Variations);

            CatalogEntryDto entryDto = CatalogContext.Current.GetCatalogEntryDto(lineItem.Code, responseGroup);
            return entryDto.CatalogEntry.FirstOrDefault();
        }

        /// <summary>
        /// Check catalog entry's tracking inventory was enable or not.
        /// </summary>
        /// <param name="catalogEntry">Catalog entry.</param>
        private bool InventoryTrackingEnabled(CatalogEntryDto.CatalogEntryRow catalogEntry)
        {
            if (catalogEntry == null)
            {
                return false;
            }

            var entryVariations = catalogEntry.GetVariationRows();
            var variation = entryVariations.FirstOrDefault();
            return variation != null && variation.TrackInventory;
        }

        private InventoryRecord InventoryToRecord(IWarehouseInventory warehouseInventory)
        {
            var ir = new InventoryRecord();
            ir.CatalogEntryCode = warehouseInventory.CatalogKey.CatalogEntryCode;
            ir.WarehouseCode = warehouseInventory.WarehouseCode;
            ir.IsTracked = warehouseInventory.InventoryStatus == InventoryTrackingStatus.Enabled;
            ir.PurchaseAvailableQuantity = warehouseInventory.InStockQuantity - warehouseInventory.ReservedQuantity;
            ir.PreorderAvailableQuantity = Math.Max(warehouseInventory.PreorderQuantity, 0);
            ir.BackorderAvailableQuantity = Math.Max(warehouseInventory.AllowBackorder ? warehouseInventory.BackorderQuantity : 0, 0);
            ir.PreorderAvailableUtc = warehouseInventory.AllowPreorder ? (warehouseInventory.PreorderAvailabilityDate ?? SafeBeginningOfTime) : SafeBeginningOfTime;
            ir.BackorderAvailableUtc = warehouseInventory.BackorderAvailabilityDate ?? SafeBeginningOfTime;
            ir.AdditionalQuantity = warehouseInventory.ReservedQuantity;
            ir.ReorderMinQuantity = warehouseInventory.ReorderMinQuantity;

            return ir;
        }

        /// <summary>
        /// Reorder line item indexes in all shipments after delete an item
        /// </summary>
        /// <param name="orderForm">order form</param>
        /// <param name="lineItem">removed line item</param>
        protected void ReorderIndexes(OrderForm orderForm, LineItem lineItem)
        {
            int lineItemIndex = orderForm.LineItems.IndexOf(lineItem);
            foreach (var ship in orderForm.Shipments.ToArray())
            {
                IEnumerable<int> listIdx = ship.LineItemIndexes.Select(c => Convert.ToInt32(c)).Where(i => i > lineItemIndex);
                foreach (int idx in listIdx)
                {
                    ship.RemoveLineItemIndex(idx);
                    ship.AddLineItemIndex(idx - 1);
                }
            }
        }

        /// <summary>
        /// Calculate new line item quantity from inventory/in-store inventory
        /// </summary>
        /// <param name="lineItem">line item</param>
        /// <param name="changeQtyReason">messages explain to clients why item's quantity is changed</param>
        /// <param name="shipment">shipment</param>
        /// <returns>new line item quantity</returns>
        protected decimal GetNewLineItemQty(LineItem lineItem, List<string> changeQtyReason, Shipment shipment)
        {
            var newLineItemQty = shipment != null ? Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId) : lineItem.Quantity;

            if (newLineItemQty < lineItem.MinQuantity)
            {
                newLineItemQty = lineItem.MinQuantity;
                changeQtyReason.Add("by Min Quantity setting");
            }
            else if (newLineItemQty > lineItem.MaxQuantity)
            {
                newLineItemQty = lineItem.MaxQuantity;
                changeQtyReason.Add("by Max Quantity setting");
            }

            var entryRow = GetEntryRowForLineItem(lineItem);
            if (!InventoryTrackingEnabled(entryRow))
            {
                return newLineItemQty;
            }

            var warehouseCode = shipment != null && !string.IsNullOrEmpty(shipment.WarehouseCode) ? shipment.WarehouseCode : lineItem.WarehouseCode;
            if (shipment == null && string.IsNullOrEmpty(warehouseCode))
            {
                var w = WarehouseRepository.List().FirstOrDefault(x => x.IsFulfillmentCenter && x.IsActive);
                warehouseCode = w.Code;
            }

            IWarehouse warehouse = WarehouseRepository.Get(warehouseCode);
            if (warehouse == null || !warehouse.IsActive)
            {
                changeQtyReason.Add("by inactive warehouse");
                return 0;
            }

            var inventoryRecord = InventoryService.Get(lineItem.Code, warehouseCode);
            if (inventoryRecord == null)
            {
                changeQtyReason.Add("by inactive warehouse");
                return 0;
            }

            // In case inventory status is Disable, always using newLineItemQty.
            if (!inventoryRecord.IsTracked)
            {
                return newLineItemQty;
            }

            return GetNewLineItemQtyFromInventory(inventoryRecord, changeQtyReason, lineItem, newLineItemQty);
        }

        private decimal GetNewLineItemQtyFromInventory(InventoryRecord inventoryRecord, List<string> changeQtyReason, LineItem lineItem, decimal lineItemQty)
        {
            decimal lineItemRequestedQty = GetLineItemRequestedQty(lineItem);
            decimal changedQuantity = lineItemQty - lineItemRequestedQty;

            var requestDate = FrameworkContext.Current.CurrentDateTime;
            var allowPreorder = inventoryRecord.PreorderAvailableUtc > SafeBeginningOfTime && requestDate >= inventoryRecord.PreorderAvailableUtc;
            var allowBackOrder = inventoryRecord.BackorderAvailableQuantity > 0 && inventoryRecord.BackorderAvailableUtc <= requestDate;

            if (requestDate >= inventoryRecord.PurchaseAvailableUtc)
            {
                var availableQuantity = Math.Max(0, inventoryRecord.PurchaseAvailableQuantity);

                if (changedQuantity > availableQuantity)
                {
                    if (allowBackOrder)
                    {
                        availableQuantity += inventoryRecord.BackorderAvailableQuantity;

                        if (changedQuantity > availableQuantity)
                        {
                            changedQuantity = availableQuantity;
                            changeQtyReason.Add("by BackOrder quantity");
                        }
                    }
                    else
                    {
                        changedQuantity = availableQuantity;
                        changeQtyReason.Add("by current available quantity");
                    }
                }
            }
            else if (allowPreorder)
            {
                var preOrderAvailableQty = Math.Max(0, inventoryRecord.PreorderAvailableQuantity);

                if (changedQuantity > preOrderAvailableQty)
                {
                    changedQuantity = preOrderAvailableQty;
                    changeQtyReason.Add("by Preorder quantity");
                }
            }
            else
            {
                changeQtyReason.Add("by Entry is not available");
                return 0;
            }

            return lineItemRequestedQty + changedQuantity;
        }

        /// <summary>
        /// Gets the requested quantity change of line item in shipments
        /// </summary>
        /// <param name="lineItem">The line item.</param>        
        /// <returns>The requested quantity change.</returns>
        private decimal GetLineItemRequestedQty(LineItem lineItem)
        {
            decimal lineItemRequestedQty = 0;
            OperationKeySerializer operationKeySerializer = new OperationKeySerializer();
            var operationKeys = new List<string>();
            var orderForm = lineItem.Parent;

            foreach (Shipment sh in orderForm.Shipments)
            {
                operationKeys.AddRange(sh.GetInventoryOperationKey(orderForm.LineItems.IndexOf(lineItem)));
            }

            InventoryRequestType requestType = default(InventoryRequestType);
            InventoryChange inventoryChange = null;

            foreach (string key in operationKeys)
            {
                operationKeySerializer.TryDeserialize(key, out requestType, out inventoryChange);

                switch (requestType)
                {
                    case InventoryRequestType.Purchase:
                        lineItemRequestedQty += inventoryChange.PurchaseRequestedChange;
                        break;
                    case InventoryRequestType.Backorder:
                        lineItemRequestedQty += inventoryChange.BackorderRequestedChange;
                        break;
                    case InventoryRequestType.Preorder:
                        lineItemRequestedQty += inventoryChange.PreorderRequestedChange;
                        break;
                }
            }

            return lineItemRequestedQty;
        }

        protected bool CancelOperationKeys(Shipment shipment)
        {
            var itemIndexStart = 0;
            var requestItems = shipment.OperationKeysMap.SelectMany(c => c.Value).Select(key =>
                    new InventoryRequestItem()
                    {
                        ItemIndex = itemIndexStart++,
                        OperationKey = key,
                        RequestType = InventoryRequestType.Cancel
                    }).ToList();

            if (requestItems.Any())
            {
                InventoryService.Request(new InventoryRequest(DateTime.UtcNow, requestItems, null));
                shipment.ClearOperationKeys();
            }
            return true;
        }

        protected InventoryTrackingStatus GetInventoryStatus(string entryCode, string warehouseCode)
        {
            var inventoryRecord = InventoryService.Get(entryCode, warehouseCode);
            if (inventoryRecord == null)
            {
                return InventoryTrackingStatus.Disabled;
            }

            return inventoryRecord.IsTracked ? InventoryTrackingStatus.Enabled : InventoryTrackingStatus.Disabled;
        }
    }
}
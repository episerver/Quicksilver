using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    public abstract class PurchaseValidation
    {
        protected readonly LocalizationService LocalizationService;

        public abstract bool ValidateModel(ModelStateDictionary modelState, CheckoutViewModel viewModel);

        protected PurchaseValidation(LocalizationService localizationService)
        {
            LocalizationService = localizationService;
        }
        
        public virtual bool ValidateOrderOperation(ModelStateDictionary modelState, IDictionary<ILineItem, IList<ValidationIssue>> validationIssues)
        {
            foreach (var validationMessage in GetValidationMessages(validationIssues))
            {
                modelState.AddModelError("", validationMessage);
            }

            return modelState.IsValid;
        }

        protected bool ValidateShippingMethods(ModelStateDictionary modelState, CheckoutViewModel checkoutViewModel)
        {
            if (checkoutViewModel.Shipments.Any(s => s.ShippingMethodId == Guid.Empty))
            {
                modelState.AddModelError("Shipment.ShippingMethod", LocalizationService.GetString("/Shared/Address/Form/Empty/ShippingMethod"));
            }

            return modelState.IsValid;
        }

        protected IEnumerable<string> GetValidationMessages(IDictionary<ILineItem, IList<ValidationIssue>> validationIssues)
        {
            foreach (var validationIssue in validationIssues)
            {
                foreach (var issue in validationIssue.Value)
                {
                    switch (issue)
                    {
                        case ValidationIssue.None:
                        case ValidationIssue.RemovedGiftDueToInsufficientQuantityInInventory:
                            break;
                        case ValidationIssue.CannotProcessDueToMissingOrderStatus:
                            yield return string.Format(LocalizationService.GetString("/Checkout/Payment/Errors/CannotProcessDueToMissingOrderStatus"),
                                validationIssue.Key.Code);
                            break;
                        case ValidationIssue.RemovedDueToCodeMissing:
                        case ValidationIssue.RemovedDueToNotAvailableInMarket:
                        case ValidationIssue.RemovedDueToInactiveWarehouse:
                        case ValidationIssue.RemovedDueToMissingInventoryInformation:
                        case ValidationIssue.RemovedDueToUnavailableCatalog:
                        case ValidationIssue.RemovedDueToUnavailableItem:
                            yield return string.Format(LocalizationService.GetString("/Checkout/Payment/Errors/RemovedDueToUnavailableItem"),
                                validationIssue.Key.Code);
                            break;
                        case ValidationIssue.RemovedDueToInsufficientQuantityInInventory:
                            yield return string.Format(LocalizationService.GetString("/Checkout/Payment/Errors/RemovedDueToInsufficientQuantityInInventory"),
                                validationIssue.Key.Code);
                            break;
                        case ValidationIssue.RemovedDueToInvalidPrice:
                            yield return string.Format(LocalizationService.GetString("/Checkout/Payment/Errors/RemovedDueToInvalidPrice"),
                                validationIssue.Key.Code);
                            break;
                        case ValidationIssue.AdjustedQuantityByMinQuantity:
                        case ValidationIssue.AdjustedQuantityByMaxQuantity:
                        case ValidationIssue.AdjustedQuantityByBackorderQuantity:
                        case ValidationIssue.AdjustedQuantityByPreorderQuantity:
                        case ValidationIssue.AdjustedQuantityByAvailableQuantity:
                            yield return string.Format(LocalizationService.GetString("/Checkout/Payment/Errors/AdjustedQuantity"),
                                validationIssue.Key.Code);
                            break;
                        case ValidationIssue.PlacedPricedChanged:
                            yield return string.Format(LocalizationService.GetString("/Checkout/Payment/Errors/PlacedPricedChanged"),
                                validationIssue.Key.Code);
                            break;
                        default:
                            yield return string.Format(LocalizationService.GetString("/Checkout/Payment/Errors/PreProcessingFailure"),
                                validationIssue.Key.Code);
                            break;
                    }
                }
            }
        }
    }
}

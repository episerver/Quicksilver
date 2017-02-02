using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.InventoryService;
using System;

namespace EPiServer.Reference.Customization
{
    public class CustomizedLineItemValidator : DefaultLineItemValidator
    {
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;

        public CustomizedLineItemValidator(
            IContentLoader contentLoader,
            ReferenceConverter referenceConverter,
            ICatalogSystem catalogSystem)
            : base(contentLoader, referenceConverter, catalogSystem)
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
        }

        public override bool Validate(ILineItem lineItem, IMarket market, Action<ILineItem, ValidationIssue> onValidationError)
        {            
            // Our customize validation here
            var item = lineItem.GetEntryContent(_referenceConverter, _contentLoader);

            // Filter by name here - this is just sample for "we don't sale this items."
            if (item.Name.Contains("Vintage"))
            {
                onValidationError(lineItem, ValidationIssue.RemovedDueToUnavailableItem);
                return false;
            }

            // Filter by code here - this is just sample for "we don't sale this kind of items anymore."
            if (item.Code.Contains("shirt"))
            {
                onValidationError(lineItem, ValidationIssue.RemovedDueToUnavailableItem);
                return false;
            }

            // Filter by type here - this is just sample for "we don't sale this kind of items anymore."
            if (item is SportsWearVariant)
            {
                onValidationError(lineItem, ValidationIssue.RemovedDueToUnavailableItem);
                return false;
            }

            // Filter by color here - this is just sample for "we don't sale this color anymore."
            var fashionItem = item as FashionVariant;
            if (fashionItem.Color == "Vintage")
            {
                onValidationError(lineItem, ValidationIssue.RemovedDueToUnavailableItem);
                return false;
            }
            
                        
            return base.Validate(lineItem, market, onValidationError);;
        }
    }
}

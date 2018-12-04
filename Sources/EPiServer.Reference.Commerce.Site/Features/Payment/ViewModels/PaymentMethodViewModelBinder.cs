using EPiServer.Commerce.Order;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class PaymentMethodViewModelBinder : DefaultModelBinder
    {
        private readonly IEnumerable<IPaymentMethod> _paymentMethods;

        public PaymentMethodViewModelBinder(IEnumerable<IPaymentMethod> paymentMethods)
        {
            _paymentMethods = paymentMethods;
        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var systemKeyword = bindingContext.ValueProvider.GetValue("SystemKeyword")?.AttemptedValue;
            var selectedPaymentMethod = _paymentMethods.FirstOrDefault(p => !string.IsNullOrEmpty(p.SystemKeyword) && p.SystemKeyword.ToString() == systemKeyword);
            if (selectedPaymentMethod == null)
            {
                return null;
            }
            bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, selectedPaymentMethod.GetType());
            return base.BindModel(controllerContext, bindingContext);
        }
    }
}
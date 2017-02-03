using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class PaymentViewModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            string controllerName = bindingContext.ValueProvider.GetValue("SystemName").AttemptedValue;
            bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, PaymentMethodViewModelResolver.Resolve(controllerName).GetType());
            object model = base.BindModel(controllerContext, bindingContext);

            return model;
        }
    }
}

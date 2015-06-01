using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public class PaymentViewModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            string controllerName = controllerContext.HttpContext.Request.Form.Get("PaymentMethod.Name");

            switch (controllerName)
            {
                case "GenericCreditCardPaymentMethod":
                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(GenericCreditCardPaymentMethodViewModel));
                    break;
            }

            object model = base.BindModel(controllerContext, bindingContext);

            return model;
        }
    }
}
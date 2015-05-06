using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Controllers
{
    public abstract class BasePaymentController<T> : Controller
    {
        private readonly Lazy<T> _paymentModel;

        protected BasePaymentController()
        {
            _paymentModel = new Lazy<T>(GetPaymentModel);
        }

        public T PaymentModel
        {
            get { return _paymentModel.Value; }
        }

        public abstract ActionResult Process();

        private T GetPaymentModel()
        {
            var mbc = new ModelBindingContext
            {
                ModelName = "FormModel",
                ModelState = ViewData.ModelState,
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(T)),
                ValueProvider = new FormCollection(Request.Form).ToValueProvider()
            };
            IModelBinder binder = new DefaultModelBinder();
            var cc = new ControllerContext();

            return (T)binder.BindModel(cc, mbc);
        }
    }
}
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using Mediachase.Commerce.Website;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    public class ModelBinderProvider : IModelBinderProvider
    {
        private static readonly IDictionary<Type, Type> ModelBinderTypeMappings = new Dictionary<Type, Type>
        {                                                                                
            { typeof(FilterOptionFormModel), typeof(FilterOptionFormModelBinder) },
            { typeof(IPaymentMethodViewModel<IPaymentOption>), typeof(PaymentViewModelBinder) }                                                             
        };

        public IModelBinder GetBinder(Type modelType)
        {
            if (ModelBinderTypeMappings.ContainsKey(modelType))
            {
                return DependencyResolver.Current.GetService(ModelBinderTypeMappings[modelType]) as IModelBinder;
            }
            return null;
        }
    }
}
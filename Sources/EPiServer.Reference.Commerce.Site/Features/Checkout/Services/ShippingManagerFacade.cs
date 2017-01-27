using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class ShippingManagerFacade
    {
        public virtual IList<ShippingMethodInfoModel> GetShippingMethodsByMarket(string marketid, bool returnInactive)
        {
            var methods = ShippingManager.GetShippingMethodsByMarket(marketid, returnInactive);
            return methods.ShippingMethod.Select(method => new ShippingMethodInfoModel
            {
                MethodId = method.ShippingMethodId,
                Currency = method.Currency,
                LanguageId = method.LanguageId,
                Ordering = method.Ordering,
                ClassName = methods.ShippingOption.FindByShippingOptionId(method.ShippingOptionId).ClassName
            }).ToList();
        }

        public virtual ShippingRate GetRate(IShipment shipment, ShippingMethodInfoModel shippingMethodInfoModel, IMarket currentMarket)
        {
            var type = Type.GetType(shippingMethodInfoModel.ClassName);
            if (type == null)
            {
                throw new TypeInitializationException(shippingMethodInfoModel.ClassName, null);
            }

            string message = null;

            var shippingInstance = Activator.CreateInstance(type, currentMarket);
            var shippingPlugin = shippingInstance as IShippingPlugin;
            if (shippingPlugin != null)
            {
                return shippingPlugin.GetRate(shippingMethodInfoModel.MethodId, shipment, ref message);
            }
            return ((IShippingGateway)shippingInstance).GetRate(shippingMethodInfoModel.MethodId, (Shipment)shipment, ref message);
        }
    }
}
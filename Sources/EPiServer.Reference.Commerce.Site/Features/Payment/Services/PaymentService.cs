using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Services
{
    [ServiceConfiguration(typeof(IPaymentService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PaymentService : IPaymentService
    {
        public IEnumerable<PaymentMethodModel> GetPaymentMethodsByMarketIdAndLanguageCode(string marketId, string languageCode)
        {
            var methods = PaymentManager.GetPaymentMethodsByMarket(marketId)
                .PaymentMethod
                .Where(x => x.IsActive && languageCode.Equals(x.LanguageId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Ordering)
                .Select(x => new PaymentMethodModel
                {
                    PaymentMethodId = x.PaymentMethodId,
                    SystemName = x.SystemKeyword,
                    FriendlyName = x.Name,
                    Description = x.Description,
                    LanguageId = x.LanguageId 
                });

            return methods;
        }
    }
}
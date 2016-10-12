using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Services
{
    public interface IPaymentService
    {
        IEnumerable<PaymentMethodModel> GetPaymentMethodsByMarketIdAndLanguageCode(string marketId, string languageCode);
    }
}
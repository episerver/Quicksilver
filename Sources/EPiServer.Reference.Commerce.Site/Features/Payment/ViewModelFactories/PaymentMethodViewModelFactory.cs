using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModelFactories
{
    [ServiceConfiguration(typeof(PaymentMethodViewModelFactory), Lifecycle = ServiceInstanceScope.Hybrid)]
    public class PaymentMethodViewModelFactory
    {
        private readonly ICurrentMarket _currentMarket;
        private readonly LanguageService _languageService;
        private readonly IEnumerable<IPaymentMethod> _paymentMethods;
        private readonly IPaymentManagerFacade _paymentManager;

        public PaymentMethodViewModelFactory(
            ICurrentMarket currentMarket,
            LanguageService languageService,
            IEnumerable<IPaymentMethod> paymentMethods, 
            IPaymentManagerFacade paymentManager)
        {
            _currentMarket = currentMarket;
            _languageService = languageService;
            _paymentMethods = paymentMethods;
            _paymentManager = paymentManager;
        }

        public PaymentMethodSelectionViewModel CreatePaymentMethodSelectionViewModel(Guid paymentMethodId)
        {
            var viewModel = CreatePaymentMethodSelectionViewModel();
            viewModel.SelectedPaymentMethod = viewModel.PaymentMethods.Single(x => x.PaymentMethod.PaymentMethodId == paymentMethodId);

            return viewModel;
        }

        public PaymentMethodSelectionViewModel CreatePaymentMethodSelectionViewModel(IPaymentMethod paymentMethod)
        {
            var viewModel = CreatePaymentMethodSelectionViewModel();
            if (paymentMethod != null)
            {
                viewModel.SelectedPaymentMethod = viewModel.PaymentMethods.Single(x => x.PaymentMethod.PaymentMethodId == paymentMethod.PaymentMethodId);
                viewModel.SelectedPaymentMethod.PaymentMethod = paymentMethod;
            }
            return viewModel;
        }

        private PaymentMethodSelectionViewModel CreatePaymentMethodSelectionViewModel()
        {
            var currentMarket = _currentMarket.GetCurrentMarket().MarketId;
            var currentLanguage = _languageService.GetCurrentLanguage().TwoLetterISOLanguageName;
            var availablePaymentMethods = GetPaymentMethodsByMarketIdAndLanguageCode(currentMarket.Value, currentLanguage);
            var viewModel = new PaymentMethodSelectionViewModel
            {
                PaymentMethods = availablePaymentMethods
            };

            viewModel.SelectedPaymentMethod = viewModel.PaymentMethods.FirstOrDefault(x => x.IsDefault) 
                ?? viewModel.PaymentMethods.FirstOrDefault();

            return viewModel;
        }

        private IEnumerable<PaymentMethodViewModel<IPaymentMethod>> GetPaymentMethodsByMarketIdAndLanguageCode(string marketId, string languageCode)
        {
            var methods = _paymentManager.GetPaymentMethodsByMarket(marketId)
                .PaymentMethod
                .Where(x => x.IsActive && languageCode.Equals(x.LanguageId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Ordering)
                .Select(x => new PaymentMethodViewModel<IPaymentMethod>
                {
                    PaymentMethod = _paymentMethods.SingleOrDefault(method => method.SystemKeyword == x.SystemKeyword),
                    IsDefault = x.IsDefault
                });

            return methods;
        }
    }
}
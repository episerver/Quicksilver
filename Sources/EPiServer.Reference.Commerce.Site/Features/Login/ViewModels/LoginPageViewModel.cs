using EPiServer.Reference.Commerce.Site.Features.Login.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public class LoginPageViewModel : PageViewModel<LoginRegistrationPage>
    {
        public LoginViewModel LoginViewModel { get; set; }
        public RegisterAccountViewModel RegisterAccountViewModel { get; set; }

        public LoginPageViewModel(LoginRegistrationPage currentPage, string returnUrl)
        {
            CurrentPage = currentPage;
            LoginViewModel = new LoginViewModel() { ReturnUrl = returnUrl };
            RegisterAccountViewModel = new RegisterAccountViewModel
            {
                Address = new AddressModel (),
                CurrentPage = currentPage
            };
        }
    }
}
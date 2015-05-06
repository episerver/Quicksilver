using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public class LoginPageViewModel<T> : ILoginPageViewModel<T> where T : PageData
    {
        public T CurrentPage { get; private set; }
        public InternalLoginViewModel LoginViewModel { get; set; }
        public RegisterAccountViewModel RegisterAccountViewModel { get; set; }

        /// <summary>
        /// Returns a new instance of a LoginViewModel.
        /// </summary>
        /// <param name="currentPage">The page acting as the DataContext for the ViewModel.</param>
        public LoginPageViewModel(T currentPage, string returnUrl)
        {
            CurrentPage = currentPage;
            LoginViewModel = new InternalLoginViewModel() { ReturnUrl = returnUrl };
            RegisterAccountViewModel = new RegisterAccountViewModel();
        }
    }
}
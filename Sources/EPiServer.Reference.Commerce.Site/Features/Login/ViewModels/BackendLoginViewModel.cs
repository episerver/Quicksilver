using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public class BackendLoginViewModel : LoginViewModelBase
    {

        [LocalizedDisplay("/Login/Form/Label/Username")]
        [LocalizedRequired("/Login/Form/Empty/Username")]
        public string Username { get; set; }

        public string Heading { get; set; }

        public string LoginMessage { get; set; }
        
    }
}
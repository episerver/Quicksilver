using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public class SendCodeViewModel
    {

        public string SelectedProvider { get; set; }

        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }

        /// <summary>
        /// The URL used for re-directing the user to the previous view.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Gets or sets whether the user wants to be remembered when re-visiting the site.
        /// </summary>
        public bool RememberMe { get; set; }

    }
}
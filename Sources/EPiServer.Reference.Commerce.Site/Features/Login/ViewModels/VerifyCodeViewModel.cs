using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public class VerifyCodeViewModel
    {

        /// <summary>
        /// Gets or sets the name of the login provider used for authenticating the user.
        /// </summary>
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// The URL used for re-directing the user to the previous view.
        /// </summary>
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        /// <summary>
        /// Gets or sets whether the user wants to be remembered when re-visiting the site.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
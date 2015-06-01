using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {

        /// <summary>
        /// Gets or sets the E-mail address.
        /// </summary>
        [LocalizedRequired("/Registration/Form/Empty/Address")]
        [LocalizedDisplay("/Registration/Form/Label/Address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the country name in the user's address.
        /// </summary>
        [LocalizedRequired("/Registration/Form/Empty/Country")]
        [LocalizedDisplay("/Registration/Form/Label/Country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the city of the user's address.
        /// </summary>
        [LocalizedRequired("/Registration/Form/Empty/City")]
        [LocalizedDisplay("/Registration/Form/Label/City")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the ZIP or postal code of the user's address.
        /// </summary>
        [LocalizedRequired("/Registration/Form/Empty/PostalCode")]
        [LocalizedDisplay("/Registration/Form/Label/PostalCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets whether the user wishes to subscribe to newsletters.
        /// </summary>
        public bool Newsletter { get; set; }

        /// <summary>
        /// The URL used for re-directing the user to the previous view.
        /// </summary>
        public string ReturnUrl { get; set; }

    }
}
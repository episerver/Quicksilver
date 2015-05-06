using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    /// <summary>
    /// View model used when registering new user accounts.
    /// </summary>
    public class RegisterAccountViewModel
    {

        /// <summary>
        /// Gets or sets the E-mail address.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/Email")]
        [LocalizedRequired("/Registration/Form/Empty/Email")]
        [LocalizedEmail("/Registration/Form/Error/InvalidEmail")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the first entered password.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/Password")]
        [LocalizedRequired("/Registration/Form/Empty/Password")]
        [LocalizedStringLength("/Registration/Form/Error/PasswordLength2", 5, 100)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the second entered password.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/Password2")]
        [LocalizedRequired("/Registration/Form/Empty/Password2")]
        [LocalizedCompare("Password", "/Registration/Form/Error/PasswordMatch")]
        [LocalizedStringLength("/Registration/Form/Error/PasswordLength2", 5, 100)]
        public string Password2 { get; set; }

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/FirstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/LastName")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/Address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the country name in the user's address.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/Country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the city of the user's address.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/City")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the ZIP or postal code of the user's address.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/ZipCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets whether the user whishes to subscribe to newsletters.
        /// </summary>
        public bool Newsletter { get; set; }

    }
}
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {

        [LocalizedRequired("/Registration/Form/Empty/Address")]
        [LocalizedDisplay("/Registration/Form/Label/Address")]
        public string Address { get; set; }

        [LocalizedRequired("/Registration/Form/Empty/Country")]
        [LocalizedDisplay("/Registration/Form/Label/Country")]
        public string Country { get; set; }

        [LocalizedRequired("/Registration/Form/Empty/City")]
        [LocalizedDisplay("/Registration/Form/Label/City")]
        public string City { get; set; }

        [LocalizedRequired("/Registration/Form/Empty/PostalCode")]
        [LocalizedDisplay("/Registration/Form/Label/PostalCode")]
        public string PostalCode { get; set; }

        public bool AcceptMarketingEmail { get; set; }

        public string ReturnUrl { get; set; }

    }
}
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Profile.Pages
{
    [ContentType(GUID = "760AEE27-E362-49EE-BFAC-4FE32F2C4123",
                  DisplayName = "Contact Page",
                  GroupName = "New Page type",
                  Order = 100,
                  Description = "The contact page.")]
    public class ContactPage : PageData
    {
        [Searchable(false)]
        [BackingType(typeof(PropertyString))]
        [Display(Name = "PageTitle",
                   Description = "PageTitle",
                   GroupName = SystemTabNames.Content,
                   Order = 1)]
        public virtual string PageTitle { get; set; }

        [Searchable(false)]
        [Display(Name = "BodyMarkup",
                   Description = "BodyMarkup",
                   GroupName = SystemTabNames.Content,
                   Order = 3)]
        public virtual XhtmlString BodyMarkup { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Location",
            Description = "Our location",
            GroupName = SystemTabNames.Content,
            Order = 6)]
        public virtual ContentArea Location { get; set; }
    }
}
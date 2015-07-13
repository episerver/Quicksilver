using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Campaign.Pages
{
    [ContentType(DisplayName = "Campaign page", GUID = "bfba39b8-3161-4d01-a543-f4b0e18e995b", Description = "A Page which is used to show campaign details.")]
    [ImageUrl("~/styles/images/page_type.png")]
    public class CampaignPage : PageData
    {
        [Display(Name = "Page Title", 
            GroupName = SystemTabNames.Content, 
            Order = 10)]
        [CultureSpecific]
        public virtual String PageTitle { get; set; }

        [Display(Name = "Main Content Area",
          Description = "This is the main content area",
          GroupName = SystemTabNames.Content,
          Order = 20)]
        public virtual ContentArea MainContentArea { get; set; }
    }
}
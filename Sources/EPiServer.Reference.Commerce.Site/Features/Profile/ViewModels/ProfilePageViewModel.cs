using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Reference.Commerce.Site.Features.Profile.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Profile.ViewModels
{
    public class ProfilePageViewModel : PageViewModel<ProfilePage>
    {
        public ConsentData ConsentData { get; set; }
    }
}
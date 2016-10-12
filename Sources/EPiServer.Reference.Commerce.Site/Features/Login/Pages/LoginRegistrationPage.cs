using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Pages
{
    [ContentType(
        DisplayName = "Login and Registration page", 
        GUID = "3c045289-8e14-420e-a815-62fdf13e4b16", 
        Description = "", 
        AvailableInEditMode = false)]
    [AvailableContentTypes(Include = new[] { typeof(LoginRegistrationPage)})]
    public class LoginRegistrationPage : PageData
    {
        [CultureSpecific]
        [Display(
            Name = "Main area",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual ContentArea MainArea { get; set; }
    }
}
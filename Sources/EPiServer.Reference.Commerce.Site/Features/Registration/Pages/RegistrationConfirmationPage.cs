using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Registration.Pages
{
    [ContentType(DisplayName = "Registration and confirmation page", GUID = "85b681d5-20d0-4cf1-9b3f-bbd624c3072e", Description = "", AvailableInEditMode = false)]
    [AvailableContentTypes(Availability = Availability.None)]
    public class RegistrationConfirmationPage : PageData
    {
    }
}
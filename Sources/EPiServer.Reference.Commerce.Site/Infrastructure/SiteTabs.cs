using EPiServer.DataAnnotations;
using EPiServer.Security;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    [GroupDefinitions]
    public static class SiteTabs
    {
        [Display(Order = 100)]
        [RequiredAccess(AccessLevel.Edit)]
        public const string SiteStructure = "Site structure";

        [Display(Order = 110)]
        [RequiredAccess(AccessLevel.Edit)]
        public const string MailTemplates = "Mail templates";
    }
}
using EPiServer.Reference.Commerce.Site.Features.Registration.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("EcfSqlConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
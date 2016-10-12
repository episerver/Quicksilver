using System;
using EPiServer.ServiceLocation;
using Mediachase.Data.Provider;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EPiServer.Reference.Commerce.Shared.Models.Identity
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private static readonly Lazy<IConnectionStringHandler> ConnectionHandler = new Lazy<IConnectionStringHandler>(() => ServiceLocator.Current.GetInstance<IConnectionStringHandler>());

        public ApplicationDbContext(IConnectionStringHandler connectionHandler)
            : base(connectionHandler.Commerce.Name, throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext(ConnectionHandler.Value);
        }
    }
}
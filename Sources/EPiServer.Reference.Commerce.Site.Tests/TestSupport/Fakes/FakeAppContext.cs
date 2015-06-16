using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using System;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    public class FakeAppContext : AppContextFacade
    {
        public override Guid ApplicationId
        {
            get { return Guid.Empty; }
        }
    }
}

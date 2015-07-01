using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    public class FakeUrlAuthorization : UrlAuthorizationFacade
    {
        public override bool CheckUrlAccessForPrincipal(string path)
        {
            return path.StartsWith("/episerver");
        }
    }
}

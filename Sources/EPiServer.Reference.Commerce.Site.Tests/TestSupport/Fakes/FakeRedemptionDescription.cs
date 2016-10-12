using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Commerce.Marketing;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    public class FakeRedemptionDescription : RedemptionDescription
    {
        public FakeRedemptionDescription(IEnumerable<IAffectedObject> affectedObjects) : base(affectedObjects)
        {
        }
    }
}

using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    class FakeCountryManager : CountryManagerFacade
    {
        public override CountryDto GetCountries()
        {
            return new CountryDto();
        }
    }
}
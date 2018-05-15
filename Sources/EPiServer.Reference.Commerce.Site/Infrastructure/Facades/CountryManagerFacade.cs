using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Facades
{
    [ServiceConfiguration(typeof(CountryManagerFacade), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CountryManagerFacade
    {
        public virtual CountryDto GetCountries()
        {
            return CountryManager.GetCountries();
        }

        public virtual CountryDto.CountryRow GetCountryByCountryCode(string countryCode)
        {
            var table = CountryManager.GetCountry(countryCode, false).Country;

            return table.Rows.Count == 1 ? table.Rows[0] as CountryDto.CountryRow : null;
        }
    }
}
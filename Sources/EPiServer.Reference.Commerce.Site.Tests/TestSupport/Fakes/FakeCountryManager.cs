using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Mediachase.Commerce.Orders.Dto;
using System;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    class FakeCountryManager : CountryManagerFacade
    {
        public override CountryDto GetCountries()
        {
            var countryDto = new CountryDto();

            countryDto.Country.AddCountryRow("United States", -1, true, "USA");
            countryDto.Country.AddCountryRow("Australia", 0, true, "AUS");
            countryDto.Country.AddCountryRow("Canada", 1, true, "CAN");

            return countryDto;
        }

        public override CountryDto.CountryRow GetCountryByCountryCode(string countryCode)
        {
            return GetCountries().Country.FirstOrDefault(x => x.Code == countryCode);
        }
    }
}
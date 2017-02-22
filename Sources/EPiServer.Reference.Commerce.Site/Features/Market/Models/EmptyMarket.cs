using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Models
{
    public class EmptyMarket : IMarket
    {
        public IEnumerable<string> Countries
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        public IEnumerable<Currency> Currencies
        {
            get
            {
                return Enumerable.Empty<Currency>();
            }
        }

        public Currency DefaultCurrency
        {
            get
            {
                return Currency.USD;
            }
        }

        public CultureInfo DefaultLanguage
        {
            get
            {
                return CultureInfo.CurrentUICulture;
            }
        }

        public bool IsEnabled
        {
            get
            {
                return true;
            }
        }

        public IEnumerable<CultureInfo> Languages
        {
            get
            {
                return Enumerable.Empty<CultureInfo>();
            }
        }

        public string MarketDescription
        {
            get
            {
                return String.Empty;
            }
        }

        public MarketId MarketId
        {
            get
            {
                return new MarketId("US");
            }
        }

        public string MarketName
        {
            get
            {
                return String.Empty;
            }
        }
    }
}
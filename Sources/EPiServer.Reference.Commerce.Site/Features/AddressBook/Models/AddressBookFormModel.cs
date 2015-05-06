using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.Models
{
    public class AddressBookFormModel
    {
        public AddressBookPage CurrentPage { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/AddressName")]
        [LocalizedRequired("/AddressBook/Form/Empty/Name")]
        public string Name { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/FirstName")]
        [LocalizedRequired("/AddressBook/Form/Empty/FirstName")]
        public string FirstName { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/LastName")]
        [LocalizedRequired("/AddressBook/Form/Empty/LastName")]
        public string LastName { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/Organization")]
        public string Organization { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/Line1")]
        [LocalizedRequired("/AddressBook/Form/Empty/Line1")]
        public string Line1 { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/Line2")]
        public string Line2 { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/City")]
        [LocalizedRequired("/AddressBook/Form/Empty/City")]
        public string City { get; set; }

        public IEnumerable<CountryDto.StateProvinceRow> RegionOptions { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/Region")]
        public string Region { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/PostalCode")]
        [LocalizedRequired("/AddressBook/Form/Empty/PostalCode")]
        public string PostalCode { get; set; }

        public IEnumerable<CountryDto.CountryRow> CountryOptions { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/CountryName")]
        [LocalizedRequired("/AddressBook/Form/Empty/CountryName")]
        public string CountryName { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/DaytimePhoneNumber")]
        public string DaytimePhoneNumber { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/Email")]
        public string Email { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/ShippingAddress")]
        public bool ShippingDefault { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/BillingAddress")]
        public bool BillingDefault { get; set; }

        public Guid? AddressId { get; set; }

        public DateTime? Modified { get; set; }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Name))
            {
                return String.Empty;
            }
            var strAddress = new StringBuilder();
            if (!String.IsNullOrEmpty(Organization))
            {
                strAddress.AppendFormat("{0}<br />", Organization);
            }
            strAddress.AppendFormat("{0} {1} <br />", FirstName, LastName);
            if (!String.IsNullOrEmpty(Email))
            {
                strAddress.AppendFormat("{0}<br />", Email);
            }
            strAddress.AppendFormat("{0}<br />", Line1);
            if (!String.IsNullOrEmpty(Line2))
            {
                strAddress.AppendFormat("{0}<br />", Line2);
            }
            if (!String.IsNullOrEmpty(City))
            {
                strAddress.AppendFormat("{0}", City);
            }
            if (!String.IsNullOrEmpty(Region))
            {
                strAddress.AppendFormat(", {0}", Region);
            }

            if (!String.IsNullOrEmpty(PostalCode))
            {
                strAddress.AppendFormat(", {0}", PostalCode);
            }

            if (!String.IsNullOrEmpty(CountryName))
            {
                strAddress.AppendFormat("<br />{0}", CountryName);
            }

            if (!String.IsNullOrEmpty(DaytimePhoneNumber))
            {
                strAddress.AppendFormat("<br />{0}", DaytimePhoneNumber);
            }
            return strAddress.ToString();
        }
    }
}
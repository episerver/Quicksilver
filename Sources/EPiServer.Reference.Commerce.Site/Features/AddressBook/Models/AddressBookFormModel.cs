using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.Models
{
    public class AddressBookFormModel : AddressModelBase
    {
        public AddressBookPage CurrentPage { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/AddressName")]
        [LocalizedRequired("/AddressBook/Form/Empty/Name")]
        public string Name { get; set; }
        
        [LocalizedDisplay("/AddressBook/Form/Label/Organization")]
        public string Organization { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/Line2")]
        public string Line2 { get; set; }
        
       

        

        [LocalizedDisplay("/AddressBook/Form/Label/DaytimePhoneNumber")]
        public string DaytimePhoneNumber { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/Email")]
        [LocalizedEmail("/AddressBook/Form/Error/InvalidEmail")]
        public string Email { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/ShippingAddress")]
        public bool ShippingDefault { get; set; }

        [LocalizedDisplay("/AddressBook/Form/Label/BillingAddress")]
        public bool BillingDefault { get; set; }

        public Guid? AddressId { get; set; }

        public DateTime? Modified { get; set; }
    }
}
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Customers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Shared.Identity
{
    public class SiteUser : ApplicationUser
    {
        public SiteUser()
        {

        }

        /// <summary>
        /// Returns a new instance of an ApplicationUser based on a previously made purchase order.
        /// </summary>
        /// <param name="purchaseOrder"></param>
        public SiteUser(IPurchaseOrder purchaseOrder)
        {
            Addresses = new List<CustomerAddress>();

            var billingAddress = purchaseOrder.GetFirstForm().Payments.First().BillingAddress;

            if (billingAddress != null)
            {
                Email = billingAddress.Email;
                UserName = billingAddress.Email;
                FirstName = billingAddress.FirstName;
                LastName = billingAddress.LastName;

                var addressesToAdd = new HashSet<IOrderAddress>(purchaseOrder.GetFirstForm().Shipments.Select(x => x.ShippingAddress));

                foreach (var shippingAddress in addressesToAdd)
                {
                    if (shippingAddress.Id != billingAddress.Id)
                    {
                        Addresses.Add(CreateCustomerAddress(shippingAddress, CustomerAddressTypeEnum.Shipping));
                    }
                }

                Addresses.Add(CreateCustomerAddress(billingAddress, CustomerAddressTypeEnum.Billing));
            }
        }

        [NotMapped]
        public List<CustomerAddress> Addresses { get; set; }

        [NotMapped]
        public string FirstName { get; set; }

        [NotMapped]
        public string LastName { get; set; }

        [NotMapped]
        public string RegistrationSource { get; set; }

        [NotMapped]
        public string Password { get; set; }

        [NotMapped]
        public bool AcceptMarketingEmail { get; set; }

        public bool NewsLetter { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<SiteUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            userIdentity.AddClaim(new Claim(ClaimTypes.Email, Email));

            if (!String.IsNullOrEmpty(FirstName))
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.GivenName, FirstName));
            }

            if (!String.IsNullOrEmpty(LastName))
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.Surname, LastName));
            }
            return userIdentity;
        }

        private CustomerAddress CreateCustomerAddress(IOrderAddress orderAddress, CustomerAddressTypeEnum addressType)
        {
            var address = CustomerAddress.CreateInstance();
            address.Name = orderAddress.Id;
            address.AddressType = addressType;
            address.PostalCode = orderAddress.PostalCode;
            address.City = orderAddress.City;
            address.CountryCode = orderAddress.CountryCode;
            address.CountryName = orderAddress.CountryName;
            address.State = orderAddress.RegionName;
            address.Email = orderAddress.Email;
            address.FirstName = orderAddress.FirstName;
            address.LastName = orderAddress.LastName;
            address.Line1 = orderAddress.Line1;
            address.Line2 = orderAddress.Line2;
            address.DaytimePhoneNumber = orderAddress.DaytimePhoneNumber;
            address.EveningPhoneNumber = orderAddress.EveningPhoneNumber;
            address.RegionCode = orderAddress.RegionCode;
            address.RegionName = orderAddress.RegionName;
            return address;
        }
    }
}
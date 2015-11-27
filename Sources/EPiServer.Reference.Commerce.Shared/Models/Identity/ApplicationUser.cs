using System;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Shared.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
        }

        /// <summary>
        /// Returns a new instance of an ApplicationUser based on a previously made purchase order.
        /// </summary>
        /// <param name="purchaseOrder"></param>
        public ApplicationUser(PurchaseOrder purchaseOrder)
        {
            Addresses = new List<CustomerAddress>();

            var firstAddress = purchaseOrder.OrderAddresses.FirstOrDefault();

            if (firstAddress != null)
            {
                Email = firstAddress.Email;
                UserName = firstAddress.Email;
                FirstName = firstAddress.FirstName;
                LastName = firstAddress.LastName;
            }

            foreach (OrderAddress orderAddress in purchaseOrder.OrderAddresses)
            {
                CustomerAddress address = CustomerAddress.CreateInstance();
                address.AddressType = CustomerAddressTypeEnum.Shipping;
                address.PostalCode = orderAddress.PostalCode;
                address.City = orderAddress.City;
                address.CountryCode = orderAddress.CountryCode;
                address.CountryName = orderAddress.CountryName;
                address.State = orderAddress.State;
                address.Email = orderAddress.Email;
                address.FirstName = orderAddress.FirstName;
                address.LastName = orderAddress.LastName;
                address.Created = orderAddress.Created;
                address.Line1 = orderAddress.Line1;
                address.Line2 = orderAddress.Line2;
                address.DaytimePhoneNumber = orderAddress.DaytimePhoneNumber;
                address.EveningPhoneNumber = orderAddress.EveningPhoneNumber;
                address.Name = orderAddress.Name;
                address.RegionCode = orderAddress.RegionCode;
                address.RegionName = orderAddress.RegionName;

                Addresses.Add(address);
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

        public bool NewsLetter { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
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
    }
}
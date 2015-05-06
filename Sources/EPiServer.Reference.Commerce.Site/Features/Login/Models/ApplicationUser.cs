using EPiServer.Reference.Commerce.Site.Features.Registration.Models;
using Mediachase.Commerce.Orders;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Mediachase.Commerce.Customers;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Models
{
    /// <summary>
    /// An application user built for the ASP.NET Identity framework.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Default public constructor.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the user's order addresses.
        /// </summary>
        [NotMapped]
        public List<CustomerAddress> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        [NotMapped]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        [NotMapped]
        public string LastName { get; set; }

        /// <summary>
        /// Gets the source from which the user's details where originally collected.
        /// </summary>
        [NotMapped]
        public string RegistrationSource { get; set; }

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        [NotMapped]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets whether the user subscribes to News letter or not.
        /// </summary>
        public bool NewsLetter { get; set; }

        /// <summary>
        /// Creates and returns a ClaimsIdentity asynchronously.
        /// </summary>
        /// <param name="manager">An instance of the site's UserManager.</param>
        /// <returns>Returns a ClaimsIdentity for the user.</returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
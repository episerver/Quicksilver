using Mediachase.Commerce.Customers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Models
{
    /// <summary>
    /// An IdentityResult in which also a CustomerContact is involved.
    /// </summary>
    public class ContactIdentityResult
    {

        private CustomerContact _contact;
        private IdentityResult _result;

        /// <summary>
        /// Returns a new instance of a ContactIdentityResult.
        /// </summary>
        /// <param name="result">An IdentityResult created as part of an ASP.NET Identity action.</param>
        /// <param name="contact">A CustomerContact entity related to the IdentityResult.</param>
        public ContactIdentityResult(IdentityResult result, CustomerContact contact)
        {
            _result = result;
            _contact = contact;
        }

        /// <summary>
        /// Gets the result of the Identity action.
        /// </summary>
        public IdentityResult Result
        {
            get { return _result; }
        }
        
        /// <summary>
        /// Gets the CustomerContact involved in the Identity action.
        /// </summary>
        public CustomerContact Contact
        {
            get { return _contact; }
        }
        
    }
}
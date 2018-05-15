using Mediachase.Commerce.Customers;
using Microsoft.AspNet.Identity;

namespace EPiServer.Reference.Commerce.Shared.Identity
{
    /// <summary>
    /// An IdentityResult in which also a CustomerContact is involved.
    /// </summary>
    public class ContactIdentityResult
    {
        /// <summary>
        /// Returns a new instance of a ContactIdentityResult.
        /// </summary>
        /// <param name="result">An IdentityResult created as part of an ASP.NET Identity action.</param>
        /// <param name="contact">A CustomerContact entity related to the IdentityResult.</param>
        public ContactIdentityResult(IdentityResult result, CustomerContact contact)
        {
            Contact = contact;
            Result = result;
        }

        /// <summary>
        /// Gets the CustomerContact involved in the Identity action.
        /// </summary>
        public CustomerContact Contact { get; }

        /// <summary>
        /// Gets the outcome of the related identity action.
        /// </summary>
        public IdentityResult Result { get; }
    }
}
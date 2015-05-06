using Mediachase.Commerce.Customers;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword
{
    public interface IPasswordHashService
    {
        byte[] CreateHash(string email);
        CustomerContact DecodeValidateHash(string hash);
    }
}
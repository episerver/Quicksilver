using System.Collections.Specialized;
using System.Net.Mail;
using EPiServer.Core;
using Microsoft.AspNet.Identity;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    public interface IMailService : IIdentityMessageService
    {
        void Send(PageReference mailReference, NameValueCollection nameValueCollection, string toEmail, string language);
        void Send(string subject, string body, string toEmail);
        void Send(MailMessage message);
        string GetHtmlBodyForMail(PageReference mailReference, NameValueCollection nameValueCollection, string langauge);
    }
}
using System.Collections.Specialized;
using System.Net.Mail;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    public interface IMailSender
    {
        void Send(PageReference mailReference, NameValueCollection nameValueCollection, string toEmail);
        void Send(string subject, string body, string toEmail);
        void Send(MailMessage message);
    }
}
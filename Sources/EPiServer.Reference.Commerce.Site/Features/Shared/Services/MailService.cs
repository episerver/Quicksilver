using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Web.Routing;
using EPiServer.ServiceLocation;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    [ServiceConfiguration(typeof(IMailService), Lifecycle = ServiceInstanceScope.PerRequest)]
    public class MailService : IMailService
    {
        private readonly HttpContextBase _httpContextBase;
        private readonly UrlResolver _urlResolver;
        private readonly IContentLoader _contentLoader;
        private readonly IHtmlDownloader _htmlDownloader;

        public MailService(HttpContextBase httpContextBase, 
            UrlResolver urlResolver, 
            IContentLoader contentLoader,
            IHtmlDownloader htmlDownloader)
        {
            _httpContextBase = httpContextBase;
            _urlResolver = urlResolver;
            _contentLoader = contentLoader;
            _htmlDownloader = htmlDownloader;
        }

        public void Send(PageReference mailReference, NameValueCollection nameValueCollection, string toEmail, string language)
        {
            var body = GetHtmlBodyForMail(mailReference, nameValueCollection, language);
            var mailPage = _contentLoader.Get<MailBasePage>(mailReference);

            Send(mailPage.MailTitle, body, toEmail);
        }

        public string GetHtmlBodyForMail(PageReference mailReference, NameValueCollection nameValueCollection, string language)
        {
            var urlBuilder = new UrlBuilder(_urlResolver.GetUrl(mailReference, language))
            {
                QueryCollection = nameValueCollection
            };

            string basePath = _httpContextBase.Request.Url.GetLeftPart(UriPartial.Authority);
            string relativePath = urlBuilder.ToString();
            
            if (relativePath.StartsWith(basePath))
            {
                relativePath = relativePath.Substring(basePath.Length);
            }

            string body = _htmlDownloader.Download(basePath, relativePath);

            return body;
        }

        public void Send(string subject, string body, string recipientMailAddress)
        {
            MailMessage message = new MailMessage()
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(recipientMailAddress);

            Send(message);
        }

        public void Send(MailMessage message)
        {
            using (SmtpClient client = new SmtpClient())
            {
                // The SMTP host, port and sender e-mail address are configured
                // in the system.net section in web.config.
                client.Send(message);
            }
        }

        public Task SendAsync(IdentityMessage message)
        {
            Send(message.Subject, message.Body, message.Destination);
            return Task.FromResult(0);
        }
    }
}
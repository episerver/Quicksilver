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

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    [ServiceConfiguration(typeof(IMailSender), Lifecycle = ServiceInstanceScope.Singleton)]
    public class MailSender : IMailSender
    {
        private readonly HttpContextBase _httpContextBase;
        private readonly UrlResolver _urlResolver;
        private readonly IContentLoader _contentLoader;
        private readonly IHtmlDownloader _htmlDownloader;

        public MailSender(HttpContextBase httpContextBase, 
            UrlResolver urlResolver, 
            IContentLoader contentLoader,
            IHtmlDownloader htmlDownloader)
        {
            _httpContextBase = httpContextBase;
            _urlResolver = urlResolver;
            _contentLoader = contentLoader;
            _htmlDownloader = htmlDownloader;
        }

        public void Send(PageReference mailReference, NameValueCollection nameValueCollection, string toEmail)
        {
            var urlBuilder = new UrlBuilder(_urlResolver.GetUrl(mailReference))
                {
                    QueryCollection = nameValueCollection
                };

            var body = _htmlDownloader.Download(_httpContextBase.Request.Url.GetLeftPart(UriPartial.Authority), urlBuilder.ToString());

            var mailPage = _contentLoader.Get<MailBasePage>(mailReference);

            Send(mailPage.MailTitle, body, toEmail);
        }

        /// <summary>
        /// Creates a new e-mail message and sends it to the recipient.
        /// </summary>
        /// <param name="subject">The subject of the e-mail message.</param>
        /// <param name="body">The HTML string representing the body of the e-mail message.</param>
        /// <param name="recipientMailAddress">The receiver's e-mail address.</param>
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

        /// <summary>
        /// Sends an e-mail message.
        /// </summary>
        /// <param name="message">The e-mail message to send.</param>
        public void Send(MailMessage message)
        {
            using (SmtpClient client = new SmtpClient())
            {
                // The SMTP host, port and sender e-mail address are configured
                // in the system.net section in web.config.
                client.Send(message);
            }
        }
    }
}
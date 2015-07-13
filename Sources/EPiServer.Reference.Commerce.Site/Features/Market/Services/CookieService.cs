using System;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Services
{
    public class CookieService
    {
        public virtual string Get(string cookie)
        {
            if (HttpContext.Current == null)
            {
                return null;
            }

            return HttpContext.Current.Request.Cookies[cookie] == null ? null : HttpContext.Current.Request.Cookies[cookie].Value;
        }

        public virtual void Set(string cookie, string value)
        {
            if (HttpContext.Current != null)
            {
                var httpCookie = new HttpCookie(cookie)
                {
                    Value = value, Expires = DateTime.Now.AddYears(1)
                };

                Set(HttpContext.Current.Request.Cookies, httpCookie);
                Set(HttpContext.Current.Response.Cookies, httpCookie);
            }
        }

        private void Set(HttpCookieCollection cookieCollection, HttpCookie cookie)
        {
            if (cookieCollection.AllKeys.Contains(cookie.Name))
            {
                cookieCollection.Remove(cookie.Name);
            }

            cookieCollection.Add(cookie);
        }
    }
}
using System;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Services
{
    public class CookieService
    {
        public virtual string Get(string cookie)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request.Cookies[cookie] == null)
            {
                return null;
            }

            // Because of we can have multiple cookies of the same name, so that, we should get latest value.
            var lastCookieIndex = Array.FindLastIndex(HttpContext.Current.Request.Cookies.AllKeys, c => c.Equals(cookie, StringComparison.Ordinal));

            return lastCookieIndex == -1 ? null : HttpContext.Current.Request.Cookies[lastCookieIndex].Value;
        }

        public virtual void Set(string cookie, string value)
        {
            if (HttpContext.Current != null)
            {
                var httpCookie = new HttpCookie(cookie, value)
                {
                    Expires = DateTime.Now.AddYears(1)
                };

                Set(httpCookie);
            }
        }

        public virtual void Remove(string cookie)
        {
            if (HttpContext.Current != null)
            {
                var httpCookie = new HttpCookie(cookie)
                {
                    Expires = DateTime.Now.AddDays(-1)
                };

                Set(httpCookie);
            }
        }

        private void Set(HttpCookie cookie)
        {
            HttpContext.Current.Response.SetCookie(cookie);
        }
    }
}
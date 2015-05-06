using System;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Market
{
    public class CookieService
    {
        public string Get(string cookie)
        {
            if (HttpContext.Current == null)
            {
                return null;
            }
            return HttpContext.Current.Request.Cookies[cookie] == null ? null : HttpContext.Current.Request.Cookies[cookie].Value;
        }

        public void Set(string cookie, string value)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(cookie) { Value = value, Expires = DateTime.Now.AddYears(1) });
            }
        }
    }
}
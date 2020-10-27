using EPiServer.Commerce.UI.CustomerService.Extensibility;
using EPiServer.Commerce.UI.CustomerService.Routing;
using System.Web.Http;

namespace EPiServer.Reference.Commerce.Site.CSRExtensibility.Controllers
{
    [EpiRoutePrefix("csr-demo")]
    public class DemoApiController: CSRAPIController
    {
        [HttpGet]
        [EpiRoute("getData")]
        public IHttpActionResult Get()
        {
            return Ok("Sample data");
        }
    }
}
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Web;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    [ServiceConfiguration(typeof(IViewTemplateModelRegistrator))]
    public class ViewTemplateModelRegistrator : IViewTemplateModelRegistrator
    {
        public void Register(TemplateModelCollection viewTemplateModelRegistrator)
        {
            viewTemplateModelRegistrator.Add(typeof(PageData), new TemplateModel
            {
                Name = "PartialPage",
                Inherit = true,
                AvailableWithoutTag = true,
                TemplateTypeCategory = TemplateTypeCategories.MvcPartialView,
                Path = "~/Views/Shared/_Page.cshtml"
            });
        }
    }
}
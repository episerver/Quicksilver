using EPiServer.Core;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels
{
    public class PageViewModel<T> where T : PageData
    {
        public T CurrentPage { get; set; }
    }
}
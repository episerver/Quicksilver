namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    public interface IHtmlDownloader
    {
        string Download(string baseUrl, string relativeUrl);
    }
}
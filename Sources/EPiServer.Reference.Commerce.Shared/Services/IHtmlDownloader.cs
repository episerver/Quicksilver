namespace EPiServer.Reference.Commerce.Shared.Services
{
    public interface IHtmlDownloader
    {
        string Download(string baseUrl, string relativeUrl);
    }
}
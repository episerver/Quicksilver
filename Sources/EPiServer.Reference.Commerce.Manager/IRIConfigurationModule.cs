using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.Reference.Commerce.Manager
{
    // To support an extended range of characters (outside of what has been defined in RFC1738) in URLs, define IRI_CHARACTERS_IN_URL_FEATURE symbol - this should be done both in Commerce Manager and Front-end sites.
    // More information about this feature: http://world.episerver.com/documentation/developer-guides/CMS/routing/internationalized-resource-identifiers-iris/

    // The code block below will allow general Unicode letter characters. 
    // To support more Unicode blocks, update the regular expression for ValidCharacters.
    // For example, to support Thai Unicode block, add \p{IsThai} to it.
    // The supported Unicode blocks can be found here: https://msdn.microsoft.com/en-us/library/20bw873z(v=vs.110).aspx#Anchor_12

#if IRI_CHARACTERS_IN_URL_FEATURE
    [ModuleDependency(typeof(Web.InitializationModule))]
    public class IRIConfigurationModule : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.RemoveAll<UrlSegmentOptions>();
            context.Services.AddSingleton(s => new UrlSegmentOptions
            {
                SupportIriCharacters = true,
                ValidCharacters = @"\p{L}0-9\-_~\.\$"
            });
        }

        public void Initialize(InitializationEngine context)
        { }

        public void Uninitialize(InitializationEngine context)
        { }
    }
#endif
}
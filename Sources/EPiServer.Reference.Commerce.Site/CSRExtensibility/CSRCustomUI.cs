using EPiServer.Commerce.UI.CustomerService.Extensibility;
using EPiServer.ServiceLocation;

namespace EPiServer.Reference.Commerce.Site.CSRExtensibility
{
    [ServiceConfiguration(typeof(CSRUIExtensionConfiguration))]
    public class CSRCustomUI : CSRUIExtensionConfiguration
    {
        public CSRCustomUI()
        {
            // comment out the line below to make the CSR extended components samples visible
            //ResourceScripts = new string[] { "/CSRExtensibility/react-app/dist/customized-csr-bundle.min.js" };
        }
    }
}
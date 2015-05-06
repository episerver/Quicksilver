using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Shell;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Descriptors
{
    [UIDescriptorRegistration]
    public class FashionProductUIDescriptor : UIDescriptor<FashionProduct>
    {
        public FashionProductUIDescriptor()
            : base(ContentTypeCssClassNames.Container)
        {
            DefaultView = CmsViewNames.OnPageEditView;
        }
    }
}
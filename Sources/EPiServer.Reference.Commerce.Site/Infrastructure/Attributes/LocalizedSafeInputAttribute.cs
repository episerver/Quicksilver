namespace EPiServer.Reference.Commerce.Site.Infrastructure.Attributes
{
    public class LocalizedSafeInputAttribute : LocalizedRegularExpressionAttribute
    {
        public LocalizedSafeInputAttribute(string name)
            : base(@"^(?!(.*<.+>.*))(.*)$", name)
        {
        }
    }
}
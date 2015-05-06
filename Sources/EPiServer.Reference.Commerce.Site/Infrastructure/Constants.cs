using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    public static class Constants
    {
        /// <summary>
        /// Tags to use for the main widths used in the Bootstrap HTML framework
        /// </summary>
        public static class ContentAreaTags
        {
            public const string FullWidth = "col-md-12";
            public const string TwoThirdsWidth = "col-md-8";
            public const string HalfWidth = "col-md-6";
            public const string OneThirdWidth = "col-md-4";
            public const string NoRenderer = "norenderer";
        }

        /// <summary>
        /// Main widths used in the Bootstrap HTML framework
        /// </summary>
        public static class ContentAreaWidths
        {
            public const int FullWidth = 12;
            public const int TwoThirdsWidth = 8;
            public const int HalfWidth = 6;
            public const int OneThirdWidth = 4;
        }

        public static Dictionary<string, int> ContentAreaTagWidths = new Dictionary<string, int>
        {
            { ContentAreaTags.FullWidth, ContentAreaWidths.FullWidth },
            { ContentAreaTags.TwoThirdsWidth, ContentAreaWidths.TwoThirdsWidth },
            { ContentAreaTags.HalfWidth, ContentAreaWidths.HalfWidth },
            { ContentAreaTags.OneThirdWidth, ContentAreaWidths.OneThirdWidth }
        };
    }
}


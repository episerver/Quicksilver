using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Descriptors
{
    [EditorDescriptorRegistration(TargetType = typeof(ContentData))]
    public class MoveCategoryEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            foreach (var property in metadata.Properties.OfType<ExtendedMetadata>())
            {
                if (property.PropertyName == "icategorizable_category")
                {
                    property.GroupName = SystemTabNames.PageHeader;
                    property.Order = 9000;
                }
            }
        }
    }
}
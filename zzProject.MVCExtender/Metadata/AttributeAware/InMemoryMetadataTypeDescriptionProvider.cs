using System;
using System.ComponentModel;
using System.Diagnostics;


//See: http://blogs.msdn.com/b/davidebb/archive/2009/07/24/using-an-associated-metadata-class-outside-dynamic-data.aspx
//See the method GetTypeDescriptor of AssociatedMetadataProvider (DataAnnotationsModelMetadataProvider inherits from this):

//protected virtual ICustomTypeDescriptor GetTypeDescriptor(Type type) {
//    return TypeDescriptorHelper.Get(type);
//}
//You can override the GetTypeDescriptor to use the InMemoryMetadataTypeDescriptionProvider.

//See: http://www.paraesthesia.com/archive/2010/01/28/separating-metadata-classes-from-model-classes-in-dataannotations-using-custom.aspx
//See: http://blogs.msdn.com/b/davidebb/archive/2008/06/16/dynamic-data-and-the-associated-metadata-class.aspx
//See: http://blogs.msdn.com/b/marcinon/archive/2008/05/22/dynamic-data-samples-custom-metadata-providers.aspx
//See: http://mattberseth.com/blog/2008/08/dynamic_data_and_custom_metada.html
namespace zzProject.MVCExtender.Metadata.AttributeAware
{
    public class InMemoryMetadataTypeDescriptionProvider : TypeDescriptionProvider {
        private Type Type { get; set; }

        // Creates an instance for the given type. The InMemoryMetadataTypeDescriptionProvider will fall back
        // to default reflection-based behavior when retrieving attributes.
        public InMemoryMetadataTypeDescriptionProvider(Type type)
            : this(type, TypeDescriptor.GetProvider(type)) {
            Type = type;
        }

        // Creates an instance for the given type. The InMemoryMetadataTypeDescriptionProvider will use the given
        // parent provider for chaining 
        public InMemoryMetadataTypeDescriptionProvider(Type type, TypeDescriptionProvider parentProvider)
            : base(parentProvider) {
            Type = type;
        }
        
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
            Debug.Assert(objectType == Type);
            return new InMemoryMetadataTypeDescriptor(base.GetTypeDescriptor(objectType, instance), Type);
        }
    }
}

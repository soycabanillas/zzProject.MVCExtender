using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace zzProject.MVCExtender.Metadata.Provider
{
    public class FluentMetadataProviderEx : FluentMetadataProvider
    {

        protected override System.ComponentModel.ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            ICustomTypeDescriptor parentTypeDescriptor = base.GetTypeDescriptor(type);
            return new AttributeAware.InMemoryMetadataTypeDescriptor(parentTypeDescriptor, type);
        }
    }
}
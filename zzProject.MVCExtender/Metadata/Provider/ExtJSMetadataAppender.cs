using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Web.Mvc;

namespace zzProject.MVCExtender.Metadata.Provider
{
    public class ExtJSMetadataAppender : DataAnnotationsModelMetadataProvider
    {
        public ModelMetadata AddMetadata(ModelMetadata modelMetadata,
                                                        IEnumerable<Attribute> attributes,
                                                        Type containerType,
                                                        Func<object> modelAccessor,
                                                        Type modelType,
                                                        string propertyName)
        {


            return modelMetadata;
        }

    }
}
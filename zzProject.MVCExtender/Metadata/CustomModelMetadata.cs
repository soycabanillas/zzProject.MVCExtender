using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
//using zzProject.MVCExtender.ExtJS.ModelMetadata;
using zzProject.MVCExtender.Metadata.CommonExtension;

namespace zzProject.MVCExtender.Metadata
{
    public class CustomModelMetadata : DataAnnotationsModelMetadata, ICommonExtensionModelMetadataGetter//, IExtJSModelMetadataGetter
    {
        public CustomModelMetadata(DataAnnotationsModelMetadataProvider provider, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName, DisplayColumnAttribute displayColumnAttribute)
            : base(provider, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute)
        {
        }

        private CommonExtensionModelMetadata _commonExtensionModelMetadata;
        public CommonExtensionModelMetadata getMM_CommonExtensionModelMetadata()
        {
            if (this._commonExtensionModelMetadata == null)
            {
                this._commonExtensionModelMetadata = new CommonExtensionModelMetadata(this);
            }
            return this._commonExtensionModelMetadata;
        }

        //private ExtJSModelMetadata _extJSModelMetadata;
        //public ExtJSModelMetadata getMM_ExtJSModelMetadata()
        //{
        //    if (this._extJSModelMetadata == null)
        //    {
        //        this._extJSModelMetadata = new ExtJSModelMetadata(this);
        //    }
        //    return this._extJSModelMetadata;
        //}
    }
}

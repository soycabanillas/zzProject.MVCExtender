using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace zzProject.MVCExtender.Metadata.CommonExtension
{
    public class CommonExtensionModelMetadata
    {
        public CommonExtensionModelMetadata(ModelMetadata modelMetadata)
        {
            this._modelMetadata = modelMetadata;
        }
        public ModelMetadata _modelMetadata;
        public ModelMetadata ModelMetadata { get { return this._modelMetadata; } }

        public bool IsKey { get; set; }
    }
}

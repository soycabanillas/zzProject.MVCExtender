using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Web;

namespace zzProject.MVCExtender
{
    public class Common
    {
        public const string HTTP_CONTEXT_MVC_EXTENDER_KEY = "__MVCExtender_Key__";
        public const string MVC_EXTENDER_DEFAULT_PREFIX = "MVCExtender_Key_";
        public const string DEFAULT_PARENT_MODEL = "Ext.data.Model";
        public const string DEFAULT_TREESTORE_TREECOLUMN = "treecolumn";

        public static string UnikeID(string prefix = null)
        {
            int result = 0;
            object key = HttpContext.Current.Items[HTTP_CONTEXT_MVC_EXTENDER_KEY];
            if (key != null)
            {
                result = (int)key + 1;
            }
            HttpContext.Current.Items[HTTP_CONTEXT_MVC_EXTENDER_KEY] = result;
            return (prefix == null ? MVC_EXTENDER_DEFAULT_PREFIX : prefix) + result.ToString();
        }
    }
}

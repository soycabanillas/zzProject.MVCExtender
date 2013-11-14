using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace zzProject.MVCExtender.Routing
{
    public class StaticFileRouteHandler : IRouteHandler
    {
        private string requestFilePath;
        private string virtualFilePath;

        public StaticFileRouteHandler(string requestFilePath, string virtualFilePath)
        {
            // make sure something was passed in
            if (string.IsNullOrEmpty(requestFilePath))
            {
                throw new ArgumentNullException("requestFilePath");
            }
            if (string.IsNullOrEmpty(virtualFilePath))
            {
                throw new ArgumentNullException("virtualFilePath");
            }

            this.requestFilePath = requestFilePath;
            this.virtualFilePath = virtualFilePath;
        }

        public System.Web.IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            if (requestContext.HttpContext.Request.FilePath == this.requestFilePath)
            {
                HttpContext.Current.RewritePath(virtualFilePath);
            }
            return new DefaultHttpHandler();
        }
    }
}

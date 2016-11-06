using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Flh.Web
{
    public static class UrlHelperExtenstion
    {
        public static bool IsCurrent(this UrlHelper url, string actionName, string controllerName)
        {
            var currentController = url.RequestContext.RouteData.Values["controller"].ToString();
            var currentAction = url.RequestContext.RouteData.Values["action"].ToString();

            return StringComparer.CurrentCultureIgnoreCase.Compare(actionName, currentAction) == 0
                && StringComparer.CurrentCultureIgnoreCase.Compare(controllerName, currentController) == 0;
        }
    }
}

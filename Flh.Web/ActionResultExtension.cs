using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace Flh.Web
{
    public static class ActionResultExtension
    {
        public static RedirectToRouteResult SetBackUrl(this RedirectToRouteResult result, string backurl)
        {
            ExceptionHelper.ThrowIfNull(result, "result");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(backurl, "backurl");
            var dic = new RouteValueDictionary(result.RouteValues);
            dic[Config.Current.BACK_URL_KEY] = backurl;
            return new RedirectToRouteResult(result.RouteName, dic, result.Permanent);
        }
    }
}

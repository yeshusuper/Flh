using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Flh.Web;

namespace Flh.Web
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class FlhAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        public FlhAuthorizeAttribute()
        {
        }

        protected bool AuthorizeCore(System.Web.HttpContextBase httpContext, UserSessionEntry user)
        {
            return user != null && user.Uid > 0;
        }

        protected ActionResult UnauthorizedResult
        {
            get
            {
                var backUrl = HttpUtility.UrlEncode(HttpContext.Current.Request.RawUrl);
                return new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary { 
                    { "action", "login" },
                    { "controller", "account"}
                }).SetBackUrl(backUrl);
            }
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            return AuthorizeCore(httpContext, httpContext.Session.GetCurrentUser());
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = UnauthorizedResult;
        }
    }
}

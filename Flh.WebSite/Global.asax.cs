using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Flh.WebSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            InjectConfig.Register();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            if (ex != null)
            {
                var bhEx = ex as FlhException;
                var context = new HttpContextWrapper(Context);
                var errorCode = bhEx == null ? ErrorCode.ServerError : bhEx.ErrorCode;
                var errorMsg = bhEx == null ? ex.ToString() : bhEx.Message;
                if (context.Request.IsAjaxRequest())
                {
                    Context.Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new Web.JsonResultEntry
                    {
                        Code = errorCode,
                        Message = errorMsg
                    }));
                }
                else
                {
                    IController ec = new Controllers.ErrorController();
                    var routeData = new RouteData();
                    routeData.Values["action"] = "index";
                    routeData.Values["controller"] = "error";
                    routeData.DataTokens["code"] = errorCode;
                    routeData.DataTokens["msg"] = errorMsg;
                    ec.Execute(new RequestContext(context, routeData));
                }
                Server.ClearError();
            }
        }
    }
}
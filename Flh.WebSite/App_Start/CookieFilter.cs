using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Flh.Business;
using Flh.Web;
using Ninject;

namespace Flh.WebSite.App_Start
{
    public class CookieFilter : FilterAttribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = filterContext.HttpContext.Session.GetCurrentUser();
            if (user == null || user.Uid <= 0)
            {
                ICookieService cookieService = new Flh.Web.CookieServiceImpl();
                var cookieUser = cookieService.User;
                if (cookieUser != null)
                {
                    try
                    {
                        IKernel ninjectKernel = new Ninject.StandardKernel(new Ninject.NinjectSettings()
                        {
                            DefaultScopeCallback = ctx => System.Web.HttpContext.Current
                        },
                         new Flh.Business.Inject.DataModule(),
                         new Flh.Business.Inject.ServiceModule()
                         );

                        IUserManager userManager = ninjectKernel.Get<IUserManager>();
                        var userService = userManager.Login(cookieUser.un, cookieUser.pwd, filterContext.HttpContext.Request.GetCurrentIP());
                        var entry = new UserSessionEntry
                          {
                              Name = userService.Name,
                              Uid = userService.Uid
                          };
                        filterContext.HttpContext.Session.SetCurrentUser(entry);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}

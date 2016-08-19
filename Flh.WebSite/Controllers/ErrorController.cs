using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.WebSite.Controllers
{
    public class ErrorController : Controller
    {        
        public ActionResult Index()
        {
            var msg = "出错啦";
            if (this.RouteData.DataTokens.ContainsKey("msg"))
            {
                msg = this.RouteData.DataTokens["msg"].ToString();
            }
            ViewBag.Message = msg;
            return View("Index");
        }
    }
}

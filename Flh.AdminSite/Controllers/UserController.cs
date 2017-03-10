using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.AdminSite.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Detail(long id)
        {
            ViewBag.Uid = id;
            return View();
        }

    }
}

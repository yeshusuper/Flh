using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flh.Web;

namespace Flh.AdminSite.Controllers
{
    [FlhAuthorize]
    public class ClassesController : Controller
    {
        public ActionResult List()
        {
            return View();
        }

    }
}

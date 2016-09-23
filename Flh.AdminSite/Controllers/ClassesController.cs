using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flh.Web;
using Flh.Business;

namespace Flh.AdminSite.Controllers
{
    [FlhAuthorize]
    public class ClassesController : Controller
    {
        private readonly IClassesManager _ClassesManager;

        public ClassesController(IClassesManager classesManager)
        {
            _ClassesManager = classesManager;
        }

        public ActionResult List()
        {
            return View(new Models.Classes.ListModel());
        }

        [HttpGet]
        public ActionResult BatchAdd(string pno)
        {
            var parent = _ClassesManager.GetEnabled(pno);

            return View(new Models.Classes.BatchAddModel(6)
            {
                ParentNo = parent.no,
                ParentFullName = parent.full_name
            });
        }
    }
}

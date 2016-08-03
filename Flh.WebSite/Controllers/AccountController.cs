using Flh.Business;
using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.WebSite.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IUserManager _UserManager;

        public AccountController(IUserManager userManager)
        {
            _UserManager = userManager;
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Models.Account.RegisterModel info)
        {
            _UserManager.Register(info);
            return JsonResult(ErrorCode.None, "注册成功");
        }
    }
}

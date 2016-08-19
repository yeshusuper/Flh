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
            return SuccessJsonResult();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Models.Account.LoginModel model)
        {
            var user = _UserManager.Login(model.UserName, model.Password, Request.GetCurrentIP());
            var entry = new UserSessionEntry
            {
                Name = user.Name,
                Uid = user.Uid
            };
            Session.SetCurrentUser(entry);
            return SuccessJsonResult();
        }

        public ActionResult Logout()
        {
            Session.SetCurrentUser(null);
            return RedirectToAction("index", "home");
        }
    }
}

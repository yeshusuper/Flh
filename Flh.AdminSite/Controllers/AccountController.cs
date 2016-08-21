using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flh.Business.Admins;
using Flh.Web;

namespace Flh.AdminSite.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAdminManager _AdminManager;

        public AccountController(IAdminManager adminManager)
        {
            _AdminManager = adminManager;
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Models.Account.LoginModel model)
        {
            var admin = _AdminManager.Login(model.UserName, model.Password, Request.GetCurrentIP());

            var entry = new UserSessionEntry
            {
                Name = admin.Name,
                Uid = admin.Uid
            };
            Session.SetCurrentUser(entry);
            return SuccessJsonResult(Request.GetBackUrl());
        }

        public ActionResult Logout()
        {
            Session.SetCurrentUser(null);
            return RedirectToAction("Index", "Home");
        }
    }
}

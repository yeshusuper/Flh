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
            if (String.IsNullOrWhiteSpace(Session.GetCurrentCertCode()) || Session.GetCurrentCertCode().ToLower() != model.CertCode.ToLower())
            {
                Session.SetCurrentCertCode(String.Empty);
                return JsonResult(ErrorCode.ArgError, "验证码错误");
            }
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
        public ActionResult VerifyImage()
        {
            var s1 = new CertCode();
            var random = new Random();
            string code = random.Next(1000, 9999).ToString();
            byte[] bytes = s1.CreateImage(out code);
            Session.SetCurrentCertCode(code);
            return File(bytes, @"image/jpeg");
        }
    }
}

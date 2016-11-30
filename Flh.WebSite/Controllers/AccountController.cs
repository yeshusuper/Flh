using Flh.Business;
using Flh.Business.Mobile;
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
        private readonly IMobileManager _MobileManager;

        public AccountController(IUserManager userManager, IMobileManager mobileManager)
        {
            _UserManager = userManager;
            _MobileManager = mobileManager;
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
            if (String.IsNullOrWhiteSpace(model.CertCode) || String.IsNullOrWhiteSpace(Session.GetCurrentCertCode()) || Session.GetCurrentCertCode().ToLower() != model.CertCode.ToLower())
            {
                Session.SetCurrentCertCode(String.Empty);
                return JsonResult(ErrorCode.ArgError, "验证码错误");
            }
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
        public ActionResult VerifyImage()
        {
            var s1 = new CertCode();
            var random = new Random();
            string code = random.Next(1000, 9999).ToString();
            byte[] bytes = s1.CreateImage(out code);
            Session.SetCurrentCertCode(code);
            return File(bytes, @"image/jpeg");
        }
        [HttpPost]
        public ActionResult SendVerifyCode(string mobile, VerifyType kind, string certCode)
        {
            if (String.IsNullOrWhiteSpace(certCode) || String.IsNullOrWhiteSpace(Session.GetCurrentCertCode()) || Session.GetCurrentCertCode().ToLower() != certCode.ToLower())
            {
                Session.SetCurrentCertCode(String.Empty);
                return JsonResult(ErrorCode.ArgError, "验证码错误");
            }
            _MobileManager.SendVerifyCode(mobile, kind);
            return SuccessJsonResult();
        }
        [HttpPost]
        public ActionResult VerifyCode(string mobile, string code)
        {
            _MobileManager.Verify(code, mobile);
            Session.SetCurrentVerifyMobile(mobile);
            return SuccessJsonResult();
        }
        [HttpGet, FlhAuthorize]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost, FlhAuthorize]
        public ActionResult ChangePassword(string oldPassword, string newPasswrod)
        {
            _UserManager.Get(this.CurrentUser.Uid).ChangePassword(oldPassword, newPasswrod);
            return SuccessJsonResult();
        }
        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ResetPassword(string mobile, string password)
        {
            _UserManager.ResetPassword(mobile, password);
            return SuccessJsonResult();
        }
        [HttpGet, FlhAuthorize]
        public ActionResult UserInfo()
        {
            var user = _UserManager.GetUsers(new long[] { this.CurrentUser.Uid }).FirstOrDefault();
            if (user == null)
                throw new FlhException(ErrorCode.NotExists, "用户不存在");
            var model = new Models.Account.UserInfoModel(user);
            return View(model);
        }
        [HttpPost, FlhAuthorize]
        public ActionResult UpdateInfo(Models.Account.UserInfoModel info)
        {
            _UserManager.Get(this.CurrentUser.Uid).UpdateInfo(info);
            return SuccessJsonResult();
        }
    }
}
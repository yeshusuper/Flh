using Flh.Business;
using Flh.Business.Mobile;
using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flh.Data;

namespace Flh.WebSite.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IUserManager _UserManager;
        private readonly IMobileManager _MobileManager;
        private readonly IClassesManager _ClassesManager;
        private readonly ITradeManager _TradeManager;
        private readonly IAreaManager _AreaManager;

        public AccountController(IUserManager userManager,
            IMobileManager mobileManager, 
            IClassesManager classesManager, 
            ITradeManager tradeManager, 
            IAreaManager areaManager)
        {
            _UserManager = userManager;
            _MobileManager = mobileManager;
            _ClassesManager = classesManager;
            _TradeManager = tradeManager;
            _AreaManager = areaManager;
        }

        [HttpGet]
        public ActionResult Register()
        {
            ViewBag.Area = Area("0001", 3);
            ViewBag.Trade = Trade(string.Empty);
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

            ICookieService cookieService = new Flh.Web.CookieServiceImpl();

            cookieService.User = new Flh.Web.CookieUser(user.Uid, model.UserName, model.Password, model.Remember);
            return SuccessJsonResult();
        }

        public ActionResult Logout()
        {
            Session.SetCurrentUser(null);
            ICookieService cookieService = new Flh.Web.CookieServiceImpl();
            cookieService.Logout();
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
        public ActionResult SendVerifyCode(string mobile, VerifyType kind)
            {
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
            var verifyMobile = Session.GetCurrentVerifyMobile();
            if (verifyMobile != mobile)
                throw new FlhException(ErrorCode.VerifyCodeExpire, "先验证手机");
            _UserManager.ResetPassword(mobile, password);
            return SuccessJsonResult();
        }
        [HttpGet, FlhAuthorize]
        public ActionResult UserInfo()
        {
            var user = _UserManager.GetUsers(new long[] { this.CurrentUser.Uid }).FirstOrDefault();
            if (user == null)
                throw new FlhException(ErrorCode.NotExists, "用户不存在");
            ViewBag.Area = Area("0001", 3);
            ViewBag.Trade = Trade(string.Empty);
            var model = new Models.Account.UserInfoModel(user);
            return View(model);
        }
        [HttpPost, FlhAuthorize]
        public ActionResult UpdateInfo(Models.Account.UserInfoModel info)
        {
            _UserManager.Get(this.CurrentUser.Uid).UpdateInfo(info);
            return SuccessJsonResult();
        }
        [HttpGet,FlhAuthorize]
        public ActionResult ChangeMobile()
        {
            return View();
        }
        [HttpPost,FlhAuthorize]
        public ActionResult ChangeMobile(string mobile, string code)
        {
            _MobileManager.Verify(code, mobile);
            Session.SetCurrentVerifyMobile(mobile);
            _UserManager.Get(this.CurrentUser.Uid).ChangeMobile(mobile);
            return SuccessJsonResult();
        }
        private string  Area(string parent, int deep = 3)
        {
            Response.Cache.SetOmitVaryStar(true);
            deep = Math.Max(1, deep);
            var maxLength = (parent ?? String.Empty).Trim().Length + 4 * deep;

            var areaInfos = _AreaManager.EnabledAreas.Where(a => a.area_no.Length <= maxLength);
            if (!String.IsNullOrWhiteSpace(parent))
                areaInfos = areaInfos.Where(a => a.area_no.StartsWith(parent) && a.area_no.Length > parent.Length);

            var result = areaInfos.OrderByDescending(a => a.order_by)
                 .ThenBy(a => a.area_no)
                 .Select(t => new Models.ClassTreeModel
                 {
                     ClassName = t.area_name,
                     ClassNo = t.area_no,
                 })
                .ToArray().Stratification().ToArray();
            return Newtonsoft.Json.JsonConvert.SerializeObject(result);
        }
        private  string Trade(string parent, int deep = 2)
        {
            Response.Cache.SetOmitVaryStar(true);
            deep = Math.Max(1, deep);
            var maxLength = (parent ?? String.Empty).Length + 4 * deep;

            var areaInfos = _TradeManager.EnabledTrades.Where(c => c.no.Length <= maxLength);
            if (!String.IsNullOrWhiteSpace(parent))
                areaInfos = areaInfos.Where(c => c.no.StartsWith(parent) && c.no.Length > parent.Length);

            var result = areaInfos.OrderByDescending(a => a.order_by)
                 .ThenBy(c => c.no)
                 .Select(t => new Models.ClassTreeModel
                 {
                     ClassName = t.name,
                     ClassNo = t.no,
                 })
                .ToArray().Stratification().ToArray();
            return Newtonsoft.Json.JsonConvert.SerializeObject(result);
        }
    }
}
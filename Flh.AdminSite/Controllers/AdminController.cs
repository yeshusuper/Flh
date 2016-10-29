using Flh.Business;
using Flh.Business.Data;
using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.AdminSite.Controllers
{
     [FlhAuthorize]
    public class AdminController : BaseController
    {
        private readonly IAdminManager _AdminManager;
        private readonly IUserManager _UserManager;
        private readonly IUserRepository _UserRepository;
        public AdminController(IAdminManager adminManager, IUserManager userManager, IUserRepository userRepository)
        {
            _AdminManager = adminManager;
            _UserManager = userManager;
            _UserRepository = userRepository;
        }

        public ActionResult List(String mobile="")
        {
            if(String.IsNullOrWhiteSpace(mobile))
            {
                var adminUsers = _AdminManager.EnabledEntities.ToArray();
                var uids = adminUsers.Select(u => u.uid).ToArray();
                var users = _UserManager.GetUsersByIds(uids).ToDictionary(d => d.Uid);
                var models = adminUsers.Select(d => new AdminManageListItemModel { 
                    Uid = d.uid,
                    CreateDate = d.created,
                    IsAdmin=true,
                }).ToArray();

                foreach (var model in models)
                {
                    if (users.ContainsKey(model.Uid))
                    {
                        var user = users[model.Uid];
                        model.UserName = user.Name;
                        model.Mobile = user.Mobile;
                    }
                }
                return View(models);
            }
            else
            {
                ViewBag.Mobile = mobile;
                var users = _UserRepository.EnabledUsers.Where(u=>u.mobile.Contains(mobile)).ToArray();
                var uids =users.Select(u=>u.uid).ToArray();
                var dictAdmins = _AdminManager.EnabledEntities.Where(a => uids.Contains(a.uid)).ToDictionary(d => d.uid);
                var models = users.Select(d => new AdminManageListItemModel { Uid = d.uid, UserName = d.name,Mobile=d.mobile,IsAdmin=false }).ToArray();
                foreach (var model in models)
                {
                    if (dictAdmins.ContainsKey(model.Uid))
                    {
                        var admin=dictAdmins[model.Uid];
                        model.IsAdmin = true;
                        model.CreateDate = admin.created;
                    }                    
                }
                return View(models);
            }            
        }

        [HttpPost]
        public ActionResult Add(long uid)
        {
            _AdminManager.Add(uid, this.CurrentUser.Uid);
            return SuccessJsonResult();
        }

        [HttpPost]
        public ActionResult Remove(long uid)
        {
            _AdminManager.Remove(adminUid: uid,operatorUid:this.CurrentUser.Uid);
            return SuccessJsonResult();
        }
    }
    public class AdminManageListItemModel
    {
        
        public bool IsAdmin { get; set; }
        public long Uid { get; set; }
        public String UserName { get; set; }
        public String Mobile { get; set; }

        /// <summary>
        /// 管理员设置的时间
        /// </summary>
        public DateTime? CreateDate { get; set; }
    }
}

using Flh.AdminSite.Models.Users;
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
    public class UserController : BaseController
    {
         //private readonly IUserManager _UserManager;
        private readonly IUserRepository _UserRepository;
        private readonly IAreaManager _AreaManager;
        private readonly IClassesManager _ClassManager;
        public UserController(IUserRepository userRepository,IAreaManager areaManager,IClassesManager classManager)
        {
            //_UserManager = userManager;
            _UserRepository = userRepository;
            _AreaManager = areaManager;
            _ClassManager = classManager;
        }
        //注册用户查询（手机、姓名、注册时间、登录时间、采购权限、行业）        
        public ActionResult List(int? page,int? limit,String mobile,String name,
            DateTime? min_register_date,DateTime? max_register_date,
            DateTime? min_login_date, DateTime? max_login_date, String industry_no, bool? is_purchaser,String area_no)
        {
            page = Math.Max(page ?? 0, 1);
            if (!limit.HasValue)
            {
                limit = 30;
            }

            var query = _UserRepository.EnabledUsers;
            if (!String.IsNullOrWhiteSpace(mobile))
            {
                query = query.Where(d => d.mobile.Contains(mobile));
            }
            if (!String.IsNullOrWhiteSpace(name))
            {
                query = query.Where(d => d.name.Contains(name));
            }
            if (min_register_date.HasValue)
            {
                query = query.Where(d => d.register_date >= min_register_date);
            }
            if (max_register_date.HasValue)
            {
                var max_register_date_add = max_register_date.Value.AddDays(1);
                query = query.Where(d => d.register_date < max_register_date_add);
            }
            if (min_login_date.HasValue)
            {
                query = query.Where(d => d.last_login_date >= min_login_date);
            }
            if (max_login_date.HasValue)
            {
                var max_login_date_add = max_login_date.Value.AddDays(1);
                query = query.Where(d => d.last_login_date < max_login_date_add);
            }
            if (!String.IsNullOrWhiteSpace(industry_no))
            {
                query = query.Where(d => d.industry_no.StartsWith(industry_no));
            }
            if (is_purchaser.HasValue)
            {
                query = query.Where(d => d.is_purchaser==is_purchaser.Value);
            }            
            var count = query.Count();
            var pageCount=(int)Math.Ceiling((float)count / (float)limit);
            int start = (page.Value - 1) * limit.Value;
            var orderQuery = query.OrderByDescending(d=>d.register_date);
            var pageList = orderQuery.Skip(start).Take(limit.Value).ToArray();
            var items = pageList.Select(d => new ListItemViewModel { Item=d}).ToArray();

            //获取地区
            var areaNoes = pageList.Select(d => d.area_no).Where(d => !String.IsNullOrWhiteSpace(d)).Distinct().ToArray();
            var areaDict= _AreaManager.EnabledAreas.Where(d => areaNoes.Contains(d.area_no)).ToDictionary(d=>d.area_no,d=>d.area_full_name);
            foreach (var item in items)
            {
                if (!String.IsNullOrWhiteSpace(item.Item.area_no) && areaDict.ContainsKey(item.Item.area_no))
                {
                    item.area = areaDict[item.Item.area_no];
                }
            }

            //获取行业
            var classNoes = pageList.Select(d => d.industry_no).Where(d=>!String.IsNullOrWhiteSpace(d)).Distinct().ToArray();
            var classDict = _ClassManager.EnabledClasses.Where(d => classNoes.Contains(d.no)).ToDictionary(d => d.no, d => d.name);
            foreach (var item in items)
            {
                if (!String.IsNullOrWhiteSpace(item.Item.industry_no) && classDict.ContainsKey(item.Item.industry_no))
                {
                    item.industry = areaDict[item.Item.industry_no];
                }
            }

            var model = new ListViewModel
            {
                Items = new PageModel<ListItemViewModel>(items, page.Value, pageCount),
                area_no = area_no,
                industry_no = industry_no,
                is_purchaser = is_purchaser,
                limit = limit,
                max_login_date = max_login_date,
                max_register_date = max_register_date,
                min_login_date = min_login_date,
                min_register_date = min_register_date,
                mobile = mobile,
                name = name,
                page = page,
            };
            return View(model);
        }
        //用户详细资料查看
        public ActionResult Details(int id)
        {
            var entity = _UserRepository.Entities.FirstOrDefault(d=>d.uid==id);
            return View(new UserViewModel { Item=entity});
        }
    }
}

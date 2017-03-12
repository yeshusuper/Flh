using Flh.AdminSite.Models.Users;
using Flh.Business;
using Flh.Business.Data;
using Flh.Business.Users;
using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.AdminSite.Controllers
{
    [FlhAuthorize]
    public class UserController : BaseController
    {
        private readonly IUserManager _UserManager;
        private readonly IAreaManager _AreaManager;
        private readonly IClassesManager _ClassManager;
        public UserController(IUserManager userManager, IAreaManager areaManager, IClassesManager classManager)
        {
            _UserManager = userManager;
            _AreaManager = areaManager;
            _ClassManager = classManager;
        }
        //注册用户查询（手机、姓名、注册时间、登录时间、采购权限、行业）        
        public ActionResult List(int? page, int? limit, String mobile, String name,
            DateTime? min_register_date, DateTime? max_register_date,
            DateTime? min_login_date, DateTime? max_login_date, String industry_no, bool? is_purchaser, String area_no)
        {
            page = Math.Max(page ?? 0, 1);
            if (!limit.HasValue)
            {
                limit = 30;
            }

            var query = _UserManager.EnabledUsers;
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
                query = query.Where(d => d.is_purchaser == is_purchaser.Value);
            }
            var count = query.Count();
            var pageCount = (int)Math.Ceiling((float)count / (float)limit);
            int start = (page.Value - 1) * limit.Value;
            var orderQuery = query.OrderByDescending(d => d.register_date);
            var pageList = orderQuery.Skip(start).Take(limit.Value).ToArray();
            var items = pageList.Select(d => new ListItemViewModel { Item = d }).ToArray();

            //获取地区
            LoadUserAreaName(items);

            //获取行业
            LoadUserIndustryName(items);

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

        public ActionResult Details(long id)
        {
            var entity = _UserManager.AllUsers.FirstOrDefault(d => d.uid == id);
            return View(new UserViewModel { Item = entity });
        }

        [HttpPost]
        public ActionResult Update(long id, String name, String mobile, String email, String tel,
            String company, String area_no, String address, String industry_no, bool? is_purchaser, bool? neet_invoice,
            bool? enabled, String enabled_memo, EmployeesCountRanges? employees_count_type)
        {
            var userService = _UserManager.Get(id);
            userService.UpdateByAdmin(name: name, mobile: mobile, email: email, tel: tel, company: company, area_no: area_no,
                address: address, industry_no: industry_no, is_purchaser: is_purchaser, neet_invoice: neet_invoice,
                enabled: enabled, enabled_memo: enabled_memo, employees_count_type: employees_count_type);
            return SuccessJsonResult();
        }

        void LoadUserAreaName(IEnumerable<ListItemViewModel> items)
        {
            var areaNoes = items.Select(d => d.Item.area_no).Where(d => !String.IsNullOrWhiteSpace(d)).Distinct().ToArray();
            var areaDict = _AreaManager.EnabledAreas.Where(d => areaNoes.Contains(d.area_no)).ToDictionary(d => d.area_no, d => d.area_full_name);
            foreach (var item in items)
            {
                if (!String.IsNullOrWhiteSpace(item.Item.area_no) && areaDict.ContainsKey(item.Item.area_no))
                {
                    item.area = areaDict[item.Item.area_no];
                }
            }
        }

        void LoadUserIndustryName(IEnumerable<ListItemViewModel> items)
        {
            var classNoes = items.Select(d => d.Item.industry_no).Where(d => !String.IsNullOrWhiteSpace(d)).Distinct().ToArray();
            var industryDict = _ClassManager.EnabledClasses.Where(d => classNoes.Contains(d.no)).ToDictionary(d => d.no, d => d.name);
            foreach (var item in items)
            {
                if (!String.IsNullOrWhiteSpace(item.Item.industry_no) && industryDict.ContainsKey(item.Item.industry_no))
                {
                    item.industry = industryDict[item.Item.industry_no];
                }
            }
        }

    }
}

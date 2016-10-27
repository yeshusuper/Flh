using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flh.Web;
using Flh.Data;
using Flh.Business;
using Newtonsoft.Json;

namespace Flh.AdminSite.Controllers
{
    public class CommonDataController : BaseController
    {
        private readonly IClassesManager _ClassesManager;
        private readonly ITradeManager _TradeManager;
        private readonly IAreaManager _AreaManager;
        public CommonDataController(IClassesManager classesManager,ITradeManager tradeManager,IAreaManager areaManager)
        {
            _ClassesManager = classesManager;
            _TradeManager = tradeManager;
            _AreaManager = areaManager;
        }
        public ActionResult Area(string parent,int deep=3)
        {
            deep = Math.Max(1, deep);
            var maxLength = (parent??String.Empty).Trim().Length + 4 * deep;

            var areaInfos = _AreaManager.EnabledAreas.Where(a => a.area_no.Length <= maxLength);
            if (!String.IsNullOrWhiteSpace(parent))
                areaInfos = areaInfos.Where(a => a.area_no.StartsWith(parent) && a.area_no.Length > parent.Length);

            var result= areaInfos.OrderByDescending(a => a.order_by)
                 .ThenBy(a => a.area_no)
                 .Select(t => new Models.ClassTreeModel
                 {
                     ClassName = t.area_name,
                     ClassNo = t.area_no,
                 })
                .ToArray().Stratification().ToArray();
            return this.SuccessJsonResult(new { item = result });
        }
        public ActionResult ProductClasses(string parent, int deep = 3)
        {
            deep = Math.Max(1, deep);
            var maxLength = (parent ?? String.Empty).Length + 4 * deep;

            var areaInfos = _ClassesManager.EnabledClasses.Where(c => c.no.Length <= maxLength);
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
            return this.SuccessJsonResult(new { item = result });
        }
        public ActionResult Trade(string parent, int deep = 2)
        {
            deep = Math.Max(1, deep);
            var maxLength = (parent ?? String.Empty).Length + 4 * deep;

            var areaInfos = _ClassesManager.EnabledClasses.Where(c => c.no.Length <= maxLength);
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
            return this.SuccessJsonResult(new { item = result });
        }
    }
}

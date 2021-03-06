using Flh.Business;
using Flh.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.AdminSite.Controllers
{
    public class AreaController : BaseController
    {
        private readonly IAreaManager _AreaManager;

        public AreaController(IAreaManager areaManager)
        {
            _AreaManager = areaManager;
        }

        public ActionResult List(string pno, int? page)
        {
            if (String.IsNullOrWhiteSpace(pno) || !pno.StartsWith(FlhConfig.AREA_CLASS_PREFIX))
            {
                pno = FlhConfig.AREA_CLASS_PREFIX;
            }
            pno = pno.Trim();
            if (!page.HasValue || page.Value < 1)
                page = 1;
            var size = 30;
            var parent = _AreaManager.GetEnabled(pno);

            var classes = _AreaManager.GetChildren(pno);
            var parentClasses = _AreaManager.EnabledAreas.Where(c => pno.StartsWith(c.area_no)).OrderBy(c => c.area_no.Length).ToDictionary(c => c.area_no, c => c.area_name);

            return View(new Models.Classes.ListModel(){
                ParentNo = pno.Trim(),
                ParentFullName = Util.DisplayClassFullName(parent.area_full_name),
                ParentClasses=parentClasses,
                Items = new PageModel<Models.Classes.ListModel.Item>(classes
                            .OrderByDescending(n => n.order_by)
                            .ThenByDescending(n => n.created)
                            .Skip((page.Value - 1) * size)
                            .Take(size)
                            .Select(n => new Models.Classes.ListModel.Item{
                                Name = n.area_name,
                                No = n.area_no,
                                Order = n.order_by
                            }), page.Value, (int)Math.Ceiling((double)classes.Count()/(double)size))
            });
        }

        [HttpGet]
        public ActionResult BatchAdd(string pno)
        {
            var parent = _AreaManager.GetEnabled(pno);

            return View(new Models.Classes.BatchAddModel(6)
            {
                ParentNo = parent.area_no,
                ParentFullName = Util.DisplayClassFullName(parent.area_full_name)
            });
        }

        [HttpPost]
        public ActionResult BatchAdd(string pno, string model)
        {
            var items = JsonConvert.DeserializeObject<Models.Classes.BatchAddModel.EditModel[]>(model);
            _AreaManager.AddRange(this.CurrentUser.Uid, pno, items);
            return SuccessJsonResult();
        }
        public ActionResult Edit(string no)
        {
            var entity = _AreaManager.GetEnabled(no);
            ViewBag.No = no;
            return SuccessJsonResult(new Models.Classes.BatchAddModel.EditModel
            {
                EnName = entity.area_name_en,
                Name = entity.area_name,
                Order = entity.order_by
            });
        }
        [HttpPost]
        public ActionResult Edit(string no, string name, string name_en, int order)
        {
            _AreaManager.Edit(this.CurrentUser.Uid, no, new Models.Classes.BatchAddModel.EditModel { EnName = name_en, Name = name, Order = order });
            return SuccessJsonResult();
        }
        [HttpPost]
        public ActionResult Delete(string nos)
        {
            var _Nos = (nos ?? String.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Where(n => !String.IsNullOrWhiteSpace(n)).Distinct().ToArray();
            _AreaManager.Delete(this.CurrentUser.Uid, _Nos);
            return SuccessJsonResult();
        }
    }
}
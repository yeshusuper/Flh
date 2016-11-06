using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flh.Web;
using Flh.Business;
using Newtonsoft.Json;

namespace Flh.AdminSite.Controllers
{
    [FlhAuthorize]
    public class ClassesController : BaseController
    {
        private readonly IClassesManager _ClassesManager;

        public ClassesController(IClassesManager classesManager)
        {
            _ClassesManager = classesManager;
        }

        public ActionResult List(string pno, int? page)
        {
            if (String.IsNullOrWhiteSpace(pno) || !pno.StartsWith(FlhConfig.CLASSNO_CLASS_PREFIX))
            {
                pno = FlhConfig.CLASSNO_CLASS_PREFIX;
            }
            pno = pno.Trim();
            if (!page.HasValue || page.Value < 1)
                page = 1;
            var size = 30;
            var parent = _ClassesManager.GetEnabled(pno);

            var classes = _ClassesManager.GetChildren(pno);
            var parentClasses = _ClassesManager.EnabledClasses.Where(c => pno.StartsWith(c.no)).OrderBy(c => c.no.Length).ToDictionary(c => c.no, c => c.name);

            return View(new Models.Classes.ListModel(){
                ParentNo = pno.Trim(),
                ParentFullName = Util.DisplayClassFullName(parent.full_name),
                ParentClasses=parentClasses,
                Items = new PageModel<Models.Classes.ListModel.Item>(classes
                            .OrderByDescending(n => n.order_by)
                            .ThenByDescending(n => n.created)
                            .Skip((page.Value - 1) * size)
                            .Take(size)
                            .Select(n => new Models.Classes.ListModel.Item{
                                Name = n.name,
                                No = n.no,
                                Order = n.order_by,
                                IndexImage=n.indexImage,
                                 Introduce=n.introduce,
                                  ListImage=n.introduce,
                            }), page.Value, (int)Math.Ceiling((double)classes.Count()/(double)size))
            });
        }

        [HttpGet]
        public ActionResult BatchAdd(string pno)
        {
            var parent = _ClassesManager.GetEnabled(pno);

            return View(new Models.Classes.BatchAddModel(6)
            {
                ParentNo = parent.no,
                ParentFullName = Util.DisplayClassFullName(parent.full_name)
            });
        }

        [HttpPost]
        public ActionResult BatchAdd(string pno, string model)
        {
            var items = JsonConvert.DeserializeObject<Models.Classes.BatchAddModel.EditModel[]>(model);
            _ClassesManager.AddRange(this.CurrentUser.Uid, pno, items);
            return SuccessJsonResult();
        }
        [HttpGet]
        public ActionResult Edit(string no)
        {
            var entity = _ClassesManager.GetEnabled(no);
            ViewBag.No = no;
            return SuccessJsonResult(new Models.Classes.BatchAddModel.EditModel
            {
                EnName = entity.name_en,
                Name = entity.name,
                Order = entity.order_by,
                Introduce = entity.introduce,
                ListImage = entity.listImage,
                IndexImage = entity.indexImage,
            });
        }
        [HttpPost]
        public ActionResult Edit(string no, string name, string name_en, int order, string listImage, string indexImage, string introduce)
        {
            _ClassesManager.Edit(this.CurrentUser.Uid, no, new Models.Classes.BatchAddModel.EditModel { EnName = name_en, Name = name, Order = order, IndexImage=indexImage,ListImage=listImage,Introduce=introduce });
            return SuccessJsonResult();
        }
        [HttpPost]
        public ActionResult Delete(string nos)
        {
            var _Nos = (nos ?? String.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Where(n => !String.IsNullOrWhiteSpace(n)).Distinct().ToArray();
            _ClassesManager.Delete(this.CurrentUser.Uid, _Nos);
            return SuccessJsonResult();
        }
    }
}

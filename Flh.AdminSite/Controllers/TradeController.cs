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
    public class TradeController : BaseController
    {
        private readonly ITradeManager _TradeManager;

        public TradeController(ITradeManager tradeManager)
        {
            _TradeManager =tradeManager;
        }
        public ActionResult List(string pno, int? page)
        {
            if (String.IsNullOrWhiteSpace(pno) || !pno.StartsWith(FlhConfig.TRADE_CLASS_PREFIX))
            {
                pno = FlhConfig.TRADE_CLASS_PREFIX;
            }
            if (!page.HasValue || page.Value < 1)
                page = 1;
            var size = 30;
            var parent = _TradeManager.GetEnabled(pno);
            pno=pno.Trim();
            var classes = _TradeManager.GetChildren(pno);
            var parentClasses = _TradeManager.EnabledTrades.Where(c =>pno.StartsWith(c.no)).OrderBy(c => c.no.Length).ToDictionary(c => c.no, c => c.name);
            return View(new Models.Classes.ListModel(){
                ParentNo = pno,
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
                                Order = n.order_by
                            }), page.Value, (int)Math.Ceiling((double)classes.Count()/(double)size))
            });
        }

        [HttpGet]
        public ActionResult BatchAdd(string pno)
        {
            var parent = _TradeManager.GetEnabled(pno);

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
            _TradeManager.AddRange(this.CurrentUser.Uid, pno, items);
            return SuccessJsonResult();
        }
        public ActionResult Edit(string no)
        {
            var entity = _TradeManager.GetEnabled(no);
            ViewBag.No = no;
            return View(new Models.Classes.BatchAddModel.EditModel
            {
                EnName = entity.name_en,
                Name = entity.name,
                Order = entity.order_by
            });
        }
        [HttpPost]
        public ActionResult Edit(string no, string name, string name_en, int order)
        {
            _TradeManager.Edit(this.CurrentUser.Uid, no, new Models.Classes.BatchAddModel.EditModel { EnName = name_en, Name = name, Order = order });
            return RedirectToAction("list", new { pno = no.Substring(0, no.Length - 4) });
        }
        [HttpPost]
        public ActionResult Delete(string nos)
        {
            var _Nos = (nos ?? String.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Where(n => !String.IsNullOrWhiteSpace(n)).Distinct().ToArray();
            _TradeManager.Delete(this.CurrentUser.Uid, _Nos);
            return SuccessJsonResult();
        }
    }
}
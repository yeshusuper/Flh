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
        [HttpGet]
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

            var classes = _TradeManager.GetChildren(pno);

            return View(new Models.Classes.ListModel(){
                ParentNo = pno.Trim(),
                ParentFullName = Util.DisplayClassFullName(parent.full_name),
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
    }
}

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
    [FlhAuthorize]
    public class ProductController : BaseController
    {
        private readonly IProductManager _ProductManager;
        public ProductController(IProductManager productManager)
        {
            _ProductManager = productManager;
        }

        public ActionResult List()
        {
            return View();
        }

        /// <summary>
        /// 产品编辑页面
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pids"></param>
        /// <param name="classNo"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult BatchEdit(int? page, long[] pids, String classNo)
        {
            if (!page.HasValue || page.Value < 1)
            {
                page = 1;
            }
            ViewBag.Page = page;
            ViewBag.Pids = pids;
            ViewBag.ClassNo = classNo;
            return View();
        }

        [HttpGet]
        public ActionResult BatchEdit(int? page, long[] pids, String classNo)
        {
            if (!page.HasValue || page.Value < 1)
            {
                page = 1;
            }
            int size = 30;
            var products = _ProductManager.GetProductList(new ProductListArgs { Pids = pids, ClassNo = classNo });
            //return View(new Models.Product.BatchEditListModel
            //{
            //    Items = new PageModel<Flh.Business.Data.Product>(products
            //                .OrderByDescending(n => n.sortNo)
            //                .ThenByDescending(n => n.created)
            //                .Skip((page.Value - 1) * size)
            //                .Take(size)
            //                .Select(p => p).ToArray(),
            //                page.Value, (int)Math.Ceiling((double)products.Count() / (double)size))
            //});

            var items = products.OrderByDescending(n => n.sortNo)
                    .ThenByDescending(n => n.created)
                    .Skip((page.Value - 1) * size)
                    .Take(size)
                    .Select(p => p).ToArray();
            return Json(new { 
                Items=items,

            });
        }

        /// <summary>
        /// 保存编辑后的产品
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BatchEdit(string models)
        {
            var items = JsonConvert.DeserializeObject<Flh.Business.Data.Product[]>(models);
            _ProductManager.AddOrUpdateProducts(items);
            return SuccessJsonResult();
        }
    }
}

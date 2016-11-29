using Flh.Business;
using Flh.Business.Data;
using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.WebSite.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductManager _ProductManager;
        private readonly IClassesManager _ClassesManager;
        public ProductController(IProductManager productManager, IClassesManager classesManager)
        {
            _ProductManager = productManager;
            _ClassesManager = classesManager;
        }

        public ActionResult Index(string no, string kw, int? page)
        {
            if (!page.HasValue || page.Value < 1)
            {
                page = 1;
            }
            var size = 15;
            var count = 0;

            var classes = _ClassesManager.GetChildren(FlhConfig.CLASSNO_CLASS_PREFIX)
                .OrderByDescending(d => d.order_by)
                .Select(d => new Flh.WebSite.Models.Product.ListModel.ClassItem { Name = d.name, No = d.no }).ToArray();
            var products = _ProductManager.Search(new ProductSearchArgs
            {
                Keyword = kw,
                Limit = size,
                Start = (page.Value - 1) * size,
                ClassNo = String.IsNullOrWhiteSpace(no) ? String.Empty : no
            }, out count);
            return View(new Models.Product.ListModel()
            {
                No = (no ?? String.Empty).Trim(),
                Keyword = (kw ?? String.Empty).Trim(),
                ClassItems = classes,
                Items = new PageModel<Models.Product.ListModel.Item>(products
                            .Select(p => new Models.Product.ListModel.Item
                            {
                                Pid = p.pid,
                                Name = p.name,
                                No = p.classNo,
                                Order = p.sortNo,
                                Color = p.color,
                                Image = p.imagePath,
                                Material = p.material,
                                Size = p.size,
                                Technique = p.technique
                            }), page.Value, (int)Math.Ceiling((double)count / (double)size))
            });
        }

        public ActionResult Detail(long id)
        {
            var product = _ProductManager.EnabledProducts.First(d => d.pid == id);
            var items = GetRelationProducts(id,null,product.classNo);
            return View(new ProductDetailModel { Detail = product, Items = items });
        }

        public ActionResult RelationProducts(long excludePid,long excludeMinPid,string no)
        {
            var items = GetRelationProducts(excludePid, excludeMinPid, no);
            return SuccessJsonResult(items);
        }

        Product[] GetRelationProducts(long excludePid, long? excludeMinPid, string no)
        {
            var query = _ProductManager.EnabledProducts.Where(d => d.pid != excludePid && d.classNo.StartsWith(no));
            if(excludeMinPid.HasValue){
                query=query.Where(d=>d.pid>excludeMinPid.Value);
            }
            return query.Take(30).ToArray();
        }
    }
    public class ProductDetailModel
    {
        public ProductDetailModel()
        {
            Items = new Product[0];
        }
        public Product Detail { get; set; }
        public Product[] Items { get; set; }
    }
}

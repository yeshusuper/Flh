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
        private readonly IProductServiceFactory _ProductServiceFactory;
        public ProductController(IProductManager productManager, 
            IClassesManager classesManager,
            IProductServiceFactory productServiceFactory)
        {
            _ProductManager = productManager;
            _ClassesManager = classesManager;
            _ProductServiceFactory= productServiceFactory;
        }

        public ActionResult Index(string no, string kw, int? page,SortType? sort,decimal? priceMin,decimal? priceMax,string color)
        {
            if (!page.HasValue || page.Value < 1)
            {
                page = 1;
            }
            var size = 12;
            var count = 0;

            //获取一级分类
            var classes = _ClassesManager.GetChildren(FlhConfig.CLASSNO_CLASS_PREFIX)
                .OrderByDescending(d => d.order_by)
                .Select(d => new Flh.WebSite.Models.Product.ListModel.ClassItem { Name = d.name, No = d.no }).ToArray();
            var products = _ProductManager.Search(new ProductSearchArgs
            {
                Keyword = kw,
                ClassNo = String.IsNullOrWhiteSpace(no) ? String.Empty : no,
                PriceMin = priceMin,
                PriceMax = priceMax,
                Limit = size,
                Start = (page.Value - 1) * size,               
                Sort=sort
            }, out count);
            return View(new Models.Product.ListModel()
            {
                No = (no ?? String.Empty).Trim(),
                Keyword = (kw ?? String.Empty).Trim(),
                Color=color,
                ClassItems = classes,
                PriceMin=priceMin,
                PriceMax=priceMax,
                Sort=sort,
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
            var product = _ProductServiceFactory.CreateService(id);
            var items = GetRelationProducts(id,null,product.Entity.classNo);
            return View(new ProductDetailModel { Detail = product.Entity, Items = items });
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
        public IProduct Detail { get; set; }
        public Product[] Items { get; set; }
    }
}

using Flh.Business;
using Flh.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.AdminSite.Controllers
{
     //[FlhAuthorize]
    public class ProductController : BaseController
    {
       private readonly IProductManager _ProductManager;
       private readonly IClassesManager _ClassesManager;
       public ProductController(IProductManager productManager, IClassesManager classesManager)
       {
           _ProductManager = productManager;
           _ClassesManager = classesManager;
       }
        public ActionResult List(string no,string keyword,int? page)
        {
            if (!page.HasValue || page.Value < 1)
                page = 1;
            var size = 30;
            var Position = "产品列表";
            var count = 0;
            if (!String.IsNullOrWhiteSpace(no))
            {
                try
                {
                    var productClass = _ClassesManager.GetEnabled(no);
                    Position = Util.DisplayClassFullName(productClass.full_name);
                }
                catch
                {
                }
            }
            var products = _ProductManager.Search(new ProductSearchArgs 
            { 
                Keyword = keyword, 
                Limit = size, 
                Start = (page.Value - 1) * size, 
                ClassNo = no 
            }, out count);
            return View(new Models.Product.ListModel()
            {
                No = (no ?? String.Empty).Trim(),
                Position = Position,
                Keyword=(keyword??String.Empty).Trim(),
                Items = new PageModel<Models.Product.ListModel.Item>(products
                            .Select(p => new Models.Product.ListModel.Item
                            {
                                Pid=p.pid,
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
        public ActionResult Delete(string pids)
        {
            var _Pids = (pids??String.Empty).Split( new string[]{","},StringSplitOptions.RemoveEmptyEntries).Select(id=>long.Parse(id)).ToArray();
            _ProductManager.Delete(this.CurrentUser.Uid, _Pids);
            return SuccessJsonResult();
        }

        /// <summary>
        /// 产品编辑页面
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pids"></param>
        /// <param name="classNo"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult BatchEdit(int? page, String pids,String classNo)
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

        //[HttpPost]//todo:改成post请求
        public ActionResult BatchEditList(string pids)
        {
            pids = pids ?? String.Empty;
            var pidsArr = pids.Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries).Select(d=>d.To<long>()).ToArray();
            var products = _ProductManager.GetProductList(new ProductListArgs { Pids = pidsArr });
            var items = products.OrderByDescending(n => n.sortNo)
                    .ThenByDescending(n => n.created)
                    .Select(p => p).ToArray();
            return Json(items,JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存编辑后的产品
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveBatchEdit(string models)
        {
            var items = JsonConvert.DeserializeObject<Flh.Business.Data.Product[]>(models);
            _ProductManager.AddOrUpdateProducts(items);
            return SuccessJsonResult();
        }

        [HttpPost]
        public ActionResult UploadImages()
        {
            List<String> fileNames = new List<string>();
            foreach (string item in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[item] as HttpPostedFileBase;
                if (file == null && file.ContentLength == 0)
                    continue;
                //判断Upload文件夹是否存在，不存在就创建
                string path = Server.MapPath("..//Upload");
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                path = AppDomain.CurrentDomain.BaseDirectory + "Upload/";
                //获取上传的文件名
                string fileName = Path.GetFileName(file.FileName);
                //限制上传文件的类型
                if (Path.GetExtension(fileName) != ".png" 
                    && Path.GetExtension(fileName) != ".jpg"
                    && Path.GetExtension(fileName) != ".jpeg"
                    && Path.GetExtension(fileName) != ".gif")
                {
                    return JsonResult(ErrorCode.ServerError, "只能上传png/jpg/jpeg/gif格式的图片");
                }
                //上传
                file.SaveAs(Path.Combine(path, fileName));

                var imgUrl = "/Upload/" + fileName;
                fileNames.Add(imgUrl);
            }
            return SuccessJsonResult<List<String>>(fileNames);
        }
    }
}

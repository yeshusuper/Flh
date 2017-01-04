using Flh.Business;
using Flh.Business.Data;
using Flh.IO;
using Flh.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.AdminSite.Controllers
{
    [FlhAuthorize]
    public class ProductController : BaseController
    {
        private readonly IProductManager _ProductManager;
        private readonly IClassesManager _ClassesManager;
        private readonly IFileStore _FileStore;
        public ProductController(IProductManager productManager, IClassesManager classesManager, IFileStore fileStore)
        {
            _ProductManager = productManager;
            _ClassesManager = classesManager;
            _FileStore = fileStore;
        }
        public ActionResult List(string no, string keyword, int? page)
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
                ClassNo = String.IsNullOrWhiteSpace(no) ? "0001" : no
            }, out count);
            return View(new Models.Product.ListModel()
            {
                No = (no ?? String.Empty).Trim(),
                Position = Position,
                Keyword = (keyword ?? String.Empty).Trim(),
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
        public ActionResult Delete(string pids)
        {
            var _Pids = (pids ?? String.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(id => long.Parse(id)).ToArray();
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
        public ActionResult BatchEdit(int? page, String pids, String classNo)
        {
            ExceptionHelper.ThrowIfTrue(String.IsNullOrWhiteSpace(pids) && String.IsNullOrWhiteSpace(classNo), "pids, classNo", "pids, classNo不能同时为空");
            if (!page.HasValue || page.Value < 1)
            {
                page = 1;
            }
            ViewBag.Page = page;
            ViewBag.Pids = pids;
            ViewBag.ClassNo = classNo;
            return View();
        }

        [HttpPost]
        public ActionResult BatchEditList(string pids)
        {
            pids = pids ?? String.Empty;
            var pidsArr = pids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.To<long>()).ToArray();
            Product[] items;
            if (pidsArr.Any())
            {
                var products = _ProductManager.GetProductList(new ProductListArgs { Pids = pidsArr });
                items = products.OrderByDescending(n => n.sortNo)
                            .ThenByDescending(n => n.created)
                    .Select(p => p).ToArray();
                return Json(items, JsonRequestBehavior.AllowGet);
            }
            else
            {
                items = new Product[0];
            }
            return Json(items);
        }

        /// <summary>
        /// 保存编辑后的产品
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveBatchEdit(string models)
        {
            models = models.Replace("\"pid\":\"\"","'pid':0");
            try
            {
                var items = JsonConvert.DeserializeObject<Flh.Business.Data.Product[]>(models);
                foreach (var item in items)
                {
                    if (item.pid > 0)
                    {
                        item.updater = CurrentUser.Uid;
                    }
                    else
                    {
                        item.createUid = CurrentUser.Uid;
                    }
                }
                _ProductManager.AddOrUpdateProducts(items);
                return SuccessJsonResult();
            }
            catch (ArgumentException ex)
            {
                return JsonResult(ErrorCode.ArgError, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult UploadImages()
        {
            List<String> fileNames = new List<string>();
            foreach (var keyItem in Request.Form.Keys)
            {
                var key =  keyItem.ToString();
                if (key.StartsWith("upload_file"))
                {
                    var base64=Request.Form[key];
                    SaveBase64Image(base64, (stream, file) => {
                        var fid = FileId.FromFileName(file);
                        _FileStore.CreateTemp(fid, stream);
                        fileNames.Add(fid.ToTempId());
                    });
                }
            }            
            return SuccessJsonResult<List<String>>(fileNames);
        }

        /// <summary>
        /// 保存base64格式的图片
        /// </summary>
        /// <param name="base64"></param>
        /// <param name="saveImage"></param>
        private static void SaveBase64Image(string base64, Action<Stream, String> saveImage)
        {
            var parts = base64.Split(',');
            var formatePart = parts[0].ToLower();
            var dataPart = parts[1];
            System.Drawing.Imaging.ImageFormat format;
            var extend = "";
            if (formatePart.Contains("jpeg"))
            {
                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                extend = ".jpg";
            }
            else if (formatePart.Contains("bmp"))
            {
                format = System.Drawing.Imaging.ImageFormat.Bmp;
                extend = ".bmp";
            }
            else if (formatePart.Contains("gif"))
            {
                format = System.Drawing.Imaging.ImageFormat.Gif;
                extend = ".gif";
            }
            else if (formatePart.Contains("png"))
            {
                format = System.Drawing.Imaging.ImageFormat.Png;
                extend = ".png";
            }
            else
            {
                throw new Exception("图片格式不支持");
            }
            byte[] arr = Convert.FromBase64String(dataPart);
            using (MemoryStream ms = new MemoryStream(arr))
            {
                using (Bitmap bmp = new Bitmap(ms))
                {
                    using (MemoryStream img = new MemoryStream())
                    {
                        var fileName = Guid.NewGuid().ToString() + extend;
                        bmp.Save(img, format);
                        img.Position = 0;
                        saveImage(img, fileName);
                    }
                }
            }
        }
    }
}

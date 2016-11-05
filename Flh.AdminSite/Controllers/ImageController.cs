using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flh.IO;
using Flh.Web;
namespace Flh.AdminSite.Controllers
{
    public class ImageController : BaseController
    {
        private FileManager _FileManager;

        public ImageController(FileManager fileManager)
        {
            _FileManager = fileManager;
        }

        [HttpPost]
        public ActionResult Upload(System.Web.HttpPostedFileBase file)
        {
             var target = FileId.FromFileName(file.FileName);
             _FileManager.CreateOrUpdate(target, file.InputStream);
             return SuccessJsonResult<string>(target.Id);
        }
        [HttpPost]
        public ActionResult UploadTemp(System.Web.HttpPostedFileBase file)
        {
            var target = FileId.FromFileName(file.FileName);
            _FileManager.CreateTemp(target, file.InputStream);
            return SuccessJsonResult<string>(target.ToTempId());
        }
        [HttpGet]
        public ActionResult Test()
        {
            return View();
        }
    }
}

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

        public ImageController(IFileStore fileStore)
        {
            _FileManager = new FileManager(fileStore);
        }

        [HttpPost]
        public ActionResult Upload(System.Web.HttpPostedFileBase file)
        {
            try
            {
                var target = FileId.FromFileName(file.FileName);
                _FileManager.CreateOrUpdate(target, file.InputStream);
                return SuccessJsonResult<object>(new { id = target.Id });
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost]
        public ActionResult UploadTemp(System.Web.HttpPostedFileBase file)
        {
            var target = FileId.FromFileName(file.FileName);
            _FileManager.CreateTemp(target, file.InputStream);
            return SuccessJsonResult<object>(new { id = target.ToTempId() });
        }
        [HttpGet]
        [OutputCache(Duration = 86400, VaryByParam = "id")]
        public ActionResult Show(string id)
        {
            System.IO.Stream stream;
            id = id.Replace("\\\\", "\\");
            if (MimeTypeHelper.IsImage(id) && (stream = _FileManager.GetImage(id)) != null)
            {
                return File(stream, MimeTypeHelper.GetMimeType(id));
            }
            else
            {
                return HttpNotFound("file not exists");
            }
        }
        [HttpGet]
        public ActionResult Test()
        {
            return View();
        }
    }
}

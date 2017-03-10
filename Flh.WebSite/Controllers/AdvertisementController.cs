using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flh.Web;
using Flh.Data;
using Flh.Business;
using Newtonsoft.Json;
using Flh.Business.Advertisement;
using Flh.IO;

namespace Flh.WebSite.Controllers
{
    public class AdvertisementController : Controller
    {
        private readonly IAdvertisementManager _AdvertisementManager;
        private readonly IFileStore _FileStore;
        public AdvertisementController(IAdvertisementManager advertisementManager, IFileStore fileStore)
        {
            _AdvertisementManager = advertisementManager;
            _FileStore = fileStore;
        }
        public ActionResult List(string id)
        {
            ExceptionHelper.ThrowIfNullOrEmpty(id, "id","广告位置不能为空");
            id = id.Trim();
            var size = 20;
            var advertisements = _AdvertisementManager.Advertisements.Where(a => a.position == id);
            var model = advertisements.OrderByDescending(a => a.orderBy).ThenByDescending(a => a.updated)
                 .Take(size)
                 .ToArray()
                .Select(a => new Models.Advertisement.Advertisement
                {
                    aid = a.aid,
                    content = a.content,
                    image = a.image,
                    title = a.title,
                    url = a.url,
                    order = a.orderBy,
                }).ToArray();
            int index = 1;
            foreach (var m in model)
            {
                m.No = index;
                index++;
            }
            return View(model);
        }
        public ActionResult Detail(long  id)
        {
            var service = _AdvertisementManager.CreateService(id);
            service.Click();
            return Redirect(service.url);
        }

    }
}

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

namespace Flh.AdminSite.Controllers
{
    [FlhAuthorize]
    public class AdvertisementController : BaseController
    {
        private readonly IAdvertisementManager _AdvertisementManager;
        private readonly IFileStore _FileStore;
        public AdvertisementController(IAdvertisementManager advertisementManager, IFileStore fileStore)
        {
            _AdvertisementManager = advertisementManager;
            _FileStore = fileStore;
        }
        public ActionResult List(string position, string key, int? page)
        {
            if (!page.HasValue || page.Value < 1)
                page = 1;
            var size = 20;
            var count = 0;
            var advertisements = _AdvertisementManager.Advertisements;
            if (!String.IsNullOrWhiteSpace(position))
            {
                position = position.Trim();
                advertisements = advertisements.Where(a => a.position == position);
            }
            if (!String.IsNullOrWhiteSpace(key))
            {
                key = key.Trim();
                advertisements = advertisements.Where(a => a.title.Contains(key) || a.url.Contains(key));
            }
             count = advertisements.Count();
             var models = advertisements.OrderByDescending(a => a.orderBy).ThenByDescending(a => a.updated)
                  .Skip((page.Value - 1) * size)
                  .Take(size)
                  .ToArray()
                 .Select(a => new Models.Advertisement.Advertisement
                 {
                     aid = a.aid,
                     clickCount = a.clickCount,
                     content = a.content,
                     created = a.created,
                     creater = a.creater,
                     image = a.image,
                     isEnabled = a.isEnabled,
                     position = a.position,
                     title = a.title,
                     updated = a.updated,
                     updater = a.updater,
                     url = a.url,
                     positionName = _AdvertisementManager.GetPositionName(a.position),
                     order = a.orderBy,
                 }).ToArray();
            return View(new Models.Advertisement.AdvertisementList()
            {
                position=position??string.Empty,
                 key=key??string.Empty,
                Items = new PageModel<Models.Advertisement.Advertisement>(models, page.Value, (int)Math.Ceiling((double)count / (double)size))
            });
        }
        public ActionResult Delete(string aids)
        {
            var _Aids = (aids ?? String.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(id => long.Parse(id)).ToArray();
            _AdvertisementManager.Delete(_Aids,this.CurrentUser.Uid);
            return SuccessJsonResult();
        }
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            Models.Advertisement.AdvertisementEdit model = new Models.Advertisement.AdvertisementEdit();
            if (id.HasValue&&id.Value>0)
            {
                var adv = _AdvertisementManager.CreateService(id.Value);
                model = new Models.Advertisement.AdvertisementEdit
                {
                    aid = adv.aid,
                    content = adv.content,
                    image = adv.image,
                    order = adv.order,
                    position = adv.position,
                    title = adv.title,
                    url = adv.url,
                };
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(Models.Advertisement.AdvertisementEdit model)
        {
            ExceptionHelper.ThrowIfNull(model, "model", "参数不能为空");
            IAdvertisementService service = null;
            if (model.aid.HasValue&&model.aid.Value>0)
            {
                service = _AdvertisementManager.CreateService(model.aid.Value);
                service.Update(this.CurrentUser.Uid, model.title, model.content, model.url, model.image, model.position, model.order);
            }
            else
            {
                service = _AdvertisementManager.Add(model.title, model.content, model.url, this.CurrentUser.Uid, model.image, model.position, model.order);
            }
            return this.SuccessJsonResult();
        }
    }
}

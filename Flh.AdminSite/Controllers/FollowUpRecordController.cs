using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flh.Web;
using Flh.Data;
using Flh.Business;
using Newtonsoft.Json;
using Flh.Business.FollowUpRecord;
using Flh.IO;

namespace Flh.AdminSite.Controllers
{
    [FlhAuthorize]
    public class FollowUpRecordController : BaseController
    {
        private readonly IFollowUpRecordManager _FollowUpRecordManager;
        private readonly IUserManager _UserManager;
        public FollowUpRecordController(IFollowUpRecordManager followUpRecordManager, IUserManager userManager)
        {
            _FollowUpRecordManager = followUpRecordManager;
            _UserManager = userManager;
        }
        public ActionResult Delete(string rids)
        {
            var _Rids = (rids ?? String.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(id => long.Parse(id)).ToArray();
            _FollowUpRecordManager.Delete(this.CurrentUser.Uid, _Rids);
            return SuccessJsonResult();
        }
        [HttpGet]
        public ActionResult Edit(long uid)
        {
            ViewBag.Uid = uid;
            return View();
        }
        [HttpPost]
        public ActionResult Edit(Models.FollowUpRecord.FollowUpRecordEdit model)
        {
            ExceptionHelper.ThrowIfNull(model, "model", "参数不能为空");
            var service = _FollowUpRecordManager.Add(model.uid, this.CurrentUser.Uid, model.content, model.kind);
            var record = new Models.FollowUpRecord.FollowUpRecord
            {
                uid = service.Uid,
                administrator = service.Administrator,
                content = service.Content,
                created = service.Created,
                kind = service.Kind,
                rid = service.Rid,
                isEnabled = service.IsEnabled,
                administratorName = _UserManager.Get(service.Administrator).Name,
            };
            return this.SuccessJsonResult<Models.FollowUpRecord.FollowUpRecord>(record);
        }
        public ActionResult List(long uid, int? page)
        {
            if (!page.HasValue || page.Value < 1)
                page = 1;
            var size = 20;
            var count = 0;
            var records = _FollowUpRecordManager.GetFollowUpRecords(uid);
            count = records.Count();
            var models = records.OrderByDescending(a => a.created)
                 .Skip((page.Value - 1) * size)
                 .Take(size)
                 .ToArray();
            var admins = models.Select(r => r.administrator).ToArray();
            var users = _UserManager.GetUsersByIds(admins).ToArray();
            var result = models.Select(r => new Models.FollowUpRecord.FollowUpRecord
                  {
                      administrator = r.administrator,
                      administratorName = users.Where(u => u.Uid == r.administrator).Select(u => u.Name).FirstOrDefault(),
                      isEnabled = r.isEnabled,
                      rid = r.rid,
                      kind = r.kind,
                      created = r.created,
                      content = r.content,
                      uid = r.uid,
                  }).ToArray();
            return View(new PageModel<Models.FollowUpRecord.FollowUpRecord>(result, page.Value, (int)Math.Ceiling((double)count / (double)size))
            );
        }
    }
}

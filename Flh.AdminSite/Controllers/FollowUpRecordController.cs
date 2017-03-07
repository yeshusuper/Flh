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
        [HttpPost]
        public ActionResult Delete(long rid)
        {
            var service = _FollowUpRecordManager.CreateService(rid);
            service.Delete(this.CurrentUser.Uid);
            return SuccessJsonResult();
        }
        [HttpGet]
        public ActionResult Edit(long id)
        {
            ViewBag.Uid = id;
            return View();
        }
        [HttpPost]
        public ActionResult Edit(Models.FollowUpRecord.FollowUpRecordEdit model)
        {
            ExceptionHelper.ThrowIfNull(model, "model", "参数不能为空");
            var service = _FollowUpRecordManager.Add(model.uid, this.CurrentUser.Uid, model.content, model.kind);
            var users = _UserManager.GetUsers(new long[] { model.uid, this.CurrentUser.Uid });

            var record = new Models.FollowUpRecord.FollowUpRecord
            {
                uid = service.Uid,
                administrator = service.Administrator,
                content = service.Content,
                created = service.Created,
                kind = service.Kind,
                rid = service.Rid,
                isEnabled = service.IsEnabled,
                administratorName = users.Where(u => u.uid == service.Administrator).Select(u => u.name).FirstOrDefault(),
                uname = users.Where(u => u.uid == service.Uid).Select(u => u.name).FirstOrDefault(),
            };
            return this.SuccessJsonResult<Models.FollowUpRecord.FollowUpRecord>(record);
        }
        public ActionResult List(long id, int? page)
        {
            if (!page.HasValue || page.Value < 1)
                page = 1;
            var size = 2;
            var count = 0;
            ViewBag.Uid = id;
            var records = _FollowUpRecordManager.GetFollowUpRecords(id);
            count = records.Count();
            var models = records.OrderByDescending(a => a.created)
                 .Skip((page.Value - 1) * size)
                 .Take(size)
                 .ToArray();
            var admins = models.Select(r => r.administrator).ToArray();
            var users = _UserManager.GetUsersByIds(admins.Concat(new long[] { id }).ToArray()).ToArray();
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
                      uname = users.Where(u => u.Uid == r.uid).Select(u => u.Name).FirstOrDefault(),
                  });
            return View(new PageModel<Models.FollowUpRecord.FollowUpRecord>(result, page.Value, (int)Math.Ceiling((double)count / (double)size)));
        }
    }
}

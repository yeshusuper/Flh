using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Flh.Web
{

    public abstract class BaseController : Controller
    {
        public UserSessionEntry CurrentUser
        {
            get
            {
                return Session.GetCurrentUser();
            }
            set
            {
                Session.SetCurrentUser(value);
            }
        }

        public ActionResult JsonResult(ErrorCode code, string msg)
        {
            return Content(JsonConvert.SerializeObject(new JsonResultEntry
            {
                Code = code,
                Message = msg,
            }));
        }

        public ActionResult JsonResult(FlhException ex)
        {
            return JsonResult(ex.ErrorCode, ex.Message);
        }

        public ActionResult SuccessJsonResult()
        {
            return JsonResult(ErrorCode.None, String.Empty);
        }
        public ActionResult SuccessJsonResult<T>(T data)
        {
            return Content(JsonConvert.SerializeObject(new JsonResultEntry<T>
            {
                Code = ErrorCode.None,
                Data = data,
                Message = String.Empty,
            }));
        }
    }
}

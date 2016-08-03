using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Flh.Web
{

    public static class RequestBaseExtension
    {
        public static string GetInfo(this HttpRequestBase request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("referrer:" + request.UrlReferrer);
            sb.AppendLine("url:" + request.RawUrl);
            if (request.Form.Count > 0)
            {
                sb.AppendLine("Form:");
                foreach (var key in request.Form.AllKeys)
                {
                    sb.AppendLine(key + ":" + request.Form.Get(key));
                }
            }
            return sb.ToString();
        }

        public static string GetRequestValue(this HttpRequestBase request, string key)
        {
            ExceptionHelper.ThrowIfNull(request, "request");
            ExceptionHelper.ThrowIfNullOrEmpty(key, "key");

            var value = request.QueryString[key];
            if (string.IsNullOrEmpty(value))
                value = request.Form[key];
            return value;
        }
        public static string GetCurrentIP(this HttpRequestBase request)
        {
            string strIP = "";
            if (request == null) return strIP;
            try
            {
                if (request.ServerVariables["HTTP_VIA"] != null)
                {
                    strIP = !String.IsNullOrEmpty(request.ServerVariables["HTTP_X_FORWARDED_FOR"])
                          ? request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString()
                          : request.ServerVariables["REMOTE_ADDR"].ToString();
                }
                else
                {
                    strIP = request.ServerVariables["REMOTE_ADDR"].ToString();
                }
            }
            catch { }
            return strIP;
        }

        public static bool IsAjaxOrJSONPRequest(this HttpRequestBase request)
        {
            ExceptionHelper.ThrowIfNull(request, "request");
            return request.IsAjaxRequest() || IsJSONPRequest(request);
        }

        public static bool IsJSONPRequest(this HttpRequestBase request)
        {
            ExceptionHelper.ThrowIfNull(request, "request");
            return !String.IsNullOrWhiteSpace(GetRequestValue(request, Config.Current.JSONP_ACTION_KEY));
        }

        public static string GetBackUrl(this HttpRequestBase request)
        {
            ExceptionHelper.ThrowIfNull(request, "request");
            return HttpUtility.UrlDecode(GetRequestValue(request, Config.Current.BACK_URL_KEY));
        }

        public static string GetBackUrlOrReferrer(this HttpRequestBase request)
        {
            ExceptionHelper.ThrowIfNull(request, "request");
            var backurl = GetBackUrl(request);
            if (String.IsNullOrWhiteSpace(backurl) && request.UrlReferrer != null)
            {
                backurl = request.UrlReferrer.AbsoluteUri;
            }
            return backurl;
        }
    }
}

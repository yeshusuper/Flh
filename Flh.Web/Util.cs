using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Flh.Web
{
    public static class Util
    {
        public static string DisplayClassFullName(string fullName)
        {
            if (fullName == null) return String.Empty;
            return String.Join(">", fullName.Split(','));
        }

        public static SelectListItem[] GetEmployeesCountRangeSelectListItems(bool addEmptyItem, Flh.Business.Users.EmployeesCountRanges? selected)
        {
            var creater = new Func<string, Flh.Business.Users.EmployeesCountRanges, SelectListItem>((name, value) =>
            {
                return new SelectListItem { Text = name, Value = ((byte)value).ToString(), Selected = selected.HasValue && selected.Value == value };
            });

            var result = new List<SelectListItem>();
            if(addEmptyItem)
                result.Add(new SelectListItem { Text = "--公司从业人员数--" });

            result.Add(creater("1-9人", Business.Users.EmployeesCountRanges.R1To9));
            result.Add(creater("10-99人", Business.Users.EmployeesCountRanges.R10To99));
            result.Add(creater("100-499人", Business.Users.EmployeesCountRanges.R100To499));
            result.Add(creater("500-999人", Business.Users.EmployeesCountRanges.R500To999));
            result.Add(creater("1000人及以上", Business.Users.EmployeesCountRanges.R1000More));

            return result.ToArray();
        }
        public static string UrlCombine(string url, NameValueCollection querys, bool doUrlEncode = true)
        {
            ExceptionHelper.ThrowIfNull(url, "url");
            ExceptionHelper.ThrowIfNull(querys, "querys");

            var oldQuerys = GetUrlQuerys(url, doUrlEncode);
            foreach (var key in querys.AllKeys)
            {
                oldQuerys.Set(key, querys[key]);
            }
            var index = url.IndexOf("?");
            if (index > -1)
                url = url.Left(index + 1);
            else if (oldQuerys.Count > 0)
                url += '?';
            var qList = new List<string>();
            foreach (var key in oldQuerys.AllKeys)
            {
                qList.Add(HttpUtility.UrlEncode(key, Encoding.UTF8) + "=" + (doUrlEncode ? HttpUtility.UrlEncode(oldQuerys[key] ?? String.Empty, Encoding.UTF8) : oldQuerys[key] ?? String.Empty));
            }
            qList.Sort();
            return url + String.Join("&", qList);
        }

        public static NameValueCollection GetUrlQuerys(string url, bool doUrlDecode = true)
        {
            ExceptionHelper.ThrowIfNull(url, "url");
            var index = url.IndexOf("?");
            var query = String.Empty;
            if (index > -1)
            {
                if (url.Length > index + 1)
                    query = url.Substring(index + 1);
            }
            else
            {
                try
                {
                    var uri = new Uri(url);
                }
                catch
                {
                    query = url;
                }
            }
            var nvc = new NameValueCollection();
            if (!String.IsNullOrEmpty(query))
            {
                string[] querys = query.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in querys)
                {
                    int i = item.IndexOf("=");
                    if (i > 0)
                    {
                        if (i < item.Length - 1)
                            nvc[HttpUtility.UrlDecode(item.Left(i), Encoding.UTF8)] = doUrlDecode ? HttpUtility.UrlDecode(item.Right(item.Length - i - 1), Encoding.UTF8) : item.Right(item.Length - i - 1);
                        else
                            nvc[HttpUtility.UrlDecode(item.Left(i), Encoding.UTF8)] = String.Empty;
                    }
                }
            }
            return nvc;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Flh.Web
{
    public static class HtmlHelperExtension
    {
        public static IHtmlString JsonString(this HtmlHelper helper, object value)
        {
            return new MvcHtmlString(Newtonsoft.Json.JsonConvert.SerializeObject(value));
        }

        public static IHtmlString HtmlString(this HtmlHelper helper, string value)
        {
            return new MvcHtmlString(value);
        }

        public static IHtmlString Pager(this HtmlHelper helper, IPageModel model,
            int showCount = 10,
            bool isShowHomeAndLast = true,
            bool isShowPrevAndNext = true,
            bool isHomeAndLastInside = true,
            bool isShowInput = true,
            bool isUseDefaultSubmitScript = true,
            bool isWithQueryString = true,
            bool isUrlEncode = false,
            string interval = "<span class=\"{0}\">...</span>",
            string itemTemplate = "<a href=\"{1}\" class=\"{2}\">{0}</a>",
            string indexTemplate = "<b class=\"{1}\">{0}</b>",
            string parentTemplate = null,
            string inputTemplate = "<input class=\"{1}\" value=\"{0}\" />",
            string submitTemplate = "<input type=\"button\" class=\"{0}\" id=\"{1}\" value=\"确定\" />",
            string homeString = null,
            string lastString = null,
            string prevString = "上一页",
            string nextString = "下一页")
        {
            return new Pager(helper,
                model,
                showCount: showCount,
                isShowHomeAndLast: isShowHomeAndLast,
                isShowPrevAndNext: isShowPrevAndNext,
                isHomeAndLastInside: isHomeAndLastInside,
                isShowInput: isShowInput,
                isUseDefaultSubmitScript: isUseDefaultSubmitScript,
                isWithQueryString: isWithQueryString,
                isUrlEncode: isUrlEncode,
                interval: interval,
                itemTemplate: itemTemplate,
                indexTemplate: indexTemplate,
                parentTemplate: parentTemplate,
                inputTemplate: inputTemplate,
                submitTemplate: submitTemplate,
                homeString: homeString,
                lastString: lastString,
                prevString: prevString,
                nextString: nextString);
        }
    }
}

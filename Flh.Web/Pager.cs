using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Flh.Web
{
    public interface IPageModel
    {
        int PageIndex { get; }
        int PageCount { get; }
        int RecordCount { get; }
        PagerUrlProvider UrlProvider { get; }
    }

    public class PageModel<Model> : IEnumerable<Model>, IPageModel
    {
        private List<Model> list;

        public PageModel(IEnumerable<Model> models, int pageIndex, int pageCount)
        {
            PageCount = Math.Max(0, pageCount);
            PageIndex = Math.Min(pageCount, Math.Max(1, pageIndex));
            list = new List<Model>(models);
        }
        public PageModel(IEnumerable<Model> models, int pageIndex, int pageCount, int recordCount)
        {
            PageCount = Math.Max(0, pageCount);
            PageIndex = Math.Min(pageCount, Math.Max(1, pageIndex));
            RecordCount = Math.Max(0, recordCount);
            list = new List<Model>(models);
        }

        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int RecordCount { get; set; }
        public PagerUrlProvider UrlProvider { get; set; }

        public IEnumerator<Model> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// 分页控件
    /// </summary>
    public class Pager : IHtmlString
    {
        private static int Counter = 0;

        public readonly System.Web.Mvc.UrlHelper UrlHelper;

        private int thisCounter;

        private int Index;
        private int Count;
        private int ShowCount;
        private string ItemTemplate;
        private string IndexTemplate;
        private string ParentTemplate;
        private string InputTemplate;
        private string SubmitTemplate;
        private Func<object, string> Provider;
        private NameValueCollection QueryString;

        private bool IsShowInput;
        private bool IsShowHomeAndLast;
        private bool IsShowPrevAndNext;
        private bool IsHomeAndLastInside;
        private bool IsUseDefaultSubmitScript;
        private string Interval;
        private string HomeString;
        private string LastString;
        private string PrevString;
        private string NextString;

        /// <summary>
        /// 分页控件
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="index">当前页</param>
        /// <param name="count">总页数</param>
        /// <param name="provider">地址生成器</param>
        /// <param name="showCount">显示页码数量</param>
        /// <param name="isShowHomeAndLast">是否显示首页和末页</param>
        /// <param name="isShowPrevAndNext">是否显示上一页和下一页</param>
        /// <param name="isHomeAndLastInside">首页和末页是否显示在上一页和下一页的内侧</param>
        /// <param name="isShowInput">是否显示转跳输入框</param>
        /// <param name="isUseDefaultSubmitScript">是否使用默认转跳js</param>
        /// <param name="isWithQueryString">是否包含QueryString字符串</param>
        /// <param name="isUrlEncode">传递的参数是否需要编码</param>
        /// <param name="interval">中间的间隔内容模板；{0}：Class</param>
        /// <param name="itemTemplate">分页模板；{0}：页码，{1}：地址，{2}：Class</param>
        /// <param name="indexTemplate">当前页码模板；{0}：页码，{1}：Class</param>
        /// <param name="parentTemplate">父级模板；{0}：分页html代码</param>
        /// <param name="inputTemplate">输入框模板</param>
        /// <param name="submitTemplate">提交按钮模板</param>
        /// <param name="homeString">首页文字</param>
        /// <param name="lastString">末页文字</param>
        /// <param name="prevString">上一页文字</param>
        /// <param name="nextString">下一页文字</param>
        internal Pager(HtmlHelper helper, IPageModel model,
            int showCount,
            bool isShowHomeAndLast,
            bool isShowPrevAndNext,
            bool isHomeAndLastInside,
            bool isShowInput,
            bool isUseDefaultSubmitScript,
            bool isWithQueryString,
            bool isUrlEncode,
            string interval,
            string itemTemplate,
            string indexTemplate,
            string parentTemplate,
            string inputTemplate,
            string submitTemplate,
            string homeString,
            string lastString,
            string prevString,
            string nextString)
        {
            thisCounter = System.Threading.Interlocked.Increment(ref Counter);

            Index = Math.Max(model.PageIndex, 1);
            Count = Math.Max(0, model.PageCount);
            Index = Math.Min(Index, Count);
            ShowCount = Math.Min(Math.Max(3, showCount), Count);
            ItemTemplate = itemTemplate;
            IndexTemplate = indexTemplate;
            ParentTemplate = parentTemplate;
            InputTemplate = inputTemplate;
            SubmitTemplate = submitTemplate;
            if (isWithQueryString)
            {
                QueryString = Util.GetUrlQuerys(helper.ViewContext.HttpContext.Request.Url.Query, isUrlEncode);
            }
            else
            {
                QueryString = new NameValueCollection();
            }
            if (model.UrlProvider == null)
            {
                UrlHelper = new System.Web.Mvc.UrlHelper(helper.ViewContext.RequestContext);
                var actionName = UrlHelper.RequestContext.RouteData.Values["action"] as String;

                Provider = page =>
                {
                    QueryString["page"] = page.ToString();
                    return Util.UrlCombine(UrlHelper.Action(actionName), QueryString, isUrlEncode);
                };
            }
            else
            {
                Provider = page =>
                {
                    var values = new RouteValueDictionary(QueryString);
                    values["page"] = page;
                    return model.UrlProvider(page, values);
                };
            }

            IsShowInput = isShowInput;
            IsUseDefaultSubmitScript = isUseDefaultSubmitScript;
            IsShowHomeAndLast = isShowHomeAndLast;
            IsShowPrevAndNext = isShowPrevAndNext;
            IsHomeAndLastInside = isHomeAndLastInside;
            Interval = interval;
            HomeString = homeString ?? String.Empty;
            LastString = lastString;
            PrevString = prevString ?? String.Empty;
            NextString = nextString ?? String.Empty;
        }

        public string ToHtmlString()
        {
            if (Count > 1)
            {
                var sb = new StringBuilder();

                int part1Count, part2Count, _showCount, pStart, pEnd;
                int part1CountLimit = Index - 1, part2CountLimit = Count - Index;
                if (ShowCount < Count)
                {
                    _showCount = ShowCount - 1;
                    part1Count = (int)Math.Floor((double)_showCount / 2d);
                    part2Count = _showCount - part1Count;
                    if (part1Count > part1CountLimit)
                    {
                        part2Count += (part1Count - part1CountLimit);
                        part1Count = Math.Min(part1Count, part1CountLimit);
                        part2Count = Math.Min(part2Count, part2CountLimit);
                    }
                    if (part2Count > part2CountLimit)
                    {
                        part1Count += (part2Count - part2CountLimit);
                        part2Count = part2CountLimit;
                    }
                    pStart = Math.Max(1, Index - part1Count);
                }
                else
                {
                    _showCount = Count - 1;
                    pStart = 1;
                    part1Count = Index - 1;
                    part2Count = Count - Index;

                }
                if (IsHomeAndLastInside)
                {
                    if (Index != 1 && IsShowPrevAndNext)
                    {
                        sb.AppendFormat(ItemTemplate, PrevString, Provider(Index - 1), "prev item");
                    }
                    if (pStart != 1 && IsShowHomeAndLast)
                    {
                        sb.AppendFormat(ItemTemplate, String.IsNullOrEmpty(HomeString) ? "1" : HomeString, Provider(1), "home item");
                    }
                }
                else
                {
                    if (pStart != 1 && IsShowHomeAndLast)
                    {
                        sb.AppendFormat(ItemTemplate, String.IsNullOrEmpty(HomeString) ? "1" : HomeString, Provider(1), "home item");
                    }
                    if (Index != 1 && IsShowPrevAndNext)
                    {
                        sb.AppendFormat(ItemTemplate, PrevString, Provider(Index - 1), "prev item");
                    }
                }
                if (pStart > 2)
                {
                    sb.AppendFormat(Interval, "interval");
                }
                if (Index != 1)
                {
                    for (int i = 0, j = pStart; i < part1Count; i++)
                    {
                        sb.AppendFormat(ItemTemplate, j, Provider(j++), "item");
                    }
                }
                sb.AppendFormat(IndexTemplate, Index, "current");
                pEnd = Index;
                for (int i = 0, j = Index + 1; i < part2Count; i++)
                {
                    pEnd = j;
                    sb.AppendFormat(ItemTemplate, j, Provider(j++), "item");
                }
                if (IsHomeAndLastInside)
                {
                    if (pEnd < Count - 1)
                    {
                        sb.AppendFormat(Interval, "interval");
                    }
                    if (pEnd != Count && IsShowHomeAndLast)
                    {
                        sb.AppendFormat(ItemTemplate, String.IsNullOrEmpty(LastString) ? Count.ToString() : LastString, Provider(Count), "last item");
                    }
                    if (Index != Count && IsShowPrevAndNext)
                    {
                        sb.AppendFormat(ItemTemplate, NextString, Provider(Index + 1), "next item");
                    }
                }
                else
                {
                    if (Index != Count && IsShowPrevAndNext)
                    {
                        sb.AppendFormat(ItemTemplate, NextString, Provider(Index + 1), "next item");
                    }
                    if (pEnd != Count && IsShowHomeAndLast)
                    {
                        sb.AppendFormat(ItemTemplate, String.IsNullOrEmpty(LastString) ? Count.ToString() : LastString, Provider(Count), "last item");
                    }
                }

                if (Count > ShowCount)
                {
                    if (IsShowInput)
                    {
                        var id = "j-pager-submit" + thisCounter;
                        sb.AppendFormat(InputTemplate, Provider("#page#"), "input");
                        sb.AppendFormat(SubmitTemplate, "submit", id);
                        if (IsUseDefaultSubmitScript)
                        {
                            sb.AppendLine("<script type=\"text/javascript\">(function($){var submit = $('#" + id + "').click(function () {var page = parseInt(input.val()); page > 0 ? location.href = template.replace('#page#', page) : alert(\"页码格式错误\") }), input = submit.prev(), template = input.val(); input.val(''); })(jQuery)</script>");
                        }
                    }
                }

                return String.IsNullOrEmpty(ParentTemplate) ? sb.ToString() : String.Format(ParentTemplate, sb.ToString());
            }
            return String.Empty;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="queryString">QueryString  包含 page</param>
    /// <returns></returns>
    public delegate string PagerUrlProvider(object page, RouteValueDictionary queryString);
}

using Flh.Business;
using Flh.Business.Data;
using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.WebSite.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IClassesManager _ClassManager;
        public HomeController(IClassesManager classManager)
        {
            _ClassManager = classManager;
        }
        public ActionResult Index(String no="")
        {
            var allClasses = _ClassManager.EnabledClasses.ToArray();
            var root = new ClassItem(allClasses, new Classes { no = FlhConfig.CLASSNO_CLASS_PREFIX,name=String.Empty});
            //左上全部，不过可以先查一级和二级
            //右上按后台排序一级
            //下面左边是一级，右边是对应的二级，是一个整体的。都是按排序展示，而且全部展示
            //下面的二级如果放不下就按他的两行，一级是全部展示的

            var model = new IndexPageClassModel();
            model.TopLeftItems = root.Children.OrderByDescending(d => d.Sort).ThenByDescending(d => d.UpdateTime).Take(12).ToArray();
            model.TopRightItems = root.Children.OrderByDescending(d=>d.Sort).ThenByDescending(d=>d.UpdateTime).Take(8).ToArray();
            model.BottomLeftItems = root.Children;
            model.BottomRightItems = root.Children.OrderByDescending(d => d.Sort).ThenByDescending(d => d.UpdateTime).ToArray();
            model.CurrentClassNo = no;
            return View(model);
        }

        public class IndexPageClassModel
        {
            public ClassItem[] TopLeftItems { get; set; }
            public ClassItem[] TopRightItems { get; set; }
            public ClassItem[] BottomLeftItems { get; set; }
            public ClassItem[] BottomRightItems { get; set; }
            public String CurrentClassNo { get; set; }
        }

        public class ClassItem
        {
            private readonly Classes[] _Classes;
            private readonly Classes _Current;
            public ClassItem(Classes[] classes, Classes current)
            {
                _Classes = classes;
                _Current = current;
            }

            public String No { get { return _Current.no; } }
            public String Name { get { return _Current.name; } }
            public int Sort { get { return _Current.order_by; } }
            public String Img { get { return _Current.indexImage; } }
            public DateTime UpdateTime { get { return _Current.updated; } }

            private ClassItem[] _Children = null;
            public ClassItem[] Children
            {
                get
                {
                    if (_Children == null)
                    {
                        List<ClassItem> childList = new List<ClassItem>();
                        var childEntities = _Classes.Where(d => d.no.StartsWith(No) && d.no.Length == No.Length + 4);
                        foreach (var entity in childEntities)
                        {
                            var subOfChild = _Classes.Where(d => d.no.StartsWith(entity.no) && d.no.Length == entity.no.Length + 4).ToArray();
                            var child = new ClassItem(subOfChild, entity);
                            childList.Add(child);
                        }
                        _Children = childList.OrderByDescending(d => d.Sort).ThenByDescending(d => d.UpdateTime).ToArray();
                    }
                    return _Children;
                }
            }
        }
    }
}

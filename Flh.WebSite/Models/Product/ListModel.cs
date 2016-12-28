using Flh.Business;
using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Flh.WebSite.Models.Product
{
    public class ListModel
    {

        public ListModel()
        {
            ClassItems = new ClassItem[0];
        }
        public string No { get; set; }
        public string Keyword { get; set; }
        public string Color { get; set; }
        public Flh.Business.SortType? Sort { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public int? Page { get; set; }
        public PageModel<Item> Items { get; set; }
        public ClassItem[] ClassItems { get; set; }
        public String ClassOneNo { get; set; }
        public String ClassOneName { get; set; }
        public String ClassTwoName { get; set; }
        public string GetColorUrl(string color)
        {
            var dic = GetBaseUrlParameters();
            dic["color"] = color;
            var url = "/Product?" + String.Join("&", dic.Select(d => d.Key + "=" + d.Value));
            return url;
        }
        public string GetSortUrl(SortType sort)
        {
            var dic = GetBaseUrlParameters();
            dic["sort"] = sort;
            return "/Product?" + String.Join("&", dic.Select(d => d.Key + "=" + d.Value));
        }
        Dictionary<string, object> GetBaseUrlParameters()
        {
            return new Dictionary<string, object> {
                {"no",this.No},
                {"kw",this.Keyword},
                {"page",this.Page},
                {"sort",this.Sort},
                {"priceMin",this.PriceMin},
                {"priceMax",this.PriceMax},
                {"color",Color},
                };
        }
       
        public class ClassItem
        {
            public string Name { get; set; }
            public string No { get; set; }
        }
        public class Item
        {
            public long Pid { get; set; }
            public string No { get; set; }
            public string Name { get; set; }
            public string Image { get; set; }
            public string Size { get; set; }
            public int Order { get; set; }
            public string Color { get; set; }
            public string Material { get; set; }
            public string Technique { get; set; }
        }
    }
}
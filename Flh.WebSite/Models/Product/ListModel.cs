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
            ClassItems=new ClassItem[0];
        }
        public string No { get; set; }
        public string Keyword { get; set; }
         public decimal? PriceMin { get; set; }
         public decimal? PriceMax { get; set; }
        public PageModel<Item> Items { get; set; }
        public ClassItem[] ClassItems { get; set; }
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
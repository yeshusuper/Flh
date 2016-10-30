using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.AdminSite.Models.Product
{
   public class ListModel
    {
        public class Item
        {
            public long Pid { get; set; }
            public string No { get; set; }
            public string Name { get; set; }
            public int Order { get; set; }
            public string Image { get; set; }
            public string Color { get; set; }
            public string Material { get; set; }
            public string Size { get; set; }
            public string Technique { get; set; }
        }

        public string No { get; set; }
        public string Position { get; set; }
        public string Keyword { get; set; }
        public PageModel<Item> Items { get; set; }
    }
}

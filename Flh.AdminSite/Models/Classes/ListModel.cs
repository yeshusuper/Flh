using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Flh.AdminSite.Models.Classes
{
    public class ListModel
    {
        public class Item
        {
            public string No { get; set; }
            public string Name { get; set; }
            public int Order { get; set; }
        }

        public string ParentNo { get; set; }
        public string ParentFullName { get; set; }
        public PageModel<Item> Items { get; set; }
        
    }
}
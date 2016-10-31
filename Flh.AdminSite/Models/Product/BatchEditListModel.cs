using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Flh.AdminSite.Models.Product
{   
    public class BatchEditListModel
    {
        public PageModel<Flh.Business.Data.Product> Items { get; set; }

    }
}
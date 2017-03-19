using Flh.Business.Data;
using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Flh.AdminSite.Models.Users
{
    public class ListViewModel
    {
        public ListViewModel()
        {
        }
        public PageModel<ListItemViewModel> Items { get; set; }
        public int? page { get; set; }
        public int? limit { get; set; }
        public String mobile { get; set; }
        public String name { get; set; }
        public DateTime? min_register_date { get; set; }
        public DateTime? max_register_date { get; set; }
        public DateTime? min_login_date { get; set; }
        public DateTime? max_login_date { get; set; }
        public String industry_no { get; set; }
        public bool? is_purchaser { get; set; }
        public String area_no { get; set; }
    }

    public class ListItemViewModel
    {
        public IUser Item { get; set; }
        public String industry { get; set; }
        public String area { get; set; }
    }
}
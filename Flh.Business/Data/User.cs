using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public class User
    {
        public long uid { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string pwd { get; set; }
        public string name { get; set; }
        public string company { get; set; }
        public string area_no { get; set; }
        public string address { get; set; }
        public Flh.Business.User.EmployeesCountRanges employees_count_type { get; set; }
        public string industry_no { get; set; }
        public bool is_purchaser { get; set; }
        public bool neet_invoice { get; set; }
        public string tel { get; set; }
        public DateTime register_date { get; set; }
        public DateTime last_login_date { get; set; }
        public bool enabled { get; set; }
        public string enabled_memo { get; set; }

    }
}

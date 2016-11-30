using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.WebSite.Models.Account
{
    public class UserInfoModel : Flh.Business.Users.IUserInfo
    {
        public string Mobile { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Company { get; set; }

        public string AreaNo { get; set; }

        public string Address { get; set; }

        public Business.Users.EmployeesCountRanges EmployeesCountRange { get; set; }

        public string IndustryNo { get; set; }

        public bool IsPurchaser { get; set; }

        public bool NeetInvoice { get; set; }

        public string Tel { get; set; }
        public UserInfoModel()
        {

        }
        public UserInfoModel(Flh.Business.Data.User u)
        {
            if (u != null)
            {
                Mobile = u.mobile;
                Email = u.email;
                Name = u.name;
                Company = u.company;
                AreaNo = u.area_no;
                Address = u.address;
                EmployeesCountRange = u.employees_count_type;
                IndustryNo = u.industry_no;
                IsPurchaser = u.is_purchaser;
                NeetInvoice = u.neet_invoice;
                Tel = u.tel;
            }
        }
    }
}

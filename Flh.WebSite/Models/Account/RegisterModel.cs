using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Flh.WebSite.Models.Account
{
    public class RegisterModel : Flh.Business.Users.IRegisterInfo
    {
        public string Mobile { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public string Company { get; set; }

        public string AreaNo { get; set; }

        public string Address { get; set; }

        public Business.Users.EmployeesCountRanges EmployeesCountRange { get; set; }

        public string IndustryNo { get; set; }

        public bool IsPurchaser { get; set; }

        public bool NeetInvoice { get; set; }

        public string Tel { get; set; }


        public string Code { get; set; }
    }
}
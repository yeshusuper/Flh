using Flh.Business.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Flh.AdminSite.Models.Users
{
    public class UserViewModel
    {
        public IUser Item { get; set; }
        public String area { get; set; }
        public String industry { get; set; }
    }
}
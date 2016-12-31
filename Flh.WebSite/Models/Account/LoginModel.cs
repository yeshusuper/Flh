using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Flh.WebSite.Models.Account
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
    }
}
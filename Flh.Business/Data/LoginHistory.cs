using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public class LoginHistory
    {
        public long uid { get; set; }
        public DateTime login_date { get; set; }
        public string ip { get; set; }
    }
}

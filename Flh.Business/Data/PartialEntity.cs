using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public partial class Admin
    {
        public static Admin CreateNewInstance(long uid)
        {
            return new Admin { uid = uid, enabled = true, created = DateTime.Now };
        }
    }
}

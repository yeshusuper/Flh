using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Web
{
    [Serializable]
    public class UserSessionEntry
    {
        public long Uid { get; set; }
        public string Name { get; set; }
    }
}

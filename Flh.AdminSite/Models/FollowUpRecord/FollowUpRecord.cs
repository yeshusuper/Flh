using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.AdminSite.Models.FollowUpRecord
{
   public class FollowUpRecord
    {
        public long rid { get; set; }
        public string content { get; set; }
        public DateTime created { get; set; }
        public long administrator { get; set; }
        public string administratorName { get; set; }
        public long uid { get; set; }
        public string  uname { get; set; }
        public bool isEnabled { get; set; }
        public Flh.Business.FollowUpRecord.FollowUpRecordKinds kind { get; set; }
    }
}

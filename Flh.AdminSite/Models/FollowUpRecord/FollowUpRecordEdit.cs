using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.AdminSite.Models.FollowUpRecord
{
   public class FollowUpRecordEdit
    {
        public long rid { get; set; }
        public long uid { get; set; }
        public string content { get; set; }
        public Flh.Business.FollowUpRecord.FollowUpRecordKinds kind { get; set; }
    }
}

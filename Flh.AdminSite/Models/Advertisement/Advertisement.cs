using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.AdminSite.Models.Advertisement
{
   public class Advertisement
    {
        public long aid { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string url { get; set; }
        public int clickCount { get; set; }
        public DateTime created { get; set; }
        public long creater { get; set; }
        public long updater { get; set; }
        public System.DateTime updated { get; set; }
        public bool isEnabled { get; set; }
        public string image { get; set; }
        public string position { get; set; }
        public string positionName { get; set; }
        public int order { get; set; }
    }
}

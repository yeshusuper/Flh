using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.WebSite.Models.Advertisement
{
   public class Advertisement
    {
        public long aid { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string url { get; set; }
        public string image { get; set; }
        public string position { get; set; }
        public int order { get; set; }
    }
}

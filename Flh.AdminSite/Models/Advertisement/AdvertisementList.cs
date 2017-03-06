using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.AdminSite.Models.Advertisement
{
   public class AdvertisementList
    {
       public string position{get;set;}
       public string key{get;set;}
      public PageModel<Advertisement> Items { get; set; }
    }
}

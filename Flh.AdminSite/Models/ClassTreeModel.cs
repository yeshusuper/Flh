using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Flh.Data;

namespace Flh.AdminSite.Models
{
    public class ClassTreeModel : IClassModel<ClassTreeModel>
    {
        public string ClassNo { get; set; }
        public string ClassName { get; set; }
       public IEnumerable<ClassTreeModel>Subs { get; set; }
    }

}

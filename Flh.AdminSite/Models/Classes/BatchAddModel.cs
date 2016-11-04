using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Flh.AdminSite.Models.Classes
{
    public class BatchAddModel
    {
        public class EditModel : Flh.Business.IClassEditInfo
        {
            public string Name { get; set; }
            public string EnName { get; set; }
            public int Order { get; set; }
            public string Introduce{get;set;}
            public string IndexImage{get;set;}
            public string ListImage{get;set;}
        }

        public string ParentNo { get; set; }
        public string ParentFullName { get; set; }

        public EditModel[] EditModels { get; set; }

        public BatchAddModel(int emptyItemCount)
        {
            EditModels = new EditModel[emptyItemCount];
        }
    }
}
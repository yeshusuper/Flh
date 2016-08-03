using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Flh.Web
{
    public static class Util
    {
        public static SelectListItem[] GetEmployeesCountRangeSelectListItems(bool addEmptyItem, Flh.Business.Users.EmployeesCountRanges? selected)
        {
            var creater = new Func<string, Flh.Business.Users.EmployeesCountRanges, SelectListItem>((name, value) =>
            {
                return new SelectListItem { Text = name, Value = ((byte)value).ToString(), Selected = selected.HasValue && selected.Value == value };
            });

            var result = new List<SelectListItem>();
            if(addEmptyItem)
                result.Add(new SelectListItem { Text = "--公司从业人员数--" });

            result.Add(creater("1-9人", Business.Users.EmployeesCountRanges.R1To9));
            result.Add(creater("10-99人", Business.Users.EmployeesCountRanges.R10To99));
            result.Add(creater("100-499人", Business.Users.EmployeesCountRanges.R100To499));
            result.Add(creater("500-999人", Business.Users.EmployeesCountRanges.R500To999));
            result.Add(creater("1000人及以上", Business.Users.EmployeesCountRanges.R1000More));

            return result.ToArray();
        }
    }
}

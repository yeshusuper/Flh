using Flh.Business.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public class Utility
    {
        public static string ReplyFullName(string fullName, string oldName, string newName)
        {
            if (fullName == null) return String.Empty;
            var names = fullName.Split(',');
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == oldName.Trim())
                    names[i] = newName.Trim();
            }
            return String.Join(",", names);
        }
        public static String GetEmployeesCount(EmployeesCountRanges type)
        {
            switch (type)
            {
                case EmployeesCountRanges.R1To9:
                    return "1-9人";
                case EmployeesCountRanges.R10To99:
                    return "10-99人";
                case EmployeesCountRanges.R100To499:
                    return "100-499人";
                case EmployeesCountRanges.R500To999:
                    return "500-999人";
                case EmployeesCountRanges.R1000More:
                    return "1000人及以上";
                default:
                    return type.ToString();
            }
        }
        public static String GetBindStringValue(String value)
        {
            return String.IsNullOrWhiteSpace(value) ? "--" : value;
        }
    }
}

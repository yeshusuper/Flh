using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Users
{
    public enum EmployeesCountRanges : byte
    {
        R1To9 = 1,
        R10To99 = 2,
        R100To499 = 3,
        R500To999 = 4,
        R1000More = 5,
    }
}

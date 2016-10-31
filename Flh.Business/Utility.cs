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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh
{
    public static class StringHelper
    {
        public static string SafeTrim(this string str)
        {
            if (str == null)
                return null;
            return str.Trim();
        }
        public static string Left(this string text, int length)
        {
            ExceptionHelper.ThrowIfNull(text, "text");
            if (text.Length < length)
                return text;
            else if (length < 0)
            {
                if (text.Length > Math.Abs(length))
                    return text.Left(text.Length + length);
                else
                    return text.Left(-length - text.Length);
            }
            else
            {
                return text.Substring(0, length);
            }
        }
        public static string Right(this string text, int length)
        {
            ExceptionHelper.ThrowIfNull(text, "text");
            if (text.Length < length)
                return text;
            else if (length < 0)
            {
                if (text.Length > Math.Abs(length))
                    return text.Right(text.Length + length);
                else
                    return text.Right(-length - text.Length);
            }
            else
            {
                return text.Substring(text.Length - length, length);
            }
        }
    }
}

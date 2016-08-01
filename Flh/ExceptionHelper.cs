using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh
{
    public static class ExceptionHelper
    {
        public static void ThrowIfNull<T>(T param, string paramName, string message = null)
               where T : class
        {
            if (param == null)
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException("参数错误", paramName);
        }

        public static void ThrowIfNullOrEmpty(string param, string paramName, string message = null)
        {
            if (String.IsNullOrEmpty(param))
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException("参数错误", paramName);
        }

        public static void ThrowIfNullOrEmpty<T>(T[] param, string paramName, string message = null)
        {
            if (param == null || param.Length == 0)
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException("参数错误", paramName);
        }

        public static void ThrowIfNullOrEmpty<TKey, TValue>(IDictionary<TKey, TValue> param, string paramName, string message = null)
        {
            if (param == null || param.Count == 0)
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException("参数错误", paramName);
        }

        public static void ThrowIfNullOrEmpty<T>(IEnumerable<T> param, string paramName, string message = null)
        {
            if (param == null || param.Count() == 0)
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException("参数错误", paramName);
        }

        public static void ThrowIfNullOrEmptyIds(ref IEnumerable<long> param, string paramName, string message = null)
        {
            if (param == null || (param = param.Where(id => id > 0).Distinct()).Count() == 0)
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException("参数错误", paramName);
        }

        public static void ThrowIfNullOrEmptyIds(ref long[] param, string paramName, string message = null)
        {
            if (param == null || (param = param.Where(id => id > 0).Distinct().ToArray()).Length == 0)
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException("参数错误", paramName);
        }

        public static void ThrowIfNullOrWhiteSpace(string param, string paramName, string message = null)
        {
            if (String.IsNullOrWhiteSpace(param))
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException("参数错误", paramName);
        }

        public static void ThrowIfNotId(long id, string paramName, string message = null)
        {
            if (id <= 0)
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException(paramName + " 必须大于 0", paramName);
        }

        public static void ThrowIfNotId(long? id, string paramName, string message = null)
        {
            if (!id.HasValue || id.Value <= 0)
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException(paramName + " 必须大于 0", paramName);
        }

        public static void ThrowIfTrue(bool result, string paramName, string message = null)
        {
            if (result)
                if (!String.IsNullOrEmpty(message))
                    throw new ArgumentException(message, paramName);
                else
                    throw new ArgumentException("参数错误", paramName);
        }
    }
}

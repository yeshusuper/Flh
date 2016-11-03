using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh
{
    /// <summary>
    /// 类型转换扩展类
    /// </summary>
    public static class ConvertExtension
    {
        /// <summary>
        /// 值类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T To<T>(this IConvertible value)
            where T : IConvertible
        {
            if (value == null) return default(T);
            if (value is T) return (T)value;

            var type = typeof(T);
            if (type.IsEnum)
            {
                var valueType = value.GetType();
                if (valueType.IsEnum)
                {
                    var underlyingType = Enum.GetUnderlyingType(valueType);
                    var underlyingValue = Convert.ChangeType(value, underlyingType).ToString();
                    try
                    {
                        return (T)Enum.Parse(type, underlyingValue);
                    }
                    catch { }

                }
                try
                {
                    return (T)Enum.Parse(type, value.ToString());
                }
                catch { }
            }
            try
            {
                return (T)System.Convert.ChangeType(value, typeof(T));
            }
            catch { }
            return default(T);
        }

        /// <summary>
        /// 值类型转换为可空值类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T? ToNullable<T>(this IConvertible value)
            where T : struct, IConvertible
        {
            if (value == null)
                return null;
            else
                return new Nullable<T>(To<T>(value));
        }

        private static DateTime m_TimestampFixed = new DateTime(1970, 1, 1);
        /// <summary>
        /// 日期转换为时间戳
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static TimeSpan ToTimestamp(this DateTime date)
        {
            return date - m_TimestampFixed;
        }
        /// <summary>
        /// 时间戳转换为日期
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static DateTime ToDatetime(this TimeSpan span)
        {
            return m_TimestampFixed + span;
        }

        /// <summary>
        /// 将两个值进行置换，int i = 1, j = 2; i = i.Replacement(ref j); //j=1,i=2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源</param>
        /// <param name="dest">目标，将替换为源的值</param>
        /// <returns>目标值</returns>
        public static T Replacement<T>(this T source, ref T dest)
        {
            var r = dest;
            dest = source;
            return r;
        }
    }
}

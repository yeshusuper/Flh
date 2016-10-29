using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Data
{
    /// <summary>
    /// 类型实例的扩展方法
    /// </summary>
    public static class TreeExtension
    {
        /// <summary>
        /// 根据classno分层（即把构造分类树）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="classNoStep">一级</param>
        /// <returns></returns>
        public static IEnumerable<T> Stratification<T>(this IEnumerable<T> self, int classNoStep = 4)
            where T : IClassModel<T>
        {
            classNoStep = Math.Max(1, classNoStep);
            if (self != null)
            {
                var array = self.ToList();
                if (array.Count > 0)
                {
                    var result = new List<T>();
                    foreach (var item in array.Where(c => c.ClassNo == null || c.ClassNo.Length == 0))
                    {
                        result.Add(item);
                    }
                    result.ForEach(item => array.Remove(item));
                    var currentStep = array.Min(c => c.ClassNo.Length);
                    var maxNoLength = array.Max(c => c.ClassNo.Length);
                    var parents = self.Where(c => c.ClassNo.Length == currentStep).ToArray();
                    foreach (var item in parents)
                    {
                        item.Subs = GetSubClasss(array, item.ClassNo, item.ClassNo.Length, maxNoLength, classNoStep);
                        result.Add(item);
                    }
                    return result;
                }
            }
            return Enumerable.Empty<T>();
        }

        private static IEnumerable<T> GetSubClasss<T>(List<T> entities, string parentClassNo, int parentClassNoLength, int maxNoLength, int classNoStep)
            where T : IClassModel<T>
        {
            var noLength = parentClassNoLength + classNoStep;
            var result = new List<T>();
            do
            {
                var array = entities.Where(c => c.ClassNo.Length == noLength && c.ClassNo.StartsWith(parentClassNo)).ToArray();
                foreach (var item in array)
                {
                    entities.Remove(item);
                    item.Subs = GetSubClasss(entities, item.ClassNo, item.ClassNo.Length, maxNoLength, classNoStep).ToArray();
                    result.Add(item);
                }
                noLength += classNoStep;
            } while (noLength <= maxNoLength);
            return result;
        }

        /// <summary>
        /// 对树状接口的类型进行深度转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="self"></param>
        /// <param name="selecter">生成新类型的实例</param>
        /// <param name="subsExpression">新类型中的子元素集合属性</param>
        /// <param name="subsSelecter">将子元素集合转换和新类型中的子元素集合类型</param>
        /// <returns></returns>
        public static IEnumerable<E> Select<T, E, R>(this IEnumerable<T> self, Func<T, E> selecter,
            System.Linq.Expressions.Expression<Func<E, R>> subsExpression,
            Func<IEnumerable<E>, R> subsSelecter)
            where T : IClassModel<T>
            where R : IEnumerable<E>
        {
            var me = (System.Linq.Expressions.MemberExpression)subsExpression.Body;
            var memberInfo = me.Member as System.Reflection.PropertyInfo;
            return Select(self, selecter, memberInfo, subsSelecter);
        }

        /// <summary>
        /// 对树状接口的类型进行深度转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <param name="self"></param>
        /// <param name="selecter">生成新类型的实例</param>
        /// <param name="subsExpression">新类型中的子元素集合属性</param>
        /// <returns></returns>
        public static IEnumerable<E> Select<T, E>(this IEnumerable<T> self, Func<T, E> selecter,
            System.Linq.Expressions.Expression<Func<E, IEnumerable<E>>> subsExpression)
            where T : IClassModel<T>
        {
            return Select(self, selecter, subsExpression, r => r);
        }

        private static IEnumerable<E> Select<T, E, R>(IEnumerable<T> self, Func<T, E> selecter,
            System.Reflection.PropertyInfo propertyInfo,
            Func<IEnumerable<E>, R> subsSelecter)
            where T : IClassModel<T>
            where R : IEnumerable<E>
        {
            ExceptionHelper.ThrowIfTrue(propertyInfo == null, "子集必须为属性");
            var targetType = typeof(E);
            foreach (var item in self)
            {
                var target = selecter(item);
                propertyInfo.SetValue(target, subsSelecter(Select(item.Subs, selecter, propertyInfo, subsSelecter)), null);
                yield return target;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Data
{
    /// <summary>
    /// 类型实例的接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IClassModel<T>
        where T : IClassModel<T>
    {
        /// <summary>
        /// 类型编号
        /// </summary>
        string ClassNo { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        string ClassName { get; set; }
        /// <summary>
        /// 子分类
        /// </summary>
        IEnumerable<T> Subs { get; set; }
    }

    /// <summary>
    /// 类型实例
    /// </summary>
    [Serializable]
    public class ClassModel : IClassModel<ClassModel>
    {
        /// <summary>
        /// 类型名称
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// 类型编号
        /// </summary>
        public string ClassNo { get; set; }
        /// <summary>
        /// 子分类
        /// </summary>
        public IEnumerable<ClassModel> Subs { get; set; }
    }
}

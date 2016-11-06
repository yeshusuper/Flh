using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Data
{
    [Serializable]
    public class Range<T>
    {
        private bool _IsEquals;
        private bool _IsCanComparable;
        private T _Min;
        private T _Max;

        public T Min { get { return _Min; } set { _Min = value; Rebulit(); } }
        public T Max { get { return _Max; } set { _Max = value; Rebulit(); } }

        public Range(T v1, T v2)
        {
            Rebulit(v1, v2);
        }

        public bool IsEquals()
        {
            return _IsEquals;
        }
        public bool IsCanComparable()
        {
            return _IsCanComparable;
        }

        /// <summary>
        /// 如果Min或Max有改变，使用此方法重新排序
        /// </summary>
        private void Rebulit()
        {
            Rebulit(_Min, _Max);
        }

        private void Rebulit(T v1, T v2)
        {

            if (v1 == null || v2 == null)
            {
                _Min = v1;
                _Max = v2;
                _IsEquals = v1 == null && v2 == null;
                return;
            }

            _IsCanComparable = typeof(T).IsValueType || v1 is IComparable || v2 is IComparable;
            if (_IsCanComparable)
            {
                var i = Comparer<T>.Default.Compare(v1, v2);
                if (i > 0)
                {
                    _Min = v2;
                    _Max = v1;
                    _IsEquals = false;
                }
                else
                {
                    _Min = v1;
                    _Max = v2;
                    _IsEquals = v1.Equals(v2);
                }
            }
            else
            {
                _Min = v1;
                _Max = v2;
                _IsEquals = Object.Equals(v1, v2);
            }
        }
    }

    public static class Range
    {
        public static Range<T> Create<T>(T v1, T v2)
             where T : IComparable
        {
            return new Range<T>(v1, v2);
        }

        public static Range<Nullable<T>> CreateNullable<T>(T? v1, T? v2)
             where T : struct
        {
            return new Range<Nullable<T>>(v1, v2);
        }
    }
}

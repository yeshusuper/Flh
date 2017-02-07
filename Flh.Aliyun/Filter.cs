using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Aliyun
{
    public interface IFilter
    {
    }
    public static class Filter
    {
        private enum Kinds
        {
            And,
            Or
        }
        private enum FilterOperators
        {
            GT,
            GTE,
            EQ,
            LT,
            LTE,
            NE,
            IN,
            NOTIN,
        }

        private class FilterItem : IFilter
        {
            public string Left { get; private set; }
            public string Rigth { get; private set; }
            public FilterOperators Operator { get; private set; }

            public FilterItem(string left, string rigth, FilterOperators @operator)
            {
                ExceptionHelper.ThrowIfNullOrWhiteSpace(left, "left");
                ExceptionHelper.ThrowIfNullOrWhiteSpace(rigth, "rigth");
                Left = left.Trim();
                Rigth = rigth.Trim();
                Operator = @operator;
            }

            public override string ToString()
            {
                return String.Format(GetOperators(Operator), Left, Rigth);
            }
        }

        private class FilterCollection : IEnumerable<IFilter>, IFilter
        {
            private List<IFilter> _Filters = new List<IFilter>();
            public Kinds Kind { get; private set; }

            public FilterCollection(Kinds kind, IFilter[] filters)
            {
                _Filters.AddRange(filters);
                Kind = kind;
            }

            public IEnumerator<IFilter> GetEnumerator()
            {
                return _Filters.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public override string ToString()
            {
                var result = new List<string>();
                foreach (var item in this)
                {
                    result.Add("(" + item.ToString() + ")");
                }
                return String.Join(Kind.ToString().ToUpper(), result);
            }
        }

        private static string GetOperators(FilterOperators @operator)
        {
            switch (@operator)
            {
                case FilterOperators.GT: return "{0}>{1}";
                case FilterOperators.GTE: return "{0}>={1}";
                case FilterOperators.EQ: return "{0}={1}";
                case FilterOperators.LT: return "{0}<{1}";
                case FilterOperators.LTE: return "{0}<={1}";
                case FilterOperators.NE: return "{0}!={1}";
                case FilterOperators.IN: return "in({0},\"{1}\")";
                case FilterOperators.NOTIN: return "notin({0},\"{1}\")";
                default:
                    throw new Exception("not exists");
            }
        }

        public static IFilter GT(string left, string rigth)
        {
            return new FilterItem(left, rigth, FilterOperators.GT);
        }
        public static IFilter GTE(string left, string rigth)
        {
            return new FilterItem(left, rigth, FilterOperators.GTE);
        }
        public static IFilter EQ(string left, string rigth)
        {
            return new FilterItem(left, rigth, FilterOperators.EQ);
        }
        public static IFilter LT(string left, string rigth)
        {
            return new FilterItem(left, rigth, FilterOperators.LT);
        }
        public static IFilter LTE(string left, string rigth)
        {
            return new FilterItem(left, rigth, FilterOperators.LTE);
        }
        public static IFilter NE(string left, string rigth)
        {
            return new FilterItem(left, rigth, FilterOperators.NE);
        }

        public static IFilter IN(string fieldName, IEnumerable<int> values)
        {
            return CollectionHandle(FilterOperators.IN, fieldName, values);
        }

        public static IFilter IN(string fieldName, IEnumerable<double> values)
        {
            return CollectionHandle(FilterOperators.IN, fieldName, values);
        }

        private static IFilter CollectionHandle<T>(FilterOperators operators, string fieldName, IEnumerable<T> values) where T : struct
        {
            return new FilterItem(fieldName, String.Join("|", values.Select(v => v.ToString())), operators);
        }

        public static IFilter And(params IFilter[] filters)
        {
            return Handle(Kinds.And, filters);
        }

        public static IFilter Or(params IFilter[] filters)
        {
            return Handle(Kinds.Or, filters);
        }

        private static IFilter Handle(Kinds kind, params IFilter[] filters)
        {
            ExceptionHelper.ThrowIfNullOrEmpty(filters, "filters");
            if (filters.Length == 1)
                return filters[0];
            else
                return new FilterCollection(kind, filters);
        }
    }
}

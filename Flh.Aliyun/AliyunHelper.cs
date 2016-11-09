using HttpLease;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flh.Aliyun
{

    public static class AliyunHelper
    {
        private static readonly IAliyunHttp _Http;
        static String Host = "http://opensearch-cn-hangzhou.aliyuncs.com";

        static AliyunHelper()
        {
            var value = new System.Configuration.AppSettingsReader().GetValue("aliyunAccessKey", typeof(String)) as string;
            if (value != null)
            {
                var arr = value.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 1)
                {
                    string id = arr[0], secret = arr[1];
                    if (!String.IsNullOrWhiteSpace(id) && !String.IsNullOrWhiteSpace(secret))
                    {
                        AliyunAccessKey = new AliyunAccessKey(id.Trim(), secret.Trim());
                    }
                }
            }

            _Http = HttpLease.HttpLease.Get<IAliyunHttp>(config =>
            {
                config.Encoding = Encoding.UTF8;
                config.Host = Host;
            });           
        }

        public readonly static AliyunAccessKey AliyunAccessKey;

      


        public static void UpdateIndexDoc(IAliyunIndexer indexer, Dictionary<string, object>[] items)
        {
            var baseQuerys = new AliyunBaseQuerys();
            var action = "push";

            var itemsJson = Newtonsoft.Json.JsonConvert.SerializeObject(items.Select(i => new
            {
                cmd = "update",
                fields = i
            }));

            var otherQuerys = new Dictionary<string, string>
                {
                    { "action", action },
                    { "table_name", indexer.AliyunTableName },
                    { "items", itemsJson }
                };
            var signature = baseQuerys.GetSignature(AliyunAccessKey.AccessKeyId, AliyunAccessKey.AccessKeySecret, "post", otherQuerys);
            var response = _Http.IndexDoc(baseQuerys.Version,
                        AliyunAccessKey.AccessKeyId,
                        signature,
                        baseQuerys.Signature.Method,
                        baseQuerys.Signature.Version,
                        baseQuerys.SignatureNonce,
                        baseQuerys.Timestamp,
                        indexer.AliyunAppName,
                        action,
                        indexer.AliyunTableName,
                        itemsJson);
            if (!response.IsOk())
            {
                throw new Exception(String.Format("系统错误,response:{0};Query:{1}",
                    Newtonsoft.Json.JsonConvert.SerializeObject(response),
                    Newtonsoft.Json.JsonConvert.SerializeObject(otherQuerys)));
            }
        }

        public static void DeleteIndexDoc( IAliyunIndexer indexer, string idKey, string[] ids)
        {
            var baseQuerys = new AliyunBaseQuerys();
            var action = "push";
            var itemsJson = Newtonsoft.Json.JsonConvert.SerializeObject(ids.Select(i => new
            {
                cmd = "delete",
                fields = new Dictionary<string, string> { { idKey, i } }
            }));
            var otherQuerys = new Dictionary<string, string>
            {
                { "action", action },                
                { "table_name", indexer.AliyunTableName },
                { "items", itemsJson }
            };
            var signature = baseQuerys.GetSignature(AliyunAccessKey.AccessKeyId,AliyunAccessKey.AccessKeySecret, "post", otherQuerys);

            var response = _Http.IndexDoc(baseQuerys.Version,
                        AliyunAccessKey.AccessKeyId,
                        signature,
                        baseQuerys.Signature.Method,
                        baseQuerys.Signature.Version,
                        baseQuerys.SignatureNonce,
                        baseQuerys.Timestamp,
                        indexer.AliyunAppName,
                        action,
                        indexer.AliyunTableName,
                        itemsJson);
            if (!response.IsOk())
            {
                throw new Exception(String.Format("系统错误,response:{0};Query:{1}",
                    Newtonsoft.Json.JsonConvert.SerializeObject(response),
                    Newtonsoft.Json.JsonConvert.SerializeObject(otherQuerys)));
            }
        }

        public static SearchResponse.SearchResult Search( IAliyunIndexer indexer, QueryBuilder query, string qp, ISummary summary = null, string formula_name = null, string[] fields = null)
        {
            var baseQuerys = new AliyunBaseQuerys();
            var queryString = query.ToString();
            var summaryString = summary == null ? String.Empty : summary.ToString();
            formula_name = (formula_name ?? String.Empty).Trim();
            var fetch_fields = fields == null || fields.Length == 0 ? String.Empty : String.Join(";", fields);
            var otherQuerys = new Dictionary<string, string>
            {
                { "query", queryString },                
                { "index_name", indexer.AliyunAppName },
                { "fetch_fields", fetch_fields },
                { "qp", qp },
                { "disable", String.Empty },
                { "first_formula_name", String.Empty },
                { "formula_name", formula_name ?? String.Empty },
                { "summary", summaryString },
            };
            var sign = baseQuerys.GetSignature(AliyunAccessKey.AccessKeyId,AliyunAccessKey.AccessKeySecret, "get", otherQuerys);

            var response = _Http.Search(baseQuerys.Version, AliyunAccessKey.AccessKeyId, sign, baseQuerys.Signature.Method, baseQuerys.Signature.Version,
                baseQuerys.SignatureNonce, baseQuerys.Timestamp, queryString, indexer.AliyunAppName,
                fetch_fields, qp, String.Empty, String.Empty, formula_name, summaryString);
            if (!response.IsOk())
            {
                throw new Exception(String.Format("系统错误,response:{0};Query:{1}",
                    Newtonsoft.Json.JsonConvert.SerializeObject(response),
                    Newtonsoft.Json.JsonConvert.SerializeObject(otherQuerys)));
            }
            return response.Result;
        }

        //public static SuggestResponse.Suggestion[] Suggest(IAliyunIndexer indexer, string suggest_name, string keyword, int? limit = 10)
        //{
        //    var baseQuerys = new AliyunBaseQuerys();
        //    var hitString = limit.HasValue ? limit.Value.ToString() : String.Empty;
        //    suggest_name = suggest_name ?? String.Empty;
        //    var otherQuerys = new Dictionary<string, string>
        //    {
        //        { "query", keyword },                
        //        { "hit", hitString },
        //        { "suggest_name", suggest_name },
        //        { "index_name", indexer.AliyunAppName },
        //    };
        //    var sign = baseQuerys.GetSignature("get", otherQuerys);
        //    var response = _Http.Suggest(baseQuerys.Version, AliyunConfig.Current.AccessKeyId, sign, baseQuerys.Signature.Method, baseQuerys.Signature.Version,
        //        baseQuerys.SignatureNonce, baseQuerys.Timestamp, keyword, indexer.AliyunAppName, suggest_name, hitString);
        //    if (!response.IsOk())
        //    {
        //        Tgnet.Log.LoggerResolver.Current.Fail(Newtonsoft.Json.JsonConvert.SerializeObject(response.Errors, Newtonsoft.Json.Formatting.Indented));
        //        throw new Tgnet.Exception("系统错误");
        //    }
        //    return response.Suggestions;
        //}
    }

    public class ProductAliyunIndexer : IAliyunIndexer
    {

        public string AliyunTableName
        {
            get { return "product"; }
        }

        public string AliyunAppName
        {
            get { return "products"; }
        }
    }
    public interface IQuery
    {
        bool IsEmpty { get; }
    }
    public interface ISort
    {
    }
    public interface IFilter
    {
    }
    public interface IGroupBy
    {
    }
    public class QueryBuilder
    {
        public IConfig Config { get; set; }
        public IQuery Query { get; set; }
        public SourtItemCollection Sort { get; set; }
        public IFilter Filter { get; set; }
        public IGroupBy GroupBy { get; set; }

        public override string ToString()
        {
            var result = new List<string>();
            if (Config != null)
                result.Add("config=" + Config.ToString());
            if (Query != null)
                result.Add("query=" + Query.ToString());
            if (Sort != null)
                result.Add("sort=" + Sort.ToString());
            if (Filter != null)
                result.Add("filter=" + Filter.ToString());
            if (GroupBy != null)
                result.Add("aggregate=" + GroupBy.ToString());

            return String.Join("&&", result);
        }
    }
    public interface IAliyunIndexer
    {
        string AliyunTableName { get; }
        string AliyunAppName { get; }
    }
    public interface IAliyunBaseQuerys
    {
        string Version { get; }
        string Timestamp { get; }
        string SignatureNonce { get; }
        IAliyunSignature Signature { get; }
        string GetSignature(string accessKeyId, string accessKeySecret, string httpMethod, IDictionary<string, string> otherQuerys);
    }
    public interface IAliyunSignature
    {
        string Version { get; }
        string Method { get; }
        string GetResult(String accessKeySecret, string httpMethod, IDictionary<string, string> allQuerys);
    }
    public class AliyunBaseQuerys : IAliyunBaseQuerys
    {
        public string Version
        {
            get { return "v2"; }
        }

        private string _Timestamp = null;
        public string Timestamp
        {
            get
            {
                if (String.IsNullOrEmpty(_Timestamp))
                {
                    _Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
                return _Timestamp;
            }
        }

        private string _SignatureNonce = null;
        public string SignatureNonce
        {
            get
            {
                if (String.IsNullOrEmpty(_SignatureNonce))
                {
                    _SignatureNonce = Guid.NewGuid().ToString("N").ToString();
                }
                return _SignatureNonce;
            }
        }

        public IAliyunSignature Signature { get; private set; }

        public AliyunBaseQuerys()
        {
            Signature = new HMACSHA1Signature();
        }

        public string GetSignature(string accessKeyId, String accessKeySecret, string httpMethod, IDictionary<string, string> otherQuerys)
        {
            var allQuery = otherQuerys == null ? new Dictionary<string, string>() : new Dictionary<string, string>(otherQuerys);

            allQuery.Add("Version", this.Version);
            allQuery.Add("AccessKeyId", accessKeyId);
            allQuery.Add("SignatureMethod", Signature.Method);
            allQuery.Add("Timestamp", Timestamp);
            allQuery.Add("SignatureVersion", Signature.Version);
            allQuery.Add("SignatureNonce", SignatureNonce);

            return Signature.GetResult(accessKeySecret, httpMethod, allQuery);
        }
    }
    class HMACSHA1Signature : IAliyunSignature
    {
        private readonly Regex _Regex = new Regex("%[0-9a-z]{2}");

        public string Version
        {
            get { return "1.0"; }
        }

        public string Method
        {
            get { return "HMAC-SHA1"; }
        }

        public string GetResult(String accessKeySecret, string httpMethod, IDictionary<string, string> allQuerys)
        {
            var querys = new List<string>();
            var sb = new StringBuilder();
            sb.Append(System.Web.HttpUtility.UrlEncode("/").ToUpper());
            sb.Append("&");
            sb.Append(UrlEncode(String.Join("&", allQuerys.OrderBy(q => q.Key, StringComparer.Ordinal).Select(q => String.Format("{0}={1}", q.Key, UrlEncode(q.Value))))));

            var encodeQuerys = sb.ToString();

            sb = new StringBuilder();
            sb.Append(httpMethod.ToUpper());
            sb.Append("&");
            sb.Append(encodeQuerys);

            using (var sh1 = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(accessKeySecret + "&")))
            {
                var result = Convert.ToBase64String(sh1.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
                return result;
            }
        }

        private string UrlEncode(string querys)
        {
            querys = System.Web.HttpUtility.UrlEncode(querys, Encoding.UTF8);
            querys = _Regex.Replace(querys, match => match.Value.ToUpper());
            return querys
                    .Replace("+", "%20")
                    .Replace("*", "%2A")
                    .Replace("%7E", "~")
                    .Replace("%7e", "~")
                    .Replace("(", "%28")
                    .Replace(")", "%29")
                    .Replace("!", "%21");
        }
    }
    public interface IAliyunHttp
    {
        [HttpPost]
        [FormUrlEncoded(IsEncodeKey = false)]
        [Url("/index/doc/{app_name}")]
        AliyunResponse IndexDoc([Field]string Version, [Field]string AccessKeyId, [Field]string Signature, [Field]string SignatureMethod, [Field]string SignatureVersion, [Field]string SignatureNonce, [Field]string Timestamp, string app_name, [Field]string action, [Field]string table_name, [Field]string items);
        [HttpGet]
        [FormUrlEncoded(IsEncodeKey = false)]
        [Url("/search")]
        SearchResponse Search(string Version, string AccessKeyId, string Signature, string SignatureMethod, string SignatureVersion, string SignatureNonce, string Timestamp, string query, string index_name, string fetch_fields, string qp, string disable, string first_formula_name, string formula_name, string summary);
    }

  
    public class SearchResponse : AliyunResponse
    {
        public class SearchResult
        {
            public class Item
            {
                [JsonProperty(PropertyName = "fields")]
                public Dictionary<string, string> Fields { get; set; }
            }

            public class GroupByResult
            {
                public class Item
                {
                    [JsonProperty(PropertyName = "value")]
                    public string Value { get; set; }
                    [JsonProperty(PropertyName = "max")]
                    public string Max { get; set; }
                    [JsonProperty(PropertyName = "min")]
                    public string Min { get; set; }
                    [JsonProperty(PropertyName = "count")]
                    public int Count { get; set; }
                }

                [JsonProperty(PropertyName = "key")]
                public string Key { get; set; }
                [JsonProperty(PropertyName = "items")]
                public Item[] Items { get; set; }
            }

            /// <summary>
            /// 查询耗时，秒
            /// </summary>
            [JsonProperty(PropertyName = "searchtime")]
            public double SearchTime { get; set; }
            /// <summary>
            /// 命中数
            /// </summary>
            [JsonProperty(PropertyName = "total")]
            public int Total { get; set; }
            /// <summary>
            /// 本次返回结果数
            /// </summary>
            [JsonProperty(PropertyName = "num")]
            public int Num { get; set; }
            /// <summary>
            /// 本次查询最大返回结果数
            /// </summary>
            [JsonProperty(PropertyName = "viewtotal")]
            public int ViewTotal { get; set; }
            [JsonProperty(PropertyName = "items")]
            public Dictionary<string, string>[] Items { get; set; }
            [JsonProperty(PropertyName = "facet")]
            public GroupByResult[] Facet { get; set; }

        }

        [JsonProperty(PropertyName = "result")]
        public SearchResult Result { get; set; }
    }

    public class AliyunResponse
    {
        public class Error
        {
            [JsonProperty(PropertyName = "code")]
            public int Code { get; set; }
            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }
        }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "request_id")]
        public string RequestId { get; set; }
        [JsonProperty(PropertyName = "errors")]
        public Error[] Errors { get; set; }

        public virtual bool IsOk()
        {
            return Status.ToUpper() == "OK";
        }
    }

    public interface ISummary
    {
    }

    public class SummaryItem : ISummary
    {
        public string FieldName { get; private set; }
        /// <summary>
        /// 标签名，默认:em
        /// </summary>
        public string Element { get; set; }
        /// <summary>
        /// 省略符号，默认:...
        /// </summary>
        public string Ellipsis { get; set; }
        /// <summary>
        /// 摘要片段个数，默认:1
        /// </summary>
        public int? Snipped { get; set; }
        /// <summary>
        /// 摘要长度，默认:1
        /// </summary>
        public int? Length { get; set; }
        /// <summary>
        /// 飘红的前缀，必须是完整的html标签，如<em>
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// 飘红的后缀，必须是完整的html标签，如</em>
        /// </summary>
        public string Postfix { get; set; }


        public SummaryItem(string fieldName)
        {
            FieldName = fieldName.Trim();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("summary_field:");
            sb.Append(FieldName);
            if (!String.IsNullOrWhiteSpace(Element))
            {
                sb.Append(",summary_element:");
                sb.Append(Element.Trim());
            }
            if (!String.IsNullOrWhiteSpace(Ellipsis))
            {
                sb.Append(",summary_ellipsis:");
                sb.Append(Ellipsis.Trim());
            }
            if (Snipped.HasValue)
            {
                sb.Append(",summary_snipped:");
                sb.Append(Snipped.Value.ToString());
            }
            if (Length.HasValue)
            {
                sb.Append(",summary_len:");
                sb.Append(Length.Value.ToString());
            }
            if (!String.IsNullOrWhiteSpace(Prefix) && !String.IsNullOrWhiteSpace(Postfix))
            {
                sb.Append(",summary_element_prefix:");
                sb.Append(Prefix.Trim());
                sb.Append(",summary_element_postfix:");
                sb.Append(Postfix.Trim());
            }

            return sb.ToString();
        }
    }

    public class Query
    {
        private enum Kinds
        {
            And,
            Or,
            AndNot,
            Rank
        }

        private class QueryCollection : IEnumerable<IQuery>, IQuery
        {
            private List<IQuery> _Querys = new List<IQuery>();
            public Kinds Kind { get; private set; }

            public QueryCollection(Kinds kind, IQuery[] querys)
            {
                Kind = kind;
                _Querys.AddRange(querys);
            }

            public IEnumerator<IQuery> GetEnumerator()
            {
                return _Querys.GetEnumerator();
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
                    if (item != None)
                    {
                        result.Add("(" + item.ToString() + ")");
                    }
                }
                return String.Join(" " + Kind.ToString().ToUpper() + " ", result);
            }

            public bool IsEmpty
            {
                get { return _Querys.Count == 0 || _Querys.All(q => q.IsEmpty); }
            }
        }

        private class NoneQuery : IQuery
        {
            public override string ToString()
            {
                return String.Empty;
            }

            public bool IsEmpty
            {
                get { return true; }
            }
        }

        public static IQuery None = new NoneQuery();

        public static IQuery And(params IQuery[] querys)
        {
            return Handle(Kinds.And, querys);
        }

        public static IQuery Or(params IQuery[] querys)
        {
            return Handle(Kinds.Or, querys);
        }

        public static IQuery AndNot(params IQuery[] querys)
        {
            return Handle(Kinds.AndNot, querys);
        }

        public static IQuery Rank(params IQuery[] querys)
        {
            return Handle(Kinds.Rank, querys);
        }

        private static IQuery Handle(Kinds kind, params IQuery[] querys)
        {
            if (querys.Length == 1)
                return querys[0];
            else
                return new QueryCollection(kind, querys);
        }
    }
    public class Config : IConfig
    {
        private int _Start = 0;
        private int _Hit = 20;

        public int Start { get { return _Start; } set { _Start = Math.Max(0, Math.Min(5000, value)); } }

        public int Hit { get { return _Hit; } set { _Hit = Math.Max(0, Math.Min(500, value)); } }

        public Formats Format { get; set; }

        public int? RerankSize { get; set; }

        public Config()
        {
            Format = Formats.Json;
        }

        public override string ToString()
        {
            var start = Start;
            var hit = Hit;
            if (start + hit > 5000)
            {
                start = 5000 - hit;
            }

            var result = String.Format("start:{0},hit:{1},format:{2}", start.ToString(), hit.ToString(), Format.ToString().ToLower());
            if (RerankSize.HasValue && RerankSize.Value >= 0 && RerankSize.Value <= 2000)
                return String.Format("{0},rerank_size:{1}", result, RerankSize.Value.ToString());
            else
                return result;
        }
    }
    public enum Formats
    {
        Xml,
        Json,
        FullJson
    }
    public class QueryItem : IQuery
    {
        public string Index { get; private set; }
        public QueryItemValue[] Values { get; private set; }
        public bool IsEmpty { get { return false; } }

        public QueryItem(string index, QueryItemValue value)
            : this(index, new[] { value })
        {
        }

        public QueryItem(string index, QueryItemValue[] values)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(index, "index");
            ExceptionHelper.ThrowIfNullOrEmpty(values, "values");
            Index = index.Trim();
            Values = values;
        }



        protected virtual string ValueTemplate { get { return "'{0}'"; } }
        protected virtual string ValueTemplateWithBoost { get { return "'{0}'^{1}"; } }

        public override string ToString()
        {
            var values = Values.Select(v =>
            {
                if (v.Boost.HasValue && v.Boost.Value >= 0 && v.Boost.Value <= 99)
                    return String.Format(ValueTemplateWithBoost, EscapeKeyword(v.Value), v.Boost.Value);
                else
                    return String.Format(ValueTemplate, EscapeKeyword(v.Value));
            });
            return String.Format("{0}:{1}", Index, String.Join("|", values));
        }

        protected virtual string EscapeKeyword(string keyword)
        {
            return keyword == null ? String.Empty : keyword.Replace("'", "\\'");
        }
    }

    public struct QueryItemValue
    {
        public string Value { get; private set; }
        public int? Boost { get; private set; }

        public QueryItemValue(string value, int? boost = null)
            : this()
        {
            Value = value;
            Boost = boost;
        }


        public static implicit operator QueryItemValue(string keyword)
        {
            return new QueryItemValue(keyword);
        }
    }
    public class KeywordQuery : QueryItem
    {
        public KeywordQuery(string index, QueryItemValue value)
            : base(index, value) { }

        public KeywordQuery(string index, QueryItemValue[] values)
            : base(index, values) { }

        protected override string ValueTemplate
        {
            get
            {
                return "\"{0}\"";
            }
        }

        protected override string ValueTemplateWithBoost
        {
            get
            {
                return "\"{0}\"^{1}";
            }
        }

        protected override string EscapeKeyword(string keyword)
        {
            return keyword == null ? String.Empty : keyword.Replace("\"", "\\\"");
        }
    }

    public class SourtItemCollection : ISort
    {
        public SourtItemCollection() { 
        }
        public SourtItemCollection(params ISort[] items)
        {
            _Items.AddRange(items);
        }
        private List<ISort> _Items = new List<ISort>();
        public void Add(ISort item)
        {
            _Items.Add(item);
        }
        public override string ToString()
        {
            return String.Join(";", _Items.Select(d => d.ToString()).ToArray());
        }
    }
    public class SortItem : ISort
    {
        public string FieldName { get; private set; }
        public SortKinds Kind { get; private set; }

        public SortItem(string filedName, SortKinds kind)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(filedName, "filedName");

            FieldName = filedName.Trim();
            Kind = kind;
        }

        public override string ToString()
        {
            return String.Format("{0}{1}", Kind == SortKinds.Desc ? "-" : "+", FieldName);
        }
    }
    public enum SortKinds
    {
        Asc,
        Desc
    }

    public interface IAliyunEntityAdapter
    {
        void FillWithAliyunEntity(Dictionary<string, string> dic);
    }
    public class FieldHelper
    {
        public static Data.Range<long> GetAreaNoLongRange(string areaNo)
        {
            if (String.IsNullOrWhiteSpace(areaNo)) return new Data.Range<long>(0, 0);
            areaNo = areaNo.Trim();
            if (areaNo.Length >= 16)
            {
                var val = areaNo.Left(16).To<long>();
                return new Data.Range<long>(val, val);
            }
            else
            {
                var maxLength = areaNo.Length;
                var max = (areaNo.To<long>() + 1).ToString().PadLeft(maxLength, '0').PadRight(16, '0').To<long>();
                var min = areaNo.PadRight(16, '0').To<long>();
                return new Data.Range<long>(min, max);
            }
        }

        public static string AreaNoToLong(string areaNo)
        {
            if (String.IsNullOrWhiteSpace(areaNo)) return "0";
            areaNo = areaNo.Trim();
            if (areaNo.Length > 16)
            {
                areaNo = areaNo.Left(16);
            }
            else if (areaNo.Length < 16)
            {
                areaNo = areaNo.PadRight(16, '0');
            }
            return areaNo.To<long>().ToString();
        }

        public static string LongitudeOrLatitudeToString(double? value)
        {
            return value.HasValue ? value.ToString() : String.Empty;
        }

        public static double? ToLongitudeOrLatitude(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;
            else
                return value.To<double>();
        }

        public static string[] ToString(long[] values)
        {
            return values == null ? new string[0] : values.Select(v => v.ToString()).ToArray();
        }

        public static string[] ToString(byte[] values)
        {
            return values == null ? new string[0] : values.Select(v => v.ToString()).ToArray();
        }

        public static string ToString(DateTime? date, DateTime? defaultDate = null)
        {
            return date.HasValue ? ToString(date.Value) : defaultDate.HasValue ? ToString(defaultDate.Value) : String.Empty;
        }

        public static string ToString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string ToString(bool value)
        {
            return value ? "1" : "0";
        }

        public static string ToString(bool? value)
        {
            return value.HasValue ? ToString(value.Value) : String.Empty;
        }

        public static bool? ToNullableBoolean(string value)
        {
            return String.IsNullOrWhiteSpace(value) ? new Nullable<bool>() : value.Trim() != "0";
        }

        public static int ToTimestamp(DateTime? date, DateTime? defaultDate = null)
        {
            return date.HasValue ? ToTimestamp(date.Value) : defaultDate.HasValue ? ToTimestamp(defaultDate.Value) : 0;
        }

        public static int ToTimestamp(DateTime date)
        {
            return (int)(date - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime? ToNullableDateTime(int timestamp)
        {
            if (timestamp > 0)
            {
                return new DateTime(1970, 1, 1).AddSeconds(timestamp);
            }
            else
            {
                return null;
            }
        }

        public static DateTime ToDateTime(int timestamp)
        {
            return ToNullableDateTime(timestamp) ?? DateTime.MinValue;
        }
    }
    public interface IConfig
    {
        int Start { get; set; }
        int Hit { get; set; }
        Formats Format { get; set; }
        /// <summary>
        /// 精排数
        /// </summary>
        int? RerankSize { get; set; }
    }
}

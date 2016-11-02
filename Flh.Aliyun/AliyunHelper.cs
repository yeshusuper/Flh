using HttpLease;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flh.Aliyun
{

    public static class AliyunHelper
    {
        private static readonly IAliyunHttp _Http;

        static AliyunHelper()
        {
            _Http = HttpLease.HttpLease.Get<IAliyunHttp>(config =>
            {
                config.Encoding = Encoding.UTF8;
                config.Host = AliyunConfig.Host;
            });
        }



        public static AliyunResponse UpdateIndexDoc(String accessKeyId, String accessKeySecret, IAliyunIndexer indexer, Dictionary<string, object>[] items)
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
            var signature = baseQuerys.GetSignature(accessKeyId, accessKeySecret, "post", otherQuerys);
            return _Http.IndexDoc(baseQuerys.Version,
                        accessKeyId,
                        signature,
                        baseQuerys.Signature.Method,
                        baseQuerys.Signature.Version,
                        baseQuerys.SignatureNonce,
                        baseQuerys.Timestamp,
                        indexer.AliyunAppName,
                        action,
                        indexer.AliyunTableName,
                        itemsJson);
        }

        public static AliyunResponse DeleteIndexDoc(String accessKeyId, String accessKeySecret, IAliyunIndexer indexer, string idKey, string[] ids)
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
            var signature = baseQuerys.GetSignature(accessKeyId, accessKeySecret, "post", otherQuerys);

            return _Http.IndexDoc(baseQuerys.Version,
                        accessKeyId,
                        signature,
                        baseQuerys.Signature.Method,
                        baseQuerys.Signature.Version,
                        baseQuerys.SignatureNonce,
                        baseQuerys.Timestamp,
                        indexer.AliyunAppName,
                        action,
                        indexer.AliyunTableName,
                        itemsJson);
        }

        public static SearchResponse.SearchResult Search(String accessKeyId, String accessKeySecret, IAliyunIndexer indexer, QueryBuilder query, string qp, ISummary summary = null, string formula_name = null, string[] fields = null)
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
            var sign = baseQuerys.GetSignature(accessKeyId, accessKeySecret, "get", otherQuerys);

            var response = _Http.Search(baseQuerys.Version, accessKeyId, sign, baseQuerys.Signature.Method, baseQuerys.Signature.Version,
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
            get { return "Flh"; }
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
        public ISort Sort { get; set; }
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

    public class AliyunConfig
    {
        public static String AccessKeyId = "LTAI631tzQg1whIy";
        public static String AccessKeySecret = "FI4KWXvfMYWhxsf8O0CIPgahEJJZW3";
        public static String Host = "http://opensearch-cn-hangzhou.aliyuncs.com";
    }
}

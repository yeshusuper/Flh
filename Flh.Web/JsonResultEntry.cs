using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Web
{
    public class JsonResultEntry
    {
        [JsonProperty(PropertyName = "code")]
        public ErrorCode Code { get; set; }
        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }
    }
    public class JsonResultEntry<T> : JsonResultEntry
    {
        [JsonProperty(PropertyName = "data")]
        public T Data { get; set; }
    }
}

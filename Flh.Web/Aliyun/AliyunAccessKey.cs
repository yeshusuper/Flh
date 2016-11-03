using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Web.Aliyun
{
    public class AliyunAccessKey
    {
        public string AccessKeyId { get; private set; }
        public string AccessKeySecret { get; private set; }

        public AliyunAccessKey(string id, string secret)
        {
            this.AccessKeyId = id;
            this.AccessKeySecret = secret;
        }
    }
}

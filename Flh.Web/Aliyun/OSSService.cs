using Aliyun.OSS;
using Flh.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Web.Aliyun
{
    public class OSSService : IFileStore
    {
        private const string endpoint = "oss-cn-shenzhen-internal.aliyuncs.com";

        private readonly string _BucketName;
        private readonly OssClient _OssClient;

        public OSSService(string bucketName)
        {
            _OssClient = new OssClient(endpoint, AliyunHelper.AliyunAccessKey.AccessKeyId, AliyunHelper.AliyunAccessKey.AccessKeySecret);
        }

        public void CreateTemp(FileId id, System.IO.Stream stream)
        {
            _OssClient.PutObject(_BucketName, id.ToTempId(), stream);
        }

        public void CreateOrUpdate(FileId id, System.IO.Stream stream)
        {
            _OssClient.PutObject(_BucketName, id.ToString(), stream);
        }

        public void CreateOrAppend(FileId id, System.IO.Stream stream)
        {
            _OssClient.AppendObject(new AppendObjectRequest(_BucketName, id.ToString()){ 
                Content = stream
            });
        }

        public void Copy(FileId sourceId, FileId destId)
        {
            _OssClient.CopyObject(new CopyObjectRequest(_BucketName, sourceId.ToString(), _BucketName, destId.ToString()));
        }

        public void Delete(FileId id)
        {
            _OssClient.DeleteObject(_BucketName, id.ToString());
        }

        public bool Exists(FileId id)
        {
            return _OssClient.DoesObjectExist(_BucketName, id.ToString());
        }
    }
}

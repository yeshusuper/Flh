using Aliyun.OSS;
using Aliyun.OSS.Common;
using Flh.IO;
using System;
using System.Collections.Generic;
using System.IO;
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
        public  Stream GetFile(FileId id)
        {
            try
            {
                var obj = _OssClient.GetObject(_BucketName, id.ToString());
                if (!obj.Metadata.UserMetadata.ContainsKey("Expires") || String.IsNullOrWhiteSpace(obj.Metadata.UserMetadata["Expires"]))
                    return obj.Content;

                var expirationTime = obj.Metadata.UserMetadata["Expires"].To<DateTime>();
                if (expirationTime > DateTime.Now)
                {
                    return obj.Content;
                }
                else
                {
                    Delete(id);
                    return null;
                }
            }
            catch (ServiceException ex)
            {
                if(ex.ErrorCode == "NoSuchKey")
                {
                    return null;
                }
                else
                {
                    throw new Exception("aliyun error:" + ex.ErrorCode + ";message:" + ex.ToString());
                }
            }
        }
        public Stream GetImage(string key)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(key, paramName: "key");

            string query;
            key = FormatKey(key, out query);

            string extension;
            var processors = ImageProcessor.ParseQuery(query, out extension);
            if (processors.Length > 0)
            {
                if (String.IsNullOrWhiteSpace(extension))
                    extension = Path.GetExtension(key);
                var formatQuery = String.Join("|", processors.Select(p => p.Arguments.ToQuery()));
                if (processors.Length > 1)
                    formatQuery = new Flh.Security.MD5().Encrypt(formatQuery);
                var currentkey = String.Format("temp/{0}@{1}", key, formatQuery);
                if (!currentkey.EndsWith(extension))
                    currentkey += extension;
                try
                {
                    var stream = GetFile(FileId.FromFileId(currentkey));
                    if (stream == null)
                    {
                        using (var source = GetFile(FileId.FromFileId(key)))
                        {
                            if (source == null) return null;
                            stream = ImageProcessor.Process(source, new ImageProcessorSetting(), processors);
                            _OssClient.PutObject(_BucketName, currentkey, stream, new ObjectMetadata());
                            stream.Position = 0;
                            return stream;
                        }
                    }
                    return stream;
                }
                catch (OssException ex)
                {
                    if (ex.ErrorCode == OssErrorCode.NoSuchKey)
                    {
                        using (var source = GetFile(FileId.FromFileId(key)))
                        {
                            var stream = ImageProcessor.Process(source, new ImageProcessorSetting(), processors);
                            _OssClient.PutObject(_BucketName, currentkey, stream, new ObjectMetadata());
                            stream.Position = 0;
                            return stream;
                        }
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            else
            {
                return GetFile(FileId.FromFileId( key));
            }
        }
        private string FormatKey(string fullKey, out string query)
        {
            var index = fullKey.IndexOf('@');
            var key = String.Empty;
            query = String.Empty;
            if (index != -1)
            {
                if (fullKey.Length > index)
                {
                    key = fullKey.Substring(0, index);
                    if (fullKey.Length > index + 1)
                        query = fullKey.Substring(index + 1);
                }
                else
                {
                    key = fullKey;
                }
            }
            else
            {
                key = fullKey;
            }
            return key;
        }
    }
}

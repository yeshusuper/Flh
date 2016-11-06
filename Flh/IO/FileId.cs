using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.IO
{
    public class FileId
    {
        public string Id { get; private set; }

        private FileId(string id)
        {
            this.Id = id;
        }

        public static FileId FromFileName(string filename)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(filename, "filename");
            var guid = Guid.NewGuid().ToString("N");
            var extension = Path.GetExtension(filename);
            if (!String.IsNullOrWhiteSpace(extension))
                guid += extension;
            return new FileId(guid);
        }

        public static FileId FromFileId(string fileId)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(fileId, "fileId");
            return new FileId(fileId);
        }

        public string ToTempId()
        {
            return Path.Combine("temp", Id);
        }
        public  bool IsTempId
        {
            get
            {
                return (Id ?? string.Empty).StartsWith("temp");
            }
        }
        public FileId ToStorageId()
        {
            if (IsTempId)
                return FileId.FromFileId(Path.GetFileName(Id));
            return this;
        }
        internal string ToPath(string root)
        {
            var arr = new string[4];
            arr[0] = Id.Substring(0, 1);
            arr[1] = Id.Substring(1, 1);
            arr[2] = Id.Substring(2, 1);
            arr[3] = Id;
            return Path.Combine(root, String.Join("/", arr));
        }

        internal string ToTempPath(string root)
        {
            return ToPath(Path.Combine(root, "temp"));
        }

        public override string ToString()
        {
            return Id;
        }
    }
}

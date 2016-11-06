using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.IO
{
    public interface IFileStore
    {
        void CreateTemp(FileId id, Stream stream);
        void CreateOrUpdate(FileId id, Stream stream);
        void CreateOrAppend(FileId id, Stream stream);
        void Copy(FileId sourceId, FileId destId);
        void Delete(FileId id);
        bool Exists(FileId id);
        Stream GetFile(FileId id);
        Stream GetImage(string key);
    }

    public class SystemFileStroe : IFileStore
    {
        private string _Root;

        public SystemFileStroe()
        {
            _Root = Path.Combine(Environment.CurrentDirectory, "_TestFile");
        }

        private string GetFilename(FileId id)
        {
            var filename = id.ToPath(_Root);
            return GetFilename(filename);
        }

        private string GetFilename(string filename)
        {
            var path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return filename;
        }

        public void CreateOrUpdate(FileId id, Stream stream)
        {
            using (var fs = File.Open(GetFilename(id), FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                stream.CopyTo(fs);
            }
        }

        public void CreateOrAppend(FileId id, Stream stream)
        {
            using (var fs = File.Open(GetFilename(id), FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                stream.CopyTo(fs);
            }
        }

        public void Copy(FileId sourceId, FileId destId)
        {
            File.Copy(GetFilename(sourceId), GetFilename(destId), true);
        }

        public void Delete(FileId id)
        {           
            File.Delete(GetFilename(id));
        }

        public bool Exists(FileId id)
        {
            return File.Exists(GetFilename(id));
        }

        public void CreateTemp(FileId id, Stream stream)
        {
            using (var fs = File.Open(GetFilename(id.ToTempPath(_Root)), FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                stream.CopyTo(fs);
            }
        }
        public Stream GetFile(FileId id)
        {
            return File.Open(GetFilename(id), FileMode.Open);
        }

        public Stream GetImage(string key)
        {
            throw new NotImplementedException();
        }
    }
}

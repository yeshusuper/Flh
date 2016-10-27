using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.IO
{
    public class FileManager
    {
        private readonly IFileStore _FileStore;

        public FileManager(IFileStore store)
        {
            _FileStore = store;
        }

        public void Copy(FileId sourceId, FileId destId)
        {
            ExceptionHelper.ThrowIfNull(sourceId, "sourceId");
            ExceptionHelper.ThrowIfNull(destId, "destId");
            _FileStore.Copy(sourceId, destId);
        }

        public void CreateTemp(FileId id, Stream stream)
        {
            ExceptionHelper.ThrowIfNull(id, "id");
            ExceptionHelper.ThrowIfNull(stream, "stream");
            _FileStore.CreateTemp(id, stream);
        }

        public void CreateOrUpdate(FileId id, Stream stream)
        {
            ExceptionHelper.ThrowIfNull(id, "id");
            ExceptionHelper.ThrowIfNull(stream, "stream");
            _FileStore.CreateOrUpdate(id, stream);
        }

        public void CreateOrAppend(FileId id, Stream stream)
        {
            ExceptionHelper.ThrowIfNull(id, "id");
            ExceptionHelper.ThrowIfNull(stream, "stream");
            _FileStore.CreateOrAppend(id, stream);
        }

        public void Delete(FileId id)
        {
            ExceptionHelper.ThrowIfNull(id, "id");
            _FileStore.Delete(id);
        }

        public bool Exists(FileId id)
        {
            ExceptionHelper.ThrowIfNull(id, "id");
            return _FileStore.Exists(id);
        }
    }
}

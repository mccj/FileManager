using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager.FileStorage
{
    public class StoreFileProvider : IFileProvider
    {
        private readonly IFileStore _store;
        public StoreFileProvider(IFileStore store)
        {
            _store = store;
        }
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new StoreFileDirectoryContents(_store, subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return new StoreFileInfo(_store, _store.GetDirectoryInfoAsync(subpath).ConfigureAwait(false).GetAwaiter().GetResult() ?? _store.GetFileInfoAsync(subpath).ConfigureAwait(false).GetAwaiter().GetResult());
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
    public class StoreFileDirectoryContents : IDirectoryContents
    {
        private readonly IFileStore _store;
        private readonly string _subpath;
        public StoreFileDirectoryContents(IFileStore store, string subpath)
        {
            _store = store;
            _subpath = subpath;
        }
        public bool Exists
        {
            get
            {
                var r = _store.GetDirectoryInfoAsync(_subpath).ConfigureAwait(false).GetAwaiter().GetResult();
                return r != null;
            }
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            var r = _store.GetDirectoryContentAsync(_subpath).ConfigureAwait(false).GetAwaiter().GetResult();
            return r.Select(f => new StoreFileInfo(_store, f)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public class StoreFileInfo : IFileInfo
    {
        private readonly IFileStoreEntry _fileStore;
        private IFileStore _store;

        internal StoreFileInfo(IFileStore store, IFileStoreEntry fileStore)
        {
            this._store = store;
            this._fileStore = fileStore;
        }
        public bool Exists => _fileStore != null;

        public long Length => _fileStore?.Length ?? -1;

        public string PhysicalPath => _fileStore?.Path;

        public string Name => _fileStore?.Name;

        public DateTimeOffset LastModified => _fileStore?.LastModifiedUtc ?? DateTimeOffset.MinValue;

        public bool IsDirectory => _fileStore?.IsDirectory == true;

        public Stream CreateReadStream()
        {
            return _store.GetFileStreamAsync(_fileStore).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}

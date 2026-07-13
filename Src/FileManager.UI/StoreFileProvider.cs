using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            return new StoreFileDirectoryContents(_store, NormalizeSubpath(subpath));
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            subpath = NormalizeSubpath(subpath);
            var entry = _store.GetDirectoryInfoAsync(subpath).ConfigureAwait(false).GetAwaiter().GetResult()
                ?? _store.GetFileInfoAsync(subpath).ConfigureAwait(false).GetAwaiter().GetResult();
            return new StoreFileInfo(_store, entry);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        private static string NormalizeSubpath(string subpath)
        {
            if (string.IsNullOrWhiteSpace(subpath))
                return string.Empty;
            return VirtualPath.Normalize(subpath);
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
                var entry = _store.GetDirectoryInfoAsync(_subpath).ConfigureAwait(false).GetAwaiter().GetResult();
                return entry != null;
            }
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            var entries = _store.GetDirectoryContentAsync(_subpath).ConfigureAwait(false).GetAwaiter().GetResult();
            return entries.Select(f => new StoreFileInfo(_store, f)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class StoreFileInfo : IFileInfo
    {
        private readonly IFileStoreEntry _fileStore;
        private readonly IFileStore _store;

        internal StoreFileInfo(IFileStore store, IFileStoreEntry fileStore)
        {
            _store = store;
            _fileStore = fileStore;
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

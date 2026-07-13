using FileManager.FileStorage;
using System;
using System.Collections.Generic;

namespace FileManager.Storage.Composite
{
    public sealed class CompositeFileStoreBuilder
    {
        private readonly Dictionary<string, Func<System.IServiceProvider, IFileStore>> _drives =
            new Dictionary<string, Func<System.IServiceProvider, IFileStore>>(StringComparer.OrdinalIgnoreCase);

        public CompositeFileStoreBuilder AddDrive(string name, IFileStore store)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Drive name is required.", nameof(name));
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            _drives[name.Trim()] = _ => store;
            return this;
        }

        public CompositeFileStoreBuilder AddDrive(string name, Func<System.IServiceProvider, IFileStore> factory)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Drive name is required.", nameof(name));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _drives[name.Trim()] = factory;
            return this;
        }

        public CompositeFileStore Build(System.IServiceProvider serviceProvider)
        {
            if (_drives.Count == 0)
                throw new InvalidOperationException("At least one drive must be registered.");

            var stores = new Dictionary<string, IFileStore>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in _drives)
                stores[pair.Key] = pair.Value(serviceProvider);

            return new CompositeFileStore(stores);
        }
    }
}

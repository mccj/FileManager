using FileManager.Storage.Memcached;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using System;

namespace FileManager
{
    public static class MemcachedStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Memcached 实验性存储。</summary>
        public static FileServicesOptions AddMemcachedStore(this FileServicesOptions app, string host, int port = 11211)
        {
            return app.AddFileStore(_ => new MemcachedStore(host, port));
        }

        /// <summary>注册 Memcached 实验性存储。</summary>
        public static FileServicesOptions AddMemcachedStore(this FileServicesOptions app, IMemcachedClient client)
        {
            return app.AddFileStore(_ => new MemcachedStore(client));
        }

        /// <summary>注册 Memcached 实验性存储。</summary>
        public static FileServicesOptions AddMemcachedStore(this FileServicesOptions app, Func<System.IServiceProvider, MemcachedStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

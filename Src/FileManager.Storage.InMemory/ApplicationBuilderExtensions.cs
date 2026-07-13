using FileManager.Storage.InMemory;
using System;

namespace FileManager
{
    public static class InMemoryStoreApplicationBuilderExtensions
    {
        /// <summary>注册内存文件存储。</summary>
        public static FileServicesOptions AddInMemoryStore(this FileServicesOptions app, Func<System.IServiceProvider, InMemoryStore> factory = null)
        {
            return app.AddFileStore(sp => factory?.Invoke(sp) ?? new InMemoryStore());
        }
    }
}

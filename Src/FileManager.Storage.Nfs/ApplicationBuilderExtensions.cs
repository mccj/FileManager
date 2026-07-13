using FileManager.Storage.Nfs;
using System;

namespace FileManager
{
    public static class NfsStoreApplicationBuilderExtensions
    {
        /// <summary>注册 NFS 挂载目录存储。</summary>
        public static FileServicesOptions AddNfsStore(this FileServicesOptions app, string mountPath)
        {
            return app.AddFileStore(_ => new NfsStore(mountPath));
        }

        /// <summary>注册 NFS 挂载目录存储。</summary>
        public static FileServicesOptions AddNfsStore(this FileServicesOptions app, Func<System.IServiceProvider, NfsStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

using FileManager.Storage.Smb;
using System;

namespace FileManager
{
    public static class SmbStoreApplicationBuilderExtensions
    {
        /// <summary>注册 SMB 网络共享存储。</summary>
        public static FileServicesOptions AddSmbStore(this FileServicesOptions app, string uncPath)
        {
            return app.AddFileStore(_ => new SmbStore(uncPath));
        }

        /// <summary>注册 SMB 网络共享存储。</summary>
        public static FileServicesOptions AddSmbStore(this FileServicesOptions app, Func<System.IServiceProvider, SmbStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

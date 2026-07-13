using FileManager.Storage.Sftp;
using Renci.SshNet;
using System;

namespace FileManager
{
    public static class SftpStoreApplicationBuilderExtensions
    {
        /// <summary>注册 SFTP 远程文件存储。</summary>
        public static FileServicesOptions AddSftpStore(this FileServicesOptions app, string host, int port, string user, string password, string root = "/")
        {
            return app.AddFileStore(_ => new SftpStore(host, port, user, password, root));
        }

        /// <summary>注册 SFTP 远程文件存储。</summary>
        public static FileServicesOptions AddSftpStore(this FileServicesOptions app, Func<System.IServiceProvider, SftpStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }

        /// <summary>注册 SFTP 远程文件存储。</summary>
        public static FileServicesOptions AddSftpStore(this FileServicesOptions app, ConnectionInfo connectionInfo, string root = "/")
        {
            return app.AddFileStore(_ => new SftpStore(connectionInfo, root));
        }
    }
}

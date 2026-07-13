using Microsoft.Extensions.DependencyInjection;
namespace FileManager
{
    /// <summary>
    /// FTP 存储注册扩展。
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>注册 FTP 文件存储。</summary>
        public static FileServicesOptions AddFtpStore(this FileServicesOptions app, System.Func<System.IServiceProvider, FileStorage.Ftp.FtpStore> func)
        {
            app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
            {
                return func(sp);
            });

            return app;
        }
        /// <summary>注册 FTP 文件存储。</summary>
        public static FileServicesOptions AddFtpStore(this FileServicesOptions app, string host, int port, string user, string pass)
        {
            return AddFtpStore(app, sp => new FileStorage.Ftp.FtpStore(host, port, user, pass));
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
namespace FileManager
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        public static FileServicesOptions AddFtpStore(this FileServicesOptions app, System.Func<System.IServiceProvider, FileStorage.Ftp.FtpStore> func)
        {
            app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
            {
                return func(sp);
            });

            return app;
        }
        public static FileServicesOptions AddFtpStore(this FileServicesOptions app, string host, int port, string user, string pass)
        {
            return AddFtpStore(app, sp => new FileStorage.Ftp.FtpStore(host, port, user, pass));
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
namespace FileManager
{
    /// <summary>
    /// ZIP 压缩包存储注册扩展。
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>注册 ZIP 压缩包只读存储。</summary>
        public static FileServicesOptions AddCompressStore(this FileServicesOptions app, System.Func<System.IServiceProvider, Storage.Compress.CompressStore> func)
        {
            app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
            {
                return func(sp);
            });

            return app;
        }
        /// <summary>注册 ZIP 压缩包只读存储。</summary>
        public static FileServicesOptions AddCompressStore(this FileServicesOptions app, System.IO.Stream zipFileStream, string password=null)
        {
            return AddCompressStore(app, sp => new Storage.Compress.CompressStore(zipFileStream, password));
        }
    }
}

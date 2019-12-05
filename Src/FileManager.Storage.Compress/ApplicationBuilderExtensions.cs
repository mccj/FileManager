using Microsoft.Extensions.DependencyInjection;
namespace FileManager
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        public static FileServicesOptions AddCompressStore(this FileServicesOptions app, System.Func<System.IServiceProvider, Storage.Compress.CompressStore> func)
        {
            app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
            {
                return func(sp);
            });

            return app;
        }
        public static FileServicesOptions AddCompressStore(this FileServicesOptions app, System.IO.Stream zipFileStream, string password=null)
        {
            return AddCompressStore(app, sp => new Storage.Compress.CompressStore(zipFileStream, password));
        }
    }
}

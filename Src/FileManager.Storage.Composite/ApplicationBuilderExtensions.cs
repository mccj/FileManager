using FileManager.Storage.Composite;
using System;

namespace FileManager
{
    public static class CompositeStoreApplicationBuilderExtensions
    {
        /// <summary>注册多盘符组合存储。</summary>
        public static FileServicesOptions AddCompositeFileStore(this FileServicesOptions app, Action<CompositeFileStoreBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var builder = new CompositeFileStoreBuilder();
            configure(builder);
            return app.AddFileStore(sp => builder.Build(sp));
        }
    }
}

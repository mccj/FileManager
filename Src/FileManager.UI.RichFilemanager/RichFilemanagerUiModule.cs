using AspNetCoreDashboard.Abstractions;
using System;

namespace FileManager
{
    public sealed class RichFilemanagerUiModule : IUiModule
    {
        public RichFilemanagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder)
        {
            RichFilemanagerApplicationBuilderExtensions.ConfigureRichFilemanagerRoutes(builder);
        }
    }
}

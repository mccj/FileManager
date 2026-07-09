using AspNetCoreDashboard.Abstractions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace FileManager
{
    public sealed class FileExplorerUiModule : IUiModule
    {
        public FileExplorerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder)
        {
            FileExplorerApplicationBuilderExtensions.ConfigureFileExplorerRoutes(builder);
        }
    }
}

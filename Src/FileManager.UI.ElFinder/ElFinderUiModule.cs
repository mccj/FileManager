using AspNetCoreDashboard.Abstractions;
using System;

namespace FileManager
{
    public sealed class ElFinderUiModule : IUiModule
    {
        public ElFinderUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder)
        {
            ElFinderApplicationBuilderExtensions.ConfigureElFinderRoutes(builder);
        }
    }
}

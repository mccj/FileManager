using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FileManager
{
    public class FileManagerOptions
    {
        public PathString PathMatch { get; set; }
        public IApplicationBuilder ApplicationBuilder { get; set; }
        public string UiModuleKind { get; set; }
        /// <summary>
        /// Theme name when <see cref="UiModuleKind"/> is <c>restshell</c> or a legacy REST-shell alias (vue, react, …).
        /// </summary>
        public string RestShellTheme { get; set; }
        public/* protected internal*/ System.Action Fun { get; set; }
    }
}

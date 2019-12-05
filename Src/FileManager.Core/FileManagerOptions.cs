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
        public/* protected internal*/ System.Action Fun { get; set; }
    }
}

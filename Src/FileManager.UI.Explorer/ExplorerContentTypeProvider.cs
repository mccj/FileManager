using Microsoft.AspNetCore.StaticFiles;
using System.Collections.Generic;

namespace FileManager
{
    public static class ExplorerContentTypeProvider
    {
        private static readonly IReadOnlyDictionary<string, string> TextMappings = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            [".cs"] = "text/plain; charset=utf-8",
            [".csx"] = "text/plain; charset=utf-8",
            [".vb"] = "text/plain; charset=utf-8",
            [".fs"] = "text/plain; charset=utf-8",
            [".fsx"] = "text/plain; charset=utf-8",
            [".txt"] = "text/plain; charset=utf-8",
            [".log"] = "text/plain; charset=utf-8",
            [".md"] = "text/plain; charset=utf-8",
            [".markdown"] = "text/plain; charset=utf-8",
            [".json"] = "application/json; charset=utf-8",
            [".xml"] = "application/xml; charset=utf-8",
            [".xsl"] = "application/xml; charset=utf-8",
            [".xslt"] = "application/xml; charset=utf-8",
            [".config"] = "application/xml; charset=utf-8",
            [".csproj"] = "application/xml; charset=utf-8",
            [".vbproj"] = "application/xml; charset=utf-8",
            [".fsproj"] = "application/xml; charset=utf-8",
            [".props"] = "application/xml; charset=utf-8",
            [".targets"] = "application/xml; charset=utf-8",
            [".sln"] = "text/plain; charset=utf-8",
            [".slnx"] = "application/xml; charset=utf-8",
            [".js"] = "text/javascript; charset=utf-8",
            [".mjs"] = "text/javascript; charset=utf-8",
            [".ts"] = "text/plain; charset=utf-8",
            [".css"] = "text/css; charset=utf-8",
            [".scss"] = "text/plain; charset=utf-8",
            [".less"] = "text/plain; charset=utf-8",
            [".sql"] = "text/plain; charset=utf-8",
            [".yaml"] = "text/plain; charset=utf-8",
            [".yml"] = "text/plain; charset=utf-8",
            [".ini"] = "text/plain; charset=utf-8",
            [".cfg"] = "text/plain; charset=utf-8",
            [".env"] = "text/plain; charset=utf-8",
            [".cshtml"] = "text/html; charset=utf-8",
            [".razor"] = "text/plain; charset=utf-8",
            [".html"] = "text/html; charset=utf-8",
            [".htm"] = "text/html; charset=utf-8",
            [".svg"] = "image/svg+xml",
            [".sh"] = "text/plain; charset=utf-8",
            [".bat"] = "text/plain; charset=utf-8",
            [".cmd"] = "text/plain; charset=utf-8",
            [".ps1"] = "text/plain; charset=utf-8",
            [".py"] = "text/plain; charset=utf-8",
            [".java"] = "text/plain; charset=utf-8",
            [".go"] = "text/plain; charset=utf-8",
            [".rs"] = "text/plain; charset=utf-8",
            [".cpp"] = "text/plain; charset=utf-8",
            [".c"] = "text/plain; charset=utf-8",
            [".h"] = "text/plain; charset=utf-8",
            [".hpp"] = "text/plain; charset=utf-8",
        };

        private static readonly FileExtensionContentTypeProvider Provider = CreateProvider();

        public static string GetMimeMapping(string fileName)
        {
            var fileExt = System.IO.Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(fileExt) && Provider.Mappings.TryGetValue(fileExt, out var mime))
                return mime;

            return "application/octet-stream";
        }

        private static FileExtensionContentTypeProvider CreateProvider()
        {
            var provider = new FileExtensionContentTypeProvider();
            foreach (var mapping in TextMappings)
                provider.Mappings[mapping.Key] = mapping.Value;
            return provider;
        }
    }
}

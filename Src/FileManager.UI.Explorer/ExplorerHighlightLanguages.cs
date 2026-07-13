using System;
using System.Collections.Generic;
using System.IO;

namespace FileManager
{
    public static class ExplorerHighlightLanguages
    {
        private static readonly IReadOnlyDictionary<string, string> ExtensionToLanguage =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".cs"] = "csharp",
                [".csx"] = "csharp",
                [".vb"] = "vbnet",
                [".fs"] = "fsharp",
                [".fsx"] = "fsharp",
                [".json"] = "json",
                [".xml"] = "xml",
                [".xsl"] = "xml",
                [".xslt"] = "xml",
                [".config"] = "xml",
                [".csproj"] = "xml",
                [".vbproj"] = "xml",
                [".fsproj"] = "xml",
                [".props"] = "xml",
                [".targets"] = "xml",
                [".slnx"] = "xml",
                [".js"] = "javascript",
                [".mjs"] = "javascript",
                [".ts"] = "typescript",
                [".css"] = "css",
                [".scss"] = "css",
                [".less"] = "css",
                [".sql"] = "sql",
                [".yaml"] = "yaml",
                [".yml"] = "yaml",
                [".sh"] = "bash",
                [".py"] = "python",
                [".java"] = "java",
                [".go"] = "go",
                [".rs"] = "rust",
                [".cpp"] = "cpp",
                [".c"] = "cpp",
                [".h"] = "cpp",
                [".hpp"] = "cpp",
                [".md"] = "markdown",
                [".markdown"] = "markdown",
                [".html"] = "xml",
                [".htm"] = "xml",
                [".cshtml"] = "xml",
                [".razor"] = "xml",
            };

        public static string GetHighlightLanguage(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
                return "plaintext";

            return ExtensionToLanguage.TryGetValue(extension, out var language)
                ? language
                : "plaintext";
        }
    }
}

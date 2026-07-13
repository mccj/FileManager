using System;
using System.Text;

namespace FileManager.Sample.Owin
{
    internal static class OwinHubPage
    {
        public static string Render(string currentProvider)
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>");
            sb.Append("<title>FileManager OWIN Sample</title>");
            sb.Append("<style>body{font-family:Segoe UI,sans-serif;margin:2rem;max-width:960px}");
            sb.Append("h1{margin-bottom:.25rem}p{color:#444}.badge{display:inline-block;padding:.2rem .6rem;border-radius:4px;background:#0d6efd;color:#fff;font-size:.85rem}");
            sb.Append("ul{line-height:1.8}code{background:#f4f4f4;padding:.1rem .35rem;border-radius:3px}</style></head><body>");
            sb.Append("<h1>FileManager OWIN Sample</h1>");
            sb.Append("<p>.NET Framework 4.8 + OWIN · storage: <span class=\"badge\">").Append(Escape(currentProvider)).Append("</span></p>");
            sb.Append("<p>OWIN only mounts <strong>FileExplorer</strong>. For all 25 UIs and 40+ storage backends, use <code>FileManager.Sample.*.Full</code> (ASP.NET Core).</p>");
            sb.Append("<h2>FileExplorer</h2><ul>");
            sb.Append("<li><a href=\"/Explorer/\">/Explorer/</a> (recommended)</li>");
            sb.Append("<li><a href=\"/Browser/\">/Browser/</a> (redirects to /Explorer/)</li>");
            sb.Append("</ul>");
            sb.Append("<h2>Storage providers (this host)</h2><ul>");
            foreach (var provider in OwinSampleStorage.SupportedProviders)
            {
                var active = string.Equals(provider, currentProvider, StringComparison.OrdinalIgnoreCase);
                sb.Append("<li>");
                if (active)
                    sb.Append("<strong>").Append(Escape(provider)).Append("</strong> (active)");
                else
                    sb.Append(Escape(provider)).Append(" — set <code>FileManager__Storage__Provider=").Append(Escape(provider)).Append("</code>");
                sb.Append("</li>");
            }
            sb.Append("</ul>");
            sb.Append("<h2>ASP.NET Core Full samples</h2><ul>");
            sb.Append("<li>Mvc.Full — http://localhost:5000</li>");
            sb.Append("<li>Blazor.Full — http://localhost:2131</li>");
            sb.Append("<li>RazorPages.Full — http://localhost:2116</li>");
            sb.Append("</ul></body></html>");
            return sb.ToString();
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }
    }
}

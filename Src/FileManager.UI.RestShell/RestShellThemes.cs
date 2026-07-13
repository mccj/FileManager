using System.Collections.Generic;

namespace FileManager
{
    internal static class RestShellThemes
    {
        internal sealed class ThemeInfo
        {
            public string Title { get; set; }
            public string AccentColor { get; set; }
            public IReadOnlyList<string> Features { get; set; } = System.Array.Empty<string>();
        }

        private static readonly Dictionary<string, ThemeInfo> Themes = new Dictionary<string, ThemeInfo>(System.StringComparer.OrdinalIgnoreCase)
        {
            ["spa"] = new ThemeInfo { Title = "SPA File Manager", AccentColor = "#0f766e" },
            ["vue"] = new ThemeInfo { Title = "Vue File Manager", AccentColor = "#42b883" },
            ["react"] = new ThemeInfo { Title = "React File Manager", AccentColor = "#61dafb" },
            ["angular"] = new ThemeInfo { Title = "Angular File Manager", AccentColor = "#dd0031" },
            ["blazor"] = new ThemeInfo { Title = "Blazor File Manager", AccentColor = "#512bd4" },
            ["svelte"] = new ThemeInfo { Title = "Svelte File Manager", AccentColor = "#ff3e00" },
            ["nextjs"] = new ThemeInfo { Title = "Next.js File Manager", AccentColor = "#000000" },
            ["preact"] = new ThemeInfo { Title = "Preact File Manager", AccentColor = "#673ab8" },
            ["solidjs"] = new ThemeInfo { Title = "Solid.js File Manager", AccentColor = "#2c4f7c" },
            ["dropzone"] = new ThemeInfo { Title = "Dropzone Upload", AccentColor = "#0087f7", Features = new[] { "dropzone" } },
            ["medialibrary"] = new ThemeInfo { Title = "Media Library", AccentColor = "#f59e0b", Features = new[] { "media" } },
            ["mobile"] = new ThemeInfo { Title = "Mobile File Manager", AccentColor = "#10b981", Features = new[] { "mobile" } },
            ["bootstrap5"] = new ThemeInfo { Title = "Bootstrap 5 File Manager", AccentColor = "#7952b3", Features = new[] { "bootstrap" } },
            ["themes"] = new ThemeInfo { Title = "Themed File Manager", AccentColor = "#334155", Features = new[] { "dark-toggle" } },
            ["i18n"] = new ThemeInfo { Title = "i18n File Manager", AccentColor = "#059669", Features = new[] { "i18n" } },
            ["sdkdemo"] = new ThemeInfo { Title = "SDK / API Demo", AccentColor = "#6366f1", Features = new[] { "sdk-hint" } },
            ["default"] = new ThemeInfo { Title = "File Manager", AccentColor = "#2563eb" },
        };

        internal static ThemeInfo Resolve(string theme)
        {
            if (string.IsNullOrWhiteSpace(theme))
                return Themes["default"];
            return Themes.TryGetValue(theme, out var info) ? info : Themes["default"];
        }

        internal static bool IsRestShellKind(string uiModuleKind)
        {
            if (string.IsNullOrEmpty(uiModuleKind))
                return false;
            if (uiModuleKind.Equals("restshell", System.StringComparison.OrdinalIgnoreCase))
                return true;
            return Themes.ContainsKey(uiModuleKind);
        }

        internal static string ResolveThemeName(string uiModuleKind, string explicitTheme)
        {
            if (!string.IsNullOrWhiteSpace(explicitTheme))
                return explicitTheme;
            if (uiModuleKind.Equals("restshell", System.StringComparison.OrdinalIgnoreCase))
                return "default";
            return uiModuleKind;
        }
    }
}

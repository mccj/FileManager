namespace FileManager
{
    /// <summary>
    /// Public helpers for routing legacy REST-shell UI kinds through <see cref="RestShellFileManagerUiModule"/>.
    /// </summary>
    public static class RestShellUi
    {
        public static bool IsShellKind(string uiModuleKind) => RestShellThemes.IsRestShellKind(uiModuleKind);

        public static string ResolveTheme(string uiModuleKind, string explicitTheme) =>
            RestShellThemes.ResolveThemeName(uiModuleKind, explicitTheme);
    }
}

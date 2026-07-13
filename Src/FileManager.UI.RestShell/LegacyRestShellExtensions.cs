namespace FileManager

{

    /// <summary>

    /// Back-compat <c>Use*FileManagerUI</c> entry points that route through the unified RestShell module.

    /// Prefer <see cref="RestShellApplicationBuilderExtensions.UseRestShellFileManagerUI"/> with an explicit theme.

    /// </summary>

    public static class LegacyRestShellExtensions

    {

        [System.Obsolete("Use UseRestShellFileManagerUI(\"spa\") and reference FileManager.UI.RestShell.")]

        public static void UseSpaFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "spa";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"vue\") and reference FileManager.UI.RestShell.")]

        public static void UseVueFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "vue";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"react\") and reference FileManager.UI.RestShell.")]

        public static void UseReactFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "react";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"angular\") and reference FileManager.UI.RestShell.")]

        public static void UseAngularFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "angular";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"blazor\") and reference FileManager.UI.RestShell.")]

        public static void UseBlazorFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "blazor";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"svelte\") and reference FileManager.UI.RestShell.")]

        public static void UseSvelteFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "svelte";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"nextjs\") and reference FileManager.UI.RestShell.")]

        public static void UseNextJsFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "nextjs";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"preact\") and reference FileManager.UI.RestShell.")]

        public static void UsePreactFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "preact";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"solidjs\") and reference FileManager.UI.RestShell.")]

        public static void UseSolidJsFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "solidjs";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"dropzone\") and reference FileManager.UI.RestShell.")]

        public static void UseDropzoneFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "dropzone";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"medialibrary\") and reference FileManager.UI.RestShell.")]

        public static void UseMediaLibraryFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "medialibrary";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"mobile\") and reference FileManager.UI.RestShell.")]

        public static void UseMobileFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "mobile";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"bootstrap5\") and reference FileManager.UI.RestShell.")]

        public static void UseBootstrap5FileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "bootstrap5";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"themes\") and reference FileManager.UI.RestShell.")]

        public static void UseThemesFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "themes";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"i18n\") and reference FileManager.UI.RestShell.")]

        public static void UseI18nFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "i18n";

        [System.Obsolete("Use UseRestShellFileManagerUI(\"sdkdemo\") and reference FileManager.UI.RestShell.")]

        public static void UseSdkDemoFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "sdkdemo";

    }

}


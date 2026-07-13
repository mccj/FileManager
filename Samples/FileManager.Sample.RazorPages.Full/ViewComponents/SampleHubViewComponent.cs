using FileManager.Sample.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Sample.RazorPages.Full.ViewComponents;

public class SampleHubViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string hostTitle,
        string? storageProvider,
        bool readOnly,
        string mountMode,
        bool showApiCards = true)
    {
        var includeExtended = string.Equals(mountMode, "Full", StringComparison.OrdinalIgnoreCase);
        return View(new SampleHubViewModel
        {
            HostTitle = hostTitle,
            StorageProvider = storageProvider ?? "Physical",
            ReadOnly = readOnly,
            MountMode = mountMode ?? "Lite",
            IncludeExtendedUis = includeExtended,
            ShowApiCards = showApiCards,
            UiCards = SampleHubCatalog.GetUiCards(includeExtended),
            ApiCards = showApiCards ? SampleHubCatalog.GetApiCards() : Array.Empty<SampleHubApiCard>(),
            StorageProviders = SampleHubCatalog.GetStorageProviders(),
        });
    }
}

using FileManager.Sample.Shared;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace FileManager.Sample.RazorPages.Full.Pages;

public class IndexModel : PageModel
{
    private readonly SampleStorageOptions _storage;
    private readonly SampleShowcaseOptions _showcase;

    public IndexModel(IOptions<SampleStorageOptions> storage, IOptions<SampleShowcaseOptions> showcase)
    {
        _storage = storage.Value;
        _showcase = showcase.Value;
    }

    public string StorageProvider => _storage.Provider ?? "Physical";

    public bool IsReadOnly => SampleStorageConfigurator.IsReadOnlyProvider(_storage.Provider);

    public string MountMode => _showcase.MountMode ?? "Lite";

    public void OnGet()
    {
    }
}

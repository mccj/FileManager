namespace FileManager.Sample.Shared;

public sealed class SampleHubViewModel
{
    public string HostTitle { get; init; } = "Full";

    public string StorageProvider { get; init; } = "Physical";

    public bool ReadOnly { get; init; }

    public string MountMode { get; init; } = "Lite";

    public bool IncludeExtendedUis { get; init; }

    public bool ShowApiCards { get; init; } = true;

    public IReadOnlyList<SampleHubUiCard> UiCards { get; init; } = Array.Empty<SampleHubUiCard>();

    public IReadOnlyList<SampleHubApiCard> ApiCards { get; init; } = Array.Empty<SampleHubApiCard>();

    public IReadOnlyList<string> StorageProviders { get; init; } = Array.Empty<string>();
}

using System.Collections.Generic;

namespace FileManager.FileStorage
{
    /// <summary>
    /// Marks a file store that exposes multiple named drives (for example <c>CompositeFileStore</c>).
    /// Explorer paths such as <c>[local]:docs/file.txt</c> keep the drive prefix for these stores.
    /// </summary>
    public interface IMultiDriveFileStore : IFileStore
    {
        IReadOnlyCollection<string> DriveNames { get; }
    }
}

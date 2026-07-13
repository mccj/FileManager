using System;

namespace FileManager.FileStorage
{
    [Flags]
    public enum FileStoreCapabilityFlags
    {
        None = 0,
        Read = 1,
        List = 2,
        Download = 4,
        Preview = 8,
        Write = 16,
        Delete = 32,
        Move = 64,
        Copy = 128,
        Compress = 256,
        Extract = 512,
        All = Read | List | Download | Preview | Write | Delete | Move | Copy | Compress | Extract
    }

    public interface IFileStoreCapabilities
    {
        FileStoreCapabilityFlags Capabilities { get; }
    }

    public static class FileStoreCapabilitiesExtensions
    {
        public static FileStoreCapabilityFlags GetCapabilities(this IFileStore fileStore)
        {
            if (fileStore is IFileStoreCapabilities capabilities)
                return capabilities.Capabilities;

            return FileStoreCapabilityFlags.All;
        }

        public static bool Supports(this IFileStore fileStore, FileStoreCapabilityFlags capability)
        {
            return (fileStore.GetCapabilities() & capability) == capability;
        }

        public static Permission GetReadOnlyFolderPermissions()
        {
            return Permission.ListSubFolders | Permission.ListFiles | Permission.Download | Permission.Preview;
        }
    }
}

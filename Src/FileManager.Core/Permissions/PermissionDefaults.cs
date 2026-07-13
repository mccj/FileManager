namespace FileManager.FileStorage
{
    public static class PermissionDefaults
    {
        public static Permission ReadOnly { get; } =
            Permission.ListSubFolders | Permission.ListFiles | Permission.Download | Permission.Preview;
    }
}

namespace FileManager.UI.Explorer
{
    public enum Permission
    {
        /// <summary>
        /// 列表子文件夹
        /// </summary>
        ListSubFolders = 1,
        /// <summary>
        /// 列表文件
        /// </summary>
        ListFiles = 2,
        /// <summary>
        /// 创建
        /// </summary>
        Create = 4,
        /// <summary>
        /// 删除
        /// </summary>
        Delete = 8,
        /// <summary>
        /// 重命名
        /// </summary>
        Rename = 16,
        /// <summary>
        /// 编辑
        /// </summary>
        Edit = 32,
        /// <summary>
        /// 上载
        /// </summary>
        Upload = 64,
        /// <summary>
        /// 下载
        /// </summary>
        Download = 128,
        /// <summary>
        /// 压缩
        /// </summary>
        Compress = 256,
        /// <summary>
        /// 提取
        /// </summary>
        Extract = 512,
        /// <summary>
        /// 剪切
        /// </summary>
        Cut = 1024,
        /// <summary>
        /// 复制
        /// </summary>
        Copy = 2048,
        /// <summary>
        /// 粘贴
        /// </summary>
        Paste = 4096,
        /// <summary>
        /// 预览
        /// </summary>
        Preview = 8192,
        /// <summary>
        /// 打印
        /// </summary>
        Print = 16384,
        /// <summary>
        /// 创建公共链接
        /// </summary>
        CreatePublicLink = 1073741824
    }
}
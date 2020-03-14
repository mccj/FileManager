using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager.FileStorage
{
    public interface IPermissions
    {
        //Task<Permission> GetFilePermissionAsync(string path);
        Task<Permission> GetPermissionAsync(IFileStoreEntry fileStoreEntry);
    }
    public class DefaultPermissions : IPermissions
    {
        private readonly Func<IFileStoreEntry, Task<Permission>> _func;

        public DefaultPermissions(Func<IFileStoreEntry, Task<Permission>> func)
        {
            this._func = func;
        }

        //public Task<Permission> GetFilePermissionAsync(string path)
        //{
        //    throw new NotImplementedException();
        //}

        public Task<Permission> GetPermissionAsync(IFileStoreEntry fileStoreEntry)
        {
            return _func(fileStoreEntry);
        }
    }

    public class DefaultPermissionHandle
    {
        private readonly IEnumerable<IPermissions> _permissions;

        public DefaultPermissionHandle(IEnumerable<IPermissions> permissions)
        {
            this._permissions = permissions;
        }
        public Task<Permission> GetPermissionAsync(IFileStoreEntry fileStoreEntry)
        {
            if (_permissions.Any()) { 
            var r = _permissions.Select(async f => await f.GetPermissionAsync(fileStoreEntry));
            return Task.Run(() =>
            {
                return (Permission)r.Sum(f => (int)f.Result);
            });
            }
            else
            {
                //var read = Permission.ListSubFolders | Permission.ListFiles | Permission.Download | Permission.Copy | Permission.Preview | Permission.Print;
                var full = Permission.ListSubFolders | Permission.ListFiles | Permission.Download | Permission.Copy | Permission.Preview | Permission.Print
                    | Permission.Create | Permission.Delete | Permission.Rename | Permission.Edit | Permission.Upload | Permission.Compress | Permission.Extract | Permission.Cut | Permission.Paste | Permission.CreatePublicLink;

                return Task.FromResult(full);
            }
        }
    }
}

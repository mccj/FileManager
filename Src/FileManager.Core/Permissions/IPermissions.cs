using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager.FileStorage
{
    public interface IPermissions
    {
        Task<Permission> GetPermissionAsync(IFileStoreEntry fileStoreEntry);
    }
    public class DefaultPermissions : IPermissions
    {
        private readonly Func<IFileStoreEntry, Task<Permission>> _func;

        public DefaultPermissions(Func<IFileStoreEntry, Task<Permission>> func)
        {
            this._func = func;
        }

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
        public async Task<Permission> GetPermissionAsync(IFileStoreEntry fileStoreEntry)
        {
            if (_permissions.Any())
            {
                var results = await Task.WhenAll(_permissions.Select(f => f.GetPermissionAsync(fileStoreEntry)));
                Permission combined = 0;
                foreach (var permission in results)
                    combined |= permission;
                return combined;
            }

            return PermissionDefaults.ReadOnly;
        }
    }
}

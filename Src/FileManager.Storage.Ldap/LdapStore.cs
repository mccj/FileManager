using FileManager.FileStorage;

using Novell.Directory.Ldap;

using System;

using System.Collections.Generic;

using System.IO;

using System.Linq;

using System.Text;

using System.Threading;

using System.Threading.Tasks;



namespace FileManager.Storage.Ldap

{

    public sealed class LdapStore : IFileStore, IFileStoreCapabilities

    {

        private static readonly FileStoreCapabilityFlags ReadOnlyCapabilities =

            FileStoreCapabilityFlags.Read | FileStoreCapabilityFlags.List | FileStoreCapabilityFlags.Download | FileStoreCapabilityFlags.Preview;



        private static readonly HashSet<string> ContainerObjectClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)

        {

            "organizationalUnit",

            "organization",

            "container",

            "domain",

            "country",

            "locality"

        };



        private readonly string _host;

        private readonly int _port;

        private readonly string _baseDn;

        private readonly string _bindDn;

        private readonly string _bindPassword;

        private readonly bool _useSsl;



        public LdapStore(string host, int port, string baseDn, string bindDn = null, string bindPassword = null, bool useSsl = false)

        {

            if (string.IsNullOrWhiteSpace(host))

                throw new ArgumentException("Host is required.", nameof(host));

            if (string.IsNullOrWhiteSpace(baseDn))

                throw new ArgumentException("Base DN is required.", nameof(baseDn));



            _host = host;

            _port = port;

            _baseDn = baseDn;

            _bindDn = bindDn;

            _bindPassword = bindPassword;

            _useSsl = useSsl;

        }



        public FileStoreCapabilityFlags Capabilities => ReadOnlyCapabilities;



        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            using var connection = CreateConnection();

            var entry = ResolveEntry(connection, path);

            if (entry == null || IsDirectoryEntry(connection, entry))

                return Task.FromResult<IFileStoreEntry>(null);



            var content = SerializeEntry(entry);

            return Task.FromResult<IFileStoreEntry>(CreateEntry(path, false, content.Length, DateTimeOffset.UtcNow));

        }



        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path) ?? string.Empty;

            using var connection = CreateConnection();

            var dn = ResolveDn(path);

            if (string.IsNullOrEmpty(path))

                return Task.FromResult<IFileStoreEntry>(CreateEntry(path, true, 0, DateTimeOffset.UtcNow));



            try

            {

                var entry = connection.Read(dn);

                if (entry == null)

                    return Task.FromResult<IFileStoreEntry>(null);



                if (IsDirectoryEntry(connection, entry) || HasChildren(connection, dn))

                    return Task.FromResult<IFileStoreEntry>(CreateEntry(path, true, 0, DateTimeOffset.UtcNow));



                return Task.FromResult<IFileStoreEntry>(null);

            }

            catch (LdapException)

            {

                return Task.FromResult<IFileStoreEntry>(null);

            }

        }



        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path) ?? string.Empty;

            using var connection = CreateConnection();

            var dn = ResolveDn(path);

            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);



            foreach (var child in SearchImmediateChildren(connection, dn))

            {

                cancellationToken.ThrowIfCancellationRequested();

                var name = GetEntryName(child);

                if (string.IsNullOrEmpty(name))

                    continue;



                var childPath = this.Combine(path, name);

                var isDirectory = IsDirectoryEntry(connection, child) || HasChildren(connection, child.Dn);

                if (isDirectory)

                {

                    entries[name] = CreateEntry(childPath, true, 0, DateTimeOffset.UtcNow);

                }

                else

                {

                    var content = SerializeEntry(child);

                    entries[name] = CreateEntry(childPath, false, content.Length, DateTimeOffset.UtcNow);

                }

            }



            if (includeSubDirectories)

            {

                foreach (var directory in entries.Values.Where(e => e.IsDirectory).ToArray())

                {

                    foreach (var nested in GetDirectoryContentAsync(directory.Path, true, cancellationToken).GetAwaiter().GetResult())

                    {

                        if (!entries.ContainsKey(nested.Path))

                            entries[nested.Path] = nested;

                    }

                }

            }



            return Task.FromResult(entries.Values.AsEnumerable());

        }



        public Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync<bool>();

        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync<bool>();

        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync<bool>();

        public Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();

        public Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();

        public Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();

        public Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();



        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            using var connection = CreateConnection();

            var entry = ResolveEntry(connection, path);

            if (entry == null || IsDirectoryEntry(connection, entry))

                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");



            var content = SerializeEntry(entry);

            return Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(content), writable: false));

        }



        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)

        {

            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);

        }



        private LdapConnection CreateConnection()

        {

            var connection = new LdapConnection();

            if (_useSsl)

                connection.SecureSocketLayer = true;



            connection.Connect(_host, _port);

            if (!string.IsNullOrWhiteSpace(_bindDn))

                connection.Bind(_bindDn, _bindPassword ?? string.Empty);

            else

                connection.Bind(string.Empty, string.Empty);



            return connection;

        }



        private string ResolveDn(string path)

        {

            if (string.IsNullOrEmpty(path))

                return _baseDn;



            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var currentDn = _baseDn;

            using var connection = CreateConnection();



            foreach (var segment in segments)

            {

                var child = FindChildByName(connection, currentDn, segment);

                if (child == null)

                    throw new FileStoreException($"LDAP path '{path}' was not found.");



                currentDn = child.Dn;

            }



            return currentDn;

        }



        private LdapEntry ResolveEntry(LdapConnection connection, string path)

        {

            try

            {

                var dn = ResolveDn(path);

                return connection.Read(dn);

            }

            catch (FileStoreException)

            {

                return null;

            }

            catch (LdapException)

            {

                return null;

            }

        }



        private IEnumerable<LdapEntry> SearchImmediateChildren(LdapConnection connection, string dn)

        {

            var results = connection.Search(dn, LdapConnection.ScopeOne, "(objectClass=*)", null, false);

            while (results.HasMore())

            {

                yield return results.Next();

            }

        }



        private static LdapEntry FindChildByName(LdapConnection connection, string parentDn, string name)

        {

            foreach (var child in SearchStaticChildren(connection, parentDn))

            {

                if (GetEntryName(child).Equals(name, StringComparison.OrdinalIgnoreCase))

                    return child;

            }



            return null;

        }



        private static IEnumerable<LdapEntry> SearchStaticChildren(LdapConnection connection, string dn)

        {

            var results = connection.Search(dn, LdapConnection.ScopeOne, "(objectClass=*)", null, false);

            while (results.HasMore())

            {

                yield return results.Next();

            }

        }



        private static bool HasChildren(LdapConnection connection, string dn)

        {

            var results = connection.Search(dn, LdapConnection.ScopeOne, "(objectClass=*)", null, false);

            return results.HasMore();

        }



        private static bool IsDirectoryEntry(LdapConnection connection, LdapEntry entry)

        {

            var objectClasses = entry.GetAttributeSet()

                .Cast<LdapAttribute>()

                .Where(a => a.Name.Equals("objectClass", StringComparison.OrdinalIgnoreCase))

                .SelectMany(a => a.StringValueArray)

                .ToArray();



            if (objectClasses.Any(objectClass => ContainerObjectClasses.Contains(objectClass)))

                return true;



            return HasChildren(connection, entry.Dn);

        }



        private static string GetEntryName(LdapEntry entry)

        {

            var dn = entry.Dn;

            var separator = dn.IndexOf(',');

            var rdn = separator >= 0 ? dn.Substring(0, separator) : dn;

            var equals = rdn.IndexOf('=');

            return equals >= 0 ? rdn.Substring(equals + 1) : rdn;

        }



        private static string SerializeEntry(LdapEntry entry)

        {

            var builder = new StringBuilder();

            builder.AppendLine($"dn: {entry.Dn}");

            foreach (LdapAttribute attribute in entry.GetAttributeSet())

            {

                foreach (var value in attribute.StringValueArray)

                    builder.AppendLine($"{attribute.Name}: {value}");

            }



            return builder.ToString();

        }



        private static Task ThrowReadOnlyAsync()

        {

            throw new FileStoreException("LdapStore is read-only.");

        }



        private static Task<T> ThrowReadOnlyAsync<T>()

        {

            throw new FileStoreException("LdapStore is read-only.");

        }



        private static LdapStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)

            => new LdapStoreEntry(path ?? string.Empty, isDirectory, length, modified);

    }

}


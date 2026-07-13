using FileManager.FileStorage;

using RabbitMQ.Client;

using System;

using System.Collections.Concurrent;

using System.Collections.Generic;

using System.IO;

using System.Linq;

using System.Text;

using System.Threading;

using System.Threading.Tasks;



namespace FileManager.Storage.RabbitMq

{

    public sealed class RabbitMqStore : IFileStore, IDisposable

    {

        private const string DirectoriesQueue = "fm.dirs";

        private const string FileQueuePrefix = "fm.file.";



        private readonly IConnection _connection;

        private readonly IModel _channel;

        private readonly ConcurrentDictionary<string, byte> _directories =

            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, byte> _files =

            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        private readonly object _sync = new object();



        public RabbitMqStore(string connectionString)

            : this(CreateConnection(connectionString))

        {

        }



        public RabbitMqStore(IConnection connection)

        {

            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

            _channel = _connection.CreateModel();

            Initialize();

        }



        public void Dispose()

        {

            _channel?.Dispose();

            _connection?.Dispose();

        }



        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            if (!_files.ContainsKey(path))

                return Task.FromResult<IFileStoreEntry>(null);



            var length = GetFileLength(path);

            return Task.FromResult<IFileStoreEntry>(CreateEntry(path, false, length, DateTimeOffset.UtcNow));

        }



        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path) ?? string.Empty;

            if (_directories.ContainsKey(path) || HasChildren(path))

                return Task.FromResult<IFileStoreEntry>(CreateEntry(path, true, 0, DateTimeOffset.UtcNow));



            return Task.FromResult<IFileStoreEntry>(null);

        }



        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path) ?? string.Empty;

            if (!_directories.ContainsKey(path) && !HasChildren(path))

                return Task.FromResult(Enumerable.Empty<IFileStoreEntry>());



            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);

            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";



            foreach (var directory in _directories.Keys)

            {

                cancellationToken.ThrowIfCancellationRequested();

                if (!IsDirectChild(path, prefix, directory, out var childName))

                    continue;

                if (!entries.ContainsKey(childName))

                    entries[childName] = CreateEntry(this.Combine(path, childName), true, 0, DateTimeOffset.UtcNow);

            }



            foreach (var filePath in _files.Keys)

            {

                cancellationToken.ThrowIfCancellationRequested();

                if (!IsDirectChild(path, prefix, filePath, out var childName))

                    continue;

                if (entries.ContainsKey(childName))

                    continue;



                var length = GetFileLength(filePath);

                entries[childName] = CreateEntry(this.Combine(path, childName), false, length, DateTimeOffset.UtcNow);

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



        public Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            if (_files.ContainsKey(path))

                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");



            EnsureParentDirectories(path);

            if (!_directories.TryAdd(path, 0))

                return Task.FromResult(false);



            PublishDirectory(path);

            return Task.FromResult(true);

        }



        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            if (!_files.TryRemove(path, out _))

                return Task.FromResult(false);



            var queueName = GetFileQueueName(path);

            lock (_sync)

            {

                _channel.QueueDelete(queueName);

            }



            return Task.FromResult(true);

        }



        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path) ?? string.Empty;

            if (string.IsNullOrEmpty(path))

                throw new FileStoreException("Cannot delete the root directory.");



            var prefix = path.TrimEnd('/') + "/";

            foreach (var filePath in _files.Keys.Where(k => k.Equals(path, StringComparison.OrdinalIgnoreCase) || k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())

                TryDeleteFileAsync(filePath, cancellationToken).GetAwaiter().GetResult();



            foreach (var directory in _directories.Keys.Where(k => k.Equals(path, StringComparison.OrdinalIgnoreCase) || k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())

            {

                _directories.TryRemove(directory, out _);

                PublishDirectoryRemoval(directory);

            }



            return Task.FromResult(true);

        }



        public Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            oldPath = this.NormalizePath(oldPath);

            newPath = this.NormalizePath(newPath);

            if (!_files.ContainsKey(oldPath))

                throw new FileStoreException($"The file '{oldPath}' does not exist.");

            if (_files.ContainsKey(newPath))

                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");



            var content = ReadFileContent(oldPath);

            EnsureParentDirectories(newPath);

            WriteFileContent(newPath, content);

            TryDeleteFileAsync(oldPath, cancellationToken).GetAwaiter().GetResult();

            return Task.CompletedTask;

        }



        public Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            srcPath = this.NormalizePath(srcPath);

            dstPath = this.NormalizePath(dstPath);

            if (!_files.ContainsKey(srcPath))

                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            if (_files.ContainsKey(dstPath))

                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");



            EnsureParentDirectories(dstPath);

            WriteFileContent(dstPath, ReadFileContent(srcPath));

            return Task.CompletedTask;

        }



        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            if (!_files.ContainsKey(path))

                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");



            var content = ReadFileContent(path);

            return Task.FromResult<Stream>(new MemoryStream(content, writable: false));

        }



        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)

        {

            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);

        }



        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            if (!overwrite && _files.ContainsKey(path))

                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            if (_directories.ContainsKey(path))

                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");



            using var memory = new MemoryStream();

            await inputStream.CopyToAsync(memory, 81920, cancellationToken);

            EnsureParentDirectories(path);

            WriteFileContent(path, memory.ToArray());

        }



        public async Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            oldPath = this.NormalizePath(oldPath);

            newPath = this.NormalizePath(newPath);

            if (await GetDirectoryInfoAsync(oldPath, cancellationToken) == null)

                throw new FileStoreException($"Cannot move directory '{oldPath}' because it does not exist.");

            if (await GetDirectoryInfoAsync(newPath, cancellationToken) != null || await GetFileInfoAsync(newPath, cancellationToken) != null)

                throw new FileStoreException($"Cannot move directory because the new path '{newPath}' already exists.");



            await this.CopyDirectoryAsync(oldPath, newPath, cancellationToken);

            await TryDeleteDirectoryAsync(oldPath, cancellationToken);

        }



        private static IConnection CreateConnection(string connectionString)

        {

            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };

            return factory.CreateConnection();

        }



        private void Initialize()

        {

            lock (_sync)

            {

                _channel.QueueDeclare(DirectoriesQueue, durable: true, exclusive: false, autoDelete: false);

                _directories[string.Empty] = 0;



                while (true)

                {

                    var result = _channel.BasicGet(DirectoriesQueue, autoAck: true);

                    if (result == null)

                        break;



                    var path = Encoding.UTF8.GetString(result.Body.ToArray());

                    if (!string.IsNullOrEmpty(path))

                        _directories[path] = 0;

                }



                PublishDirectory(string.Empty);

            }

        }



        private void PublishDirectory(string path)

        {

            lock (_sync)

            {

                var body = Encoding.UTF8.GetBytes(path ?? string.Empty);

                _channel.BasicPublish(string.Empty, DirectoriesQueue, null, body);

            }

        }



        private void PublishDirectoryRemoval(string path)

        {

            // Directory removals are tracked in-memory; queues are append-only metadata.

        }



        private void EnsureParentDirectories(string path)

        {

            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length <= 1)

                return;



            var current = string.Empty;

            for (var i = 0; i < segments.Length - 1; i++)

            {

                current = string.IsNullOrEmpty(current) ? segments[i] : current + "/" + segments[i];

                if (_directories.TryAdd(current, 0))

                    PublishDirectory(current);

            }

        }



        private void WriteFileContent(string path, byte[] content)

        {

            var queueName = GetFileQueueName(path);

            lock (_sync)

            {

                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

                _channel.QueuePurge(queueName);

                var properties = _channel.CreateBasicProperties();

                properties.Persistent = true;

                _channel.BasicPublish(string.Empty, queueName, properties, content);

            }



            _files[path] = 0;

        }



        private byte[] ReadFileContent(string path)

        {

            var queueName = GetFileQueueName(path);

            lock (_sync)

            {

                var result = _channel.BasicGet(queueName, autoAck: true);

                if (result == null)

                    throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");



                var content = result.Body.ToArray();

                var properties = _channel.CreateBasicProperties();

                properties.Persistent = true;

                _channel.BasicPublish(string.Empty, queueName, properties, content);

                return content;

            }

        }



        private long GetFileLength(string path)

        {

            var queueName = GetFileQueueName(path);

            lock (_sync)

            {

                var result = _channel.QueueDeclarePassive(queueName);

                if (result.MessageCount == 0)

                    return 0;



                var message = _channel.BasicGet(queueName, autoAck: false);

                if (message == null)

                    return 0;



                var length = message.Body.Length;

                _channel.BasicNack(message.DeliveryTag, false, true);

                return length;

            }

        }



        private static string GetFileQueueName(string path)

        {

            return FileQueuePrefix + path.Replace('/', '.');

        }



        private bool HasChildren(string path)

        {

            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";

            return _directories.Keys.Any(k => IsDirectChild(path, prefix, k, out _))

                || _files.Keys.Any(k => IsDirectChild(path, prefix, k, out _));

        }



        private static bool IsDirectChild(string path, string prefix, string candidate, out string childName)

        {

            childName = null;

            if (string.IsNullOrEmpty(candidate))

                return false;

            if (!string.IsNullOrEmpty(path) && candidate.Equals(path, StringComparison.OrdinalIgnoreCase))

                return false;



            var relative = string.IsNullOrEmpty(prefix)

                ? candidate

                : candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? candidate.Substring(prefix.Length) : null;

            if (string.IsNullOrEmpty(relative))

                return false;



            var slash = relative.IndexOf('/');

            if (slash >= 0)

                relative = relative.Substring(0, slash);

            if (string.IsNullOrEmpty(relative))

                return false;



            childName = relative;

            return true;

        }



        private static RabbitMqStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)

            => new RabbitMqStoreEntry(path ?? string.Empty, isDirectory, length, modified);

    }

}


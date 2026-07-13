using Confluent.Kafka;

using FileManager.FileStorage;

using System;

using System.Collections.Concurrent;

using System.Collections.Generic;

using System.IO;

using System.Linq;

using System.Text;

using System.Threading;

using System.Threading.Tasks;



namespace FileManager.Storage.Kafka

{

    public sealed class KafkaStore : IFileStore, IDisposable

    {

        private const string DirectoryKeyPrefix = "dir:";

        private const string FileKeyPrefix = "file:";



        private readonly string _topic;

        private readonly IProducer<string, byte[]> _producer;

        private readonly ConcurrentDictionary<string, byte> _directories =

            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, FileMetadata> _files =

            new ConcurrentDictionary<string, FileMetadata>(StringComparer.OrdinalIgnoreCase);

        private readonly object _sync = new object();



        public KafkaStore(string bootstrapServers, string topic = "fm-files")

        {

            if (string.IsNullOrWhiteSpace(bootstrapServers))

                throw new ArgumentException("Bootstrap servers are required.", nameof(bootstrapServers));



            _topic = string.IsNullOrWhiteSpace(topic) ? "fm-files" : topic;

            var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };

            _producer = new ProducerBuilder<string, byte[]>(producerConfig).Build();

            LoadExistingState(bootstrapServers);

        }



        public void Dispose()

        {

            _producer?.Flush(TimeSpan.FromSeconds(5));

            _producer?.Dispose();

        }



        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            if (!_files.TryGetValue(path, out var metadata))

                return Task.FromResult<IFileStoreEntry>(null);



            return Task.FromResult<IFileStoreEntry>(CreateEntry(path, false, metadata.Length, metadata.LastModifiedUtc));

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



                var metadata = _files[filePath];

                entries[childName] = CreateEntry(this.Combine(path, childName), false, metadata.Length, metadata.LastModifiedUtc);

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



            Produce(DirectoryKeyPrefix + path, Array.Empty<byte>());

            return Task.FromResult(true);

        }



        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            if (!_files.TryRemove(path, out _))

                return Task.FromResult(false);



            Produce(FileKeyPrefix + path, null);

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

                Produce(DirectoryKeyPrefix + directory, null);

            }



            return Task.FromResult(true);

        }



        public Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            oldPath = this.NormalizePath(oldPath);

            newPath = this.NormalizePath(newPath);

            if (!_files.TryGetValue(oldPath, out var metadata))

                throw new FileStoreException($"The file '{oldPath}' does not exist.");

            if (_files.ContainsKey(newPath))

                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");



            EnsureParentDirectories(newPath);

            _files[newPath] = metadata;

            _files.TryRemove(oldPath, out _);

            Produce(FileKeyPrefix + newPath, metadata.Content);

            Produce(FileKeyPrefix + oldPath, null);

            return Task.CompletedTask;

        }



        public Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            srcPath = this.NormalizePath(srcPath);

            dstPath = this.NormalizePath(dstPath);

            if (!_files.TryGetValue(srcPath, out var source))

                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            if (_files.ContainsKey(dstPath))

                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");



            EnsureParentDirectories(dstPath);

            var copy = new FileMetadata

            {

                Content = source.Content.ToArray(),

                Length = source.Length,

                LastModifiedUtc = DateTimeOffset.UtcNow

            };

            _files[dstPath] = copy;

            Produce(FileKeyPrefix + dstPath, copy.Content);

            return Task.CompletedTask;

        }



        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            if (!_files.TryGetValue(path, out var metadata))

                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");



            return Task.FromResult<Stream>(new MemoryStream(metadata.Content, writable: false));

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

            var content = memory.ToArray();

            EnsureParentDirectories(path);

            _files[path] = new FileMetadata

            {

                Content = content,

                Length = content.LongLength,

                LastModifiedUtc = DateTimeOffset.UtcNow

            };

            Produce(FileKeyPrefix + path, content);

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



        private void LoadExistingState(string bootstrapServers)

        {

            _directories[string.Empty] = 0;

            var consumerConfig = new ConsumerConfig

            {

                BootstrapServers = bootstrapServers,

                GroupId = $"fm-kafka-store-{Guid.NewGuid():N}",

                AutoOffsetReset = AutoOffsetReset.Earliest,

                EnableAutoCommit = false

            };



            using var consumer = new ConsumerBuilder<string, byte[]>(consumerConfig).Build();

            try

            {

                consumer.Subscribe(_topic);

                while (true)

                {

                    var result = consumer.Consume(TimeSpan.FromMilliseconds(100));

                    if (result == null)

                        break;



                    ApplyMessage(result.Message.Key, result.Message.Value);

                }

            }

            catch (ConsumeException)

            {

            }

            finally

            {

                consumer.Close();

            }

        }



        private void ApplyMessage(string key, byte[] value)

        {

            if (string.IsNullOrEmpty(key))

                return;



            if (key.StartsWith(DirectoryKeyPrefix, StringComparison.Ordinal))

            {

                var path = key.Substring(DirectoryKeyPrefix.Length);

                if (value == null || value.Length == 0)

                    _directories.TryRemove(path, out _);

                else

                    _directories[path] = 0;

                return;

            }



            if (!key.StartsWith(FileKeyPrefix, StringComparison.Ordinal))

                return;



            var filePath = key.Substring(FileKeyPrefix.Length);

            if (value == null)

            {

                _files.TryRemove(filePath, out _);

                return;

            }



            _files[filePath] = new FileMetadata

            {

                Content = value,

                Length = value.LongLength,

                LastModifiedUtc = DateTimeOffset.UtcNow

            };

        }



        private void Produce(string key, byte[] value)

        {

            lock (_sync)

            {

                _producer.Produce(_topic, new Message<string, byte[]> { Key = key, Value = value },

                    report =>

                    {

                        if (report.Error.IsError)

                            throw new FileStoreException($"Kafka produce failed: {report.Error.Reason}");

                    });

                _producer.Flush(TimeSpan.FromSeconds(10));

            }

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

                    Produce(DirectoryKeyPrefix + current, Encoding.UTF8.GetBytes(current));

            }

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



        private static KafkaStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)

            => new KafkaStoreEntry(path ?? string.Empty, isDirectory, length, modified);



        private sealed class FileMetadata

        {

            public byte[] Content { get; set; } = Array.Empty<byte>();

            public long Length { get; set; }

            public DateTimeOffset LastModifiedUtc { get; set; }

        }

    }

}


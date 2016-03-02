namespace FreshLibrary
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Periodically cleans up a folder.
    /// </summary>
    public sealed class FreshFolder : IDisposable
    {
        /// <summary>
        /// Path of the folder to periodically cleanup.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "That's alright, it's readonly.")]
        public readonly string Path;

        /// <summary>
        /// The minimum lifetime of the file to be deleted.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "That's alright, it's readonly.")]
        public readonly TimeSpan Threshold;

        /// <summary>
        /// How often the cleanup occurs.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "That's alright, it's readonly.")]
        public readonly TimeSpan Interval;

        /// <summary>
        /// Timestamp used to determine if the file must be deleted.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "That's alright, it's readonly.")]
        public readonly FileTimestamp TimestampToCheck;

        private readonly object _cleanLock;

        private readonly Timer _timer;

        /// <summary>
        /// Constructs and starts the cleaner.
        /// </summary>
        /// <param name="path">Path of the folder to periodically cleanup</param>
        /// <param name="threshold">The minimum lifetime of the file to be deleted</param>
        /// <param name="interval">How often the cleanup occurs</param>
        /// <param name="timestampToCheck">Timestamp to use to determine if the file must be deleted</param>
        public FreshFolder(string path, TimeSpan threshold, TimeSpan interval, FileTimestamp timestampToCheck)
        {
            Path = path;
            Threshold = threshold;
            Interval = interval;
            TimestampToCheck = timestampToCheck;

            if (!Enum.IsDefined(typeof(FileTimestamp), timestampToCheck))
                throw new InvalidEnumArgumentException("Unexpected FileTimestamp: \"" + TimestampToCheck + "\"");

            _cleanLock = new object();
            _timer = new Timer(Clean, null, TimeSpan.Zero, interval);
        }

        /// <summary>
        /// Disposes this folder cleaner and it's underlying timer.
        /// </summary>
        public void Dispose()
        {
            lock (_cleanLock)
            {
                _timer.Dispose();
            }
        }

        private void Clean(object ignored)
        {
            lock (_cleanLock)
            {
                var now = DateTime.Now;
                if (Directory.Exists(Path))
                {
                    foreach (var filepath in Directory.GetFiles(Path))
                    {
                        var file = new FileInfo(filepath);
                        var timestamp = GetTimestamp(file);
                        var lifetime = now - timestamp;

                        if (lifetime >= Threshold)
                            file.Delete();
                    }
                }
            }
        }

        private DateTime GetTimestamp(FileInfo file)
        {
            switch (TimestampToCheck)
            {
                case FileTimestamp.Creation:
                    return file.CreationTime;

                case FileTimestamp.LastAccess:
                    return file.LastAccessTime;

                case FileTimestamp.LastWrite:
                    return file.LastWriteTime;

                default:
                    throw new InvalidEnumArgumentException("Unexpected FileTimestamp: \"" + TimestampToCheck + "\"");
            }
        }
    }
}

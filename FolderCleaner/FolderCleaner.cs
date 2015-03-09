using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace FolderCleaning
{
    /// <summary>
    /// Periodically cleans up a folder.
    /// </summary>
    public sealed class FolderCleaner : IDisposable
    {
        /// <summary>
        /// Path of the folder to periodically cleanup.
        /// </summary>
        public readonly string FolderPath;

        /// <summary>
        /// The minimum lifetime of the file to be deleted.
        /// </summary>
        public readonly TimeSpan MinimumFileLifetime;

        /// <summary>
        /// How often the cleanup occurs.
        /// </summary>
        public readonly TimeSpan CleanPeriod;

        /// <summary>
        /// Timestamp used to determine if the file must be deleted.
        /// </summary>
        public readonly FileTimestamps TimestampToUse;

        /// <summary>
        /// For thread safety.
        /// </summary>
        private readonly object _cleanLock;

        /// <summary>
        /// Cleanup timer.
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Constructs and starts the cleaner.
        /// </summary>
        /// <param name="folderPath">Path of the folder to periodically cleanup</param>
        /// <param name="minimumFileLifetime">The minimum lifetime of the file to be deleted</param>
        /// <param name="cleanPeriod">How often the cleanup occurs</param>
        /// <param name="timestampToUse">Timestamp to use to determine if the file must be deleted</param>
        public FolderCleaner(string folderPath, TimeSpan minimumFileLifetime, TimeSpan cleanPeriod,
            FileTimestamps timestampToUse)
        {
            FolderPath = folderPath;
            MinimumFileLifetime = minimumFileLifetime;
            CleanPeriod = cleanPeriod;
            CheckTimestamp(timestampToUse);
            TimestampToUse = timestampToUse;
            _cleanLock = new object();
            _timer = new Timer(CleanFiles, null, TimeSpan.Zero, cleanPeriod);
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

        /// <summary>
        /// Performs the cleanup.
        /// </summary>
        /// <param name="ignored">State object passed by the timer class</param>
        private void CleanFiles(object ignored)
        {
            lock (_cleanLock)
            {
                var now = DateTime.Now;
                foreach (var filepath in Directory.GetFiles(FolderPath))
                {
                    var file = new FileInfo(filepath);
                    var timestamp = GetTimestamp(file);
                    var lifetime = now - timestamp;
                    if (lifetime >= MinimumFileLifetime) file.Delete();
                }
            }
        }

        /// <summary>
        /// Checks if the given timestamp is a valid FileTimestamps enum value.
        /// </summary>
        /// <param name="timestamp">Value to check</param>
        /// <exception cref="InvalidEnumArgumentException">
        /// If an value not present in FileTimestamps enum was given
        /// </exception>
        private void CheckTimestamp(FileTimestamps timestamp)
        {
            switch (timestamp)
            {
                case FileTimestamps.Creation:
                case FileTimestamps.LastAccess:
                case FileTimestamps.LastWrite:
                    return;
                default:
                    throw new InvalidEnumArgumentException("Unexpected FileTimestamp: \"" + TimestampToUse + "\"");
            }
        }

        /// <summary>
        /// Returns the file timestamp to use for the cleanup.
        /// </summary>
        /// <param name="file">File to get the timestamp</param>
        /// <returns>Timestamp to use for the cleanup</returns>
        private DateTime GetTimestamp(FileInfo file)
        {
            switch (TimestampToUse)
            {
                case FileTimestamps.Creation:
                    return file.CreationTime;
                case FileTimestamps.LastAccess:
                    return file.LastAccessTime;
                case FileTimestamps.LastWrite:
                    return file.LastWriteTime;
                default:
                    throw new InvalidEnumArgumentException("Unexpected FileTimestamp: \"" + TimestampToUse + "\"");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBooster
{
    public class SystemCleanup
    {
        public class CleanupResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public long SpaceFreed { get; set; }
            public int FilesCleaned { get; set; }
            public List<string> CleanedFiles { get; set; } = new List<string>();
        }

        public class CleanupOptions
        {
            public bool CleanTempFiles { get; set; } = true;
            public bool CleanRecycleBin { get; set; } = false;
            public bool CleanBrowserCache { get; set; } = true;
            public bool CleanLogFiles { get; set; } = true;
            public bool CleanOldUpdates { get; set; } = false;
            public bool CleanThumbnails { get; set; } = true;
            public int DaysOld { get; set; } = 7; // Clean files older than X days
        }

        public async Task<CleanupResult> PerformCleanupAsync(CleanupOptions options = null)
        {
            options = options ?? new CleanupOptions();
            var result = new CleanupResult();
            long totalSpaceFreed = 0;
            int totalFilesCleaned = 0;
            var cleanedFiles = new List<string>();

            try
            {
                if (options.CleanTempFiles)
                {
                    var tempResult = CleanTempFiles(options.DaysOld);
                    totalSpaceFreed += tempResult.SpaceFreed;
                    totalFilesCleaned += tempResult.FilesCleaned;
                    cleanedFiles.AddRange(tempResult.CleanedFiles);
                }

                if (options.CleanBrowserCache)
                {
                    var browserResult = CleanBrowserCache(options.DaysOld);
                    totalSpaceFreed += browserResult.SpaceFreed;
                    totalFilesCleaned += browserResult.FilesCleaned;
                    cleanedFiles.AddRange(browserResult.CleanedFiles);
                }

                if (options.CleanLogFiles)
                {
                    var logResult = CleanLogFiles(options.DaysOld);
                    totalSpaceFreed += logResult.SpaceFreed;
                    totalFilesCleaned += logResult.FilesCleaned;
                    cleanedFiles.AddRange(logResult.CleanedFiles);
                }

                if (options.CleanThumbnails)
                {
                    var thumbResult = CleanThumbnails();
                    totalSpaceFreed += thumbResult.SpaceFreed;
                    totalFilesCleaned += thumbResult.FilesCleaned;
                    cleanedFiles.AddRange(thumbResult.CleanedFiles);
                }

                result.Success = true;
                result.Message = $"Cleanup completed successfully! Freed {FormatFileSize(totalSpaceFreed)} by removing {totalFilesCleaned} files.";
                result.SpaceFreed = totalSpaceFreed;
                result.FilesCleaned = totalFilesCleaned;
                result.CleanedFiles = cleanedFiles;

                // Force garbage collection to ensure freed space is returned to the system
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Cleanup failed: {ex.Message}";
            }

            return result;
        }

        private CleanupResult CleanTempFiles(int daysOld)
        {
            var result = new CleanupResult();
            var tempPaths = new List<string>
            {
                Path.GetTempPath(),
                Environment.GetEnvironmentVariable("TEMP") ?? "",
                Environment.GetEnvironmentVariable("TMP") ?? "",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")
            };

            // Remove duplicates and invalid paths
            tempPaths = tempPaths.Where(path => !string.IsNullOrEmpty(path) && Directory.Exists(path)).Distinct().ToList();

            long spaceFreed = 0;
            int filesCleaned = 0;
            var cleanedFiles = new List<string>();

            foreach (var tempPath in tempPaths)
            {
                try
                {
                    var tempDir = new DirectoryInfo(tempPath);
                    var files = tempDir.GetFiles("*", SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        try
                        {
                            if (file.CreationTime < DateTime.Now.AddDays(-daysOld))
                            {
                                var fileSize = file.Length;
                                file.Delete();
                                spaceFreed += fileSize;
                                filesCleaned++;
                                cleanedFiles.Add(file.FullName);
                            }
                        }
                        catch
                        {
                            // Skip files that can't be deleted
                        }
                    }
                }
                catch
                {
                    // Skip directories that can't be accessed
                }
            }

            result.SpaceFreed = spaceFreed;
            result.FilesCleaned = filesCleaned;
            result.CleanedFiles = cleanedFiles;
            return result;
        }

        private CleanupResult CleanBrowserCache(int daysOld)
        {
            var result = new CleanupResult();
            var browserCachePaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Cache2"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Default", "Cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "INetCache")
            };

            long spaceFreed = 0;
            int filesCleaned = 0;
            var cleanedFiles = new List<string>();

            foreach (var cachePath in browserCachePaths)
            {
                if (!Directory.Exists(cachePath)) continue;

                try
                {
                    var cacheDir = new DirectoryInfo(cachePath);
                    var files = cacheDir.GetFiles("*", SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        try
                        {
                            if (file.CreationTime < DateTime.Now.AddDays(-daysOld))
                            {
                                var fileSize = file.Length;
                                file.Delete();
                                spaceFreed += fileSize;
                                filesCleaned++;
                                cleanedFiles.Add(file.FullName);
                            }
                        }
                        catch
                        {
                            // Skip files that can't be deleted
                        }
                    }
                }
                catch
                {
                    // Skip directories that can't be accessed
                }
            }

            // Special handling for Firefox profiles
            var firefoxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles");
            if (Directory.Exists(firefoxPath))
            {
                try
                {
                    var profileDirs = Directory.GetDirectories(firefoxPath);
                    foreach (var profileDir in profileDirs)
                    {
                        var cacheDir = Path.Combine(profileDir, "cache2");
                        if (Directory.Exists(cacheDir))
                        {
                            var cacheFiles = Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories);
                            foreach (var file in cacheFiles)
                            {
                                try
                                {
                                    var fileInfo = new FileInfo(file);
                                    if (fileInfo.CreationTime < DateTime.Now.AddDays(-daysOld))
                                    {
                                        var fileSize = fileInfo.Length;
                                        fileInfo.Delete();
                                        spaceFreed += fileSize;
                                        filesCleaned++;
                                        cleanedFiles.Add(fileInfo.FullName);
                                    }
                                }
                                catch
                                {
                                    // Skip files that can't be deleted
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Skip if Firefox profiles can't be accessed
                }
            }

            result.SpaceFreed = spaceFreed;
            result.FilesCleaned = filesCleaned;
            result.CleanedFiles = cleanedFiles;
            return result;
        }

        private CleanupResult CleanLogFiles(int daysOld)
        {
            var result = new CleanupResult();
            var logPaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Windows", "AppModelLog"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "WER")
            };

            long spaceFreed = 0;
            int filesCleaned = 0;
            var cleanedFiles = new List<string>();

            foreach (var logPath in logPaths)
            {
                if (!Directory.Exists(logPath)) continue;

                try
                {
                    var logDir = new DirectoryInfo(logPath);
                    var logFiles = logDir.GetFiles("*.*log*", SearchOption.AllDirectories);

                    foreach (var file in logFiles)
                    {
                        try
                        {
                            if (file.CreationTime < DateTime.Now.AddDays(-daysOld))
                            {
                                var fileSize = file.Length;
                                file.Delete();
                                spaceFreed += fileSize;
                                filesCleaned++;
                                cleanedFiles.Add(file.FullName);
                            }
                        }
                        catch
                        {
                            // Skip files that can't be deleted
                        }
                    }
                }
                catch
                {
                    // Skip directories that can't be accessed
                }
            }

            result.SpaceFreed = spaceFreed;
            result.FilesCleaned = filesCleaned;
            result.CleanedFiles = cleanedFiles;
            return result;
        }

        private CleanupResult CleanThumbnails()
        {
            var result = new CleanupResult();
            var thumbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Explorer");

            long spaceFreed = 0;
            int filesCleaned = 0;
            var cleanedFiles = new List<string>();

            if (Directory.Exists(thumbPath))
            {
                try
                {
                    var thumbDir = new DirectoryInfo(thumbPath);
                    var thumbFiles = thumbDir.GetFiles("thumbcache_*.db");

                    foreach (var file in thumbFiles)
                    {
                        try
                        {
                            var fileSize = file.Length;
                            file.Delete();
                            spaceFreed += fileSize;
                            filesCleaned++;
                            cleanedFiles.Add(file.FullName);
                        }
                        catch
                        {
                            // Skip files that can't be deleted
                        }
                    }
                }
                catch
                {
                    // Skip if thumbnail cache can't be accessed
                }
            }

            result.SpaceFreed = spaceFreed;
            result.FilesCleaned = filesCleaned;
            result.CleanedFiles = cleanedFiles;
            return result;
        }

        public async Task<CleanupResult> DeepCleanupAsync()
        {
            var options = new CleanupOptions
            {
                CleanTempFiles = true,
                CleanBrowserCache = true,
                CleanLogFiles = true,
                CleanThumbnails = true,
                CleanOldUpdates = true,
                DaysOld = 30
            };

            return await PerformCleanupAsync(options);
        }

        public async Task<CleanupResult> QuickCleanupAsync()
        {
            var options = new CleanupOptions
            {
                CleanTempFiles = true,
                CleanBrowserCache = true,
                CleanThumbnails = true,
                DaysOld = 7
            };

            return await PerformCleanupAsync(options);
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }

        public long GetPotentialCleanupSize(CleanupOptions options = null)
        {
            options = options ?? new CleanupOptions();
            long totalSize = 0;

            if (options.CleanTempFiles)
            {
                totalSize += GetTempFilesSize(options.DaysOld);
            }

            if (options.CleanBrowserCache)
            {
                totalSize += GetBrowserCacheSize(options.DaysOld);
            }

            if (options.CleanLogFiles)
            {
                totalSize += GetLogFilesSize(options.DaysOld);
            }

            if (options.CleanThumbnails)
            {
                totalSize += GetThumbnailsSize();
            }

            return totalSize;
        }

        private long GetTempFilesSize(int daysOld)
        {
            var tempPaths = new List<string>
            {
                Path.GetTempPath(),
                Environment.GetEnvironmentVariable("TEMP") ?? "",
                Environment.GetEnvironmentVariable("TMP") ?? "",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")
            };

            tempPaths = tempPaths.Where(path => !string.IsNullOrEmpty(path) && Directory.Exists(path)).Distinct().ToList();

            long totalSize = 0;

            foreach (var tempPath in tempPaths)
            {
                try
                {
                    var tempDir = new DirectoryInfo(tempPath);
                    var files = tempDir.GetFiles("*", SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        try
                        {
                            if (file.CreationTime < DateTime.Now.AddDays(-daysOld))
                            {
                                totalSize += file.Length;
                            }
                        }
                        catch
                        {
                            // Skip files that can't be accessed
                        }
                    }
                }
                catch
                {
                    // Skip directories that can't be accessed
                }
            }

            return totalSize;
        }

        private long GetBrowserCacheSize(int daysOld)
        {
            var browserCachePaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Cache2"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Default", "Cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "INetCache")
            };

            long totalSize = 0;

            foreach (var cachePath in browserCachePaths)
            {
                if (!Directory.Exists(cachePath)) continue;

                try
                {
                    var cacheDir = new DirectoryInfo(cachePath);
                    var files = cacheDir.GetFiles("*", SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        try
                        {
                            if (file.CreationTime < DateTime.Now.AddDays(-daysOld))
                            {
                                totalSize += file.Length;
                            }
                        }
                        catch
                        {
                            // Skip files that can't be accessed
                        }
                    }
                }
                catch
                {
                    // Skip directories that can't be accessed
                }
            }

            // Special handling for Firefox profiles
            var firefoxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles");
            if (Directory.Exists(firefoxPath))
            {
                try
                {
                    var profileDirs = Directory.GetDirectories(firefoxPath);
                    foreach (var profileDir in profileDirs)
                    {
                        var cacheDir = Path.Combine(profileDir, "cache2");
                        if (Directory.Exists(cacheDir))
                        {
                            var cacheFiles = Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories);
                            foreach (var file in cacheFiles)
                            {
                                try
                                {
                                    var fileInfo = new FileInfo(file);
                                    if (fileInfo.CreationTime < DateTime.Now.AddDays(-daysOld))
                                    {
                                        totalSize += fileInfo.Length;
                                    }
                                }
                                catch
                                {
                                    // Skip files that can't be accessed
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Skip if Firefox profiles can't be accessed
                }
            }

            return totalSize;
        }

        private long GetLogFilesSize(int daysOld)
        {
            var logPaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Windows", "AppModelLog"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "WER")
            };

            long totalSize = 0;

            foreach (var logPath in logPaths)
            {
                if (!Directory.Exists(logPath)) continue;

                try
                {
                    var logDir = new DirectoryInfo(logPath);
                    var logFiles = logDir.GetFiles("*.*log*", SearchOption.AllDirectories);

                    foreach (var file in logFiles)
                    {
                        try
                        {
                            if (file.CreationTime < DateTime.Now.AddDays(-daysOld))
                            {
                                totalSize += file.Length;
                            }
                        }
                        catch
                        {
                            // Skip files that can't be accessed
                        }
                    }
                }
                catch
                {
                    // Skip directories that can't be accessed
                }
            }

            return totalSize;
        }

        private long GetThumbnailsSize()
        {
            var thumbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Explorer");
            long totalSize = 0;

            if (Directory.Exists(thumbPath))
            {
                try
                {
                    var thumbDir = new DirectoryInfo(thumbPath);
                    var thumbFiles = thumbDir.GetFiles("thumbcache_*.db");

                    foreach (var file in thumbFiles)
                    {
                        try
                        {
                            totalSize += file.Length;
                        }
                        catch
                        {
                            // Skip files that can't be accessed
                        }
                    }
                }
                catch
                {
                    // Skip if thumbnail cache can't be accessed
                }
            }

            return totalSize;
        }
    }
}
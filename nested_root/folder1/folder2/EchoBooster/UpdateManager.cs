using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Text;

namespace EchoBooster
{
    // Data structure to represent a file in the repository
    public class RepositoryFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Sha { get; set; }
        public long Size { get; set; }
        public string Url { get; set; }
        public string DownloadUrl { get; set; }
    }

    // Data structure to represent the commit information
    public class CommitInfo
    {
        public string Sha { get; set; }
        public CommitDetails Commit { get; set; }
    }

    public class CommitDetails
    {
        public AuthorInfo Author { get; set; }
        public string Message { get; set; }
    }

    public class AuthorInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Date { get; set; }
    }

    public class UpdateManager
    {
        private readonly string _repositoryUrl = "https://api.github.com/repos/aidandelange170-hub/Echo";
        private readonly HttpClient _httpClient;
        private readonly MainWindow _mainWindow;
        private readonly string _localFilesPath;
        private readonly string _updateCachePath;

        public UpdateManager(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "EchoBooster-App");
            _localFilesPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Environment.CurrentDirectory;
            _updateCachePath = Path.Combine(Path.GetTempPath(), "EchoBoosterUpdates");
            
            // Create update cache directory if it doesn't exist
            if (!Directory.Exists(_updateCachePath))
            {
                Directory.CreateDirectory(_updateCachePath);
            }
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                // Get the latest commit information from the repository
                var latestCommit = await GetLatestCommitInfoAsync();
                if (string.IsNullOrEmpty(latestCommit?.Sha))
                {
                    return false;
                }

                // Get all files in the repository
                var repositoryFiles = await GetRepositoryFilesAsync();
                
                // Compare with local files to detect changes
                var updatedFiles = await FindUpdatedFilesAsync(repositoryFiles);
                
                // If there are updated files, cache them for download
                if (updatedFiles.Any())
                {
                    await CacheUpdatedFilesAsync(updatedFiles);
                    return true; // Updates are available
                }
                
                return false; // No updates available
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.StatusText.Text = $"Update check failed: {ex.Message}";
                });
                return false;
            }
        }

        public async Task<bool> DownloadAndInstallUpdateAsync()
        {
            try
            {
                // Show update notification in the UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.StatusText.Text = "Installing update...";
                });

                // Check if there are cached updated files to install
                var updateCacheDir = new DirectoryInfo(_updateCachePath);
                var cachedFiles = updateCacheDir.GetFiles("*", SearchOption.AllDirectories);
                
                if (cachedFiles.Length == 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.StatusText.Text = "No updates to install.";
                    });
                    return false;
                }

                // Install the cached updates
                foreach (var cachedFile in cachedFiles)
                {
                    var relativePath = GetRelativePath(cachedFile.FullName, _updateCachePath);
                    var targetPath = Path.Combine(_localFilesPath, relativePath);
                    
                    // Ensure the target directory exists
                    var targetDir = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }
                    
                    // Copy the cached file to the target location
                    File.Copy(cachedFile.FullName, targetPath, true);
                }

                // Clear the update cache after installation
                foreach (var cachedFile in cachedFiles)
                {
                    cachedFile.Delete();
                }

                // Show update complete message
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.StatusText.Text = "Update installed successfully! Restarting application...";
                });

                // Restart the application to apply updates
                RestartApplication();

                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.StatusText.Text = $"Update failed: {ex.Message}";
                });
                return false;
            }
        }

        public async Task<bool> CheckForCachedUpdatesAsync()
        {
            try
            {
                // Check if there are cached updated files to install from a previous session
                var updateCacheDir = new DirectoryInfo(_updateCachePath);
                var cachedFiles = updateCacheDir.GetFiles("*", SearchOption.AllDirectories);
                
                if (cachedFiles.Length > 0)
                {
                    // There are cached updates available, install them automatically
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.StatusText.Text = "Installing cached updates...";
                    });

                    // Install the cached updates
                    foreach (var cachedFile in cachedFiles)
                    {
                        var relativePath = GetRelativePath(cachedFile.FullName, _updateCachePath);
                        var targetPath = Path.Combine(_localFilesPath, relativePath);
                        
                        // Ensure the target directory exists
                        var targetDir = Path.GetDirectoryName(targetPath);
                        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                        
                        // Copy the cached file to the target location
                        File.Copy(cachedFile.FullName, targetPath, true);
                    }

                    // Clear the update cache after installation
                    foreach (var cachedFile in cachedFiles)
                    {
                        cachedFile.Delete();
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.StatusText.Text = "Cached updates installed successfully!";
                    });
                    
                    return true;
                }
                
                return false; // No cached updates to install
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.StatusText.Text = $"Cached update installation failed: {ex.Message}";
                });
                return false;
            }
        }

        public async Task StartBackgroundUpdateCheckingAsync(TimeSpan interval)
        {
            while (true)
            {
                try
                {
                    await Task.Delay(interval);
                    
                    // Check for updates in the background
                    bool updateAvailable = await CheckForUpdatesAsync();
                    
                    if (updateAvailable)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _mainWindow.StatusText.Text = "New update downloaded in background! Restart to apply.";
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue the background checking
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.StatusText.Text = $"Background update check error: {ex.Message}";
                    });
                    // Wait a bit before continuing to avoid rapid error loops
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }

        private void RestartApplication()
        {
            try
            {
                // Get the current executable path
                string exePath = Assembly.GetEntryAssembly()?.Location ?? Process.GetCurrentProcess().MainModule?.FileName;

                if (!string.IsNullOrEmpty(exePath))
                {
                    // Start a new instance of the application
                    Process.Start(exePath);
                }
                else
                {
                    // Fallback: restart using the current process
                    Process.Start(Application.ResourceAssembly.Location);
                }

                // Close the current instance
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.StatusText.Text = $"Restart failed: {ex.Message}";
                });
            }
        }

        private async Task<CommitInfo> GetLatestCommitInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{_repositoryUrl}/commits/main");
                var commitInfo = JsonSerializer.Deserialize<CommitInfo>(response);
                return commitInfo;
            }
            catch
            {
                // Try with master branch as fallback
                try
                {
                    var response = await _httpClient.GetStringAsync($"{_repositoryUrl}/commits/master");
                    var commitInfo = JsonSerializer.Deserialize<CommitInfo>(response);
                    return commitInfo;
                }
                catch
                {
                    return null;
                }
            }
        }

        private async Task<List<RepositoryFile>> GetRepositoryFilesAsync()
        {
            var files = new List<RepositoryFile>();
            
            try
            {
                // Get the tree of all files in the repository
                var response = await _httpClient.GetStringAsync($"{_repositoryUrl}/git/trees/main?recursive=1");
                var doc = JsonDocument.Parse(response);
                
                if (doc.RootElement.TryGetProperty("tree", out var treeElement))
                {
                    foreach (var item in treeElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("type", out var typeElement) && 
                            typeElement.GetString() == "blob") // Only files, not directories
                        {
                            var file = new RepositoryFile
                            {
                                Path = item.GetProperty("path").GetString(),
                                Sha = item.GetProperty("sha").GetString(),
                                Size = item.GetProperty("size").GetInt64(),
                                Url = item.GetProperty("url").GetString()
                            };
                            
                            // Construct download URL for the file
                            file.DownloadUrl = $"https://raw.githubusercontent.com/aidandelange170-hub/Echo/main/{file.Path}";
                            files.Add(file);
                        }
                    }
                }
            }
            catch
            {
                // Try with master branch as fallback
                try
                {
                    var response = await _httpClient.GetStringAsync($"{_repositoryUrl}/git/trees/master?recursive=1");
                    var doc = JsonDocument.Parse(response);
                    
                    if (doc.RootElement.TryGetProperty("tree", out var treeElement))
                    {
                        foreach (var item in treeElement.EnumerateArray())
                        {
                            if (item.TryGetProperty("type", out var typeElement) && 
                                typeElement.GetString() == "blob") // Only files, not directories
                            {
                                var file = new RepositoryFile
                                {
                                    Path = item.GetProperty("path").GetString(),
                                    Sha = item.GetProperty("sha").GetString(),
                                    Size = item.GetProperty("size").GetInt64(),
                                    Url = item.GetProperty("url").GetString()
                                };
                                
                                // Construct download URL for the file
                                file.DownloadUrl = $"https://raw.githubusercontent.com/aidandelange170-hub/Echo/master/{file.Path}";
                                files.Add(file);
                            }
                        }
                    }
                }
                catch
                {
                    // If both attempts fail, return empty list
                }
            }
            
            return files;
        }

        private async Task<List<RepositoryFile>> FindUpdatedFilesAsync(List<RepositoryFile> repositoryFiles)
        {
            var updatedFiles = new List<RepositoryFile>();
            
            foreach (var repoFile in repositoryFiles)
            {
                var localPath = Path.Combine(_localFilesPath, repoFile.Path);
                
                // Check if the local file exists
                if (!File.Exists(localPath))
                {
                    // File doesn't exist locally, needs to be downloaded
                    updatedFiles.Add(repoFile);
                    continue;
                }
                
                // Calculate the SHA of the local file
                var localFileSha = await CalculateFileSha256Async(localPath);
                
                // Compare with the repository file's SHA
                if (!string.Equals(localFileSha, repoFile.Sha, StringComparison.OrdinalIgnoreCase))
                {
                    // File has been updated in the repository
                    updatedFiles.Add(repoFile);
                }
            }
            
            return updatedFiles;
        }

        private async Task<string> CalculateFileSha256Async(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private async Task CacheUpdatedFilesAsync(List<RepositoryFile> updatedFiles)
        {
            foreach (var file in updatedFiles)
            {
                try
                {
                    // Download the file content
                    var fileContent = await _httpClient.GetStringAsync(file.DownloadUrl);
                    
                    // Create the target path in the update cache
                    var cachePath = Path.Combine(_updateCachePath, file.Path);
                    
                    // Ensure the target directory exists
                    var targetDir = Path.GetDirectoryName(cachePath);
                    if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }
                    
                    // Write the file content to the cache
                    await File.WriteAllTextAsync(cachePath, fileContent, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.StatusText.Text = $"Failed to cache file {file.Path}: {ex.Message}";
                    });
                }
            }
        }

        private string GetRelativePath(string filePath, string basePath)
        {
            var fileUri = new Uri(filePath);
            var baseUri = new Uri(basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) 
                ? basePath 
                : basePath + Path.DirectorySeparatorChar);
            
            var relativeUri = baseUri.MakeRelativeUri(fileUri);
            return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        public void CheckForUpdatesWithNotification()
        {
            Task.Run(async () =>
            {
                bool updateAvailable = await CheckForUpdatesAsync();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (updateAvailable)
                    {
                        _mainWindow.StatusText.Text = "New update available! Click here to update.";
                        
                        // Add click handler to status text for update
                        var statusText = _mainWindow.StatusText;
                        statusText.TextDecorations = System.Windows.TextDecorations.Underline;
                        
                        // In a real implementation, you would add an event handler for clicks
                        // For now, we'll just show a message
                        MessageBoxResult result = MessageBox.Show(
                            "A new version of EchoBooster is available. Would you like to update now?",
                            "Update Available",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            _ = DownloadAndInstallUpdateAsync();
                        }
                    }
                });
            });
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
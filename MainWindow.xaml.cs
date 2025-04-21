using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using OpenNISharp2;
using System.Text.RegularExpressions;
using IFLEGameLauncher.Model;
using Newtonsoft.Json.Linq;

namespace IFLEGameLauncher
{
    public partial class MainWindow : Window
    {
        private string floorId;
        private string selectedDownloadFolder;
        private string guid;
        private List<Game> games = new List<Game>();
        private static string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public MainWindow(string selectedFloorId)
        {
            InitializeComponent();
            floorId = selectedFloorId;
            LoadGameData();
            selectedDownloadFolder = LoadDownloadPath();
            DownloadPathText.Text = selectedDownloadFolder;
            CheckDeviceUri();
        }
        private static string LoadDownloadPath()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    var settings = JsonConvert.DeserializeObject<Settings>(json);

                    if (!string.IsNullOrEmpty(settings?.DownloadFolder) && Directory.Exists(settings.DownloadFolder))
                    {
                        return settings.DownloadFolder;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // If no valid path is found, return the default
            return GetDefaultDownloadPath();
        }

        private static string GetDefaultDownloadPath()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string gamesFolder = Path.Combine(desktopPath, "IFLE", "Games");


            // Create directories if they don’t exist yet !@
            if (!Directory.Exists(gamesFolder))
                Directory.CreateDirectory(gamesFolder);

            return gamesFolder;
        }
        private async void LoadGameData()
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(App.AccessToken);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(claim =>
                    claim.Type == "userId" || claim.Type.EndsWith("/identity/claims/nameidentifier"));

                string? userId = userIdClaim?.Value;
                App.UserId = userId;

                if (string.IsNullOrEmpty(userId))
                {
                    Debug.WriteLine("Failed to get user ID from access token.");
                    return;
                }

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", App.AccessToken);

                    //string apiUrl = $"http://160.187.240.95:8080/api/game/user/{userId}/purchased";
                    string apiUrl = $"https://localhost:7174/api/floors/{floorId}/game-package/playable";

                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    string responseData = await response.Content.ReadAsStringAsync();
                    var packages = JsonConvert.DeserializeObject<List<PlayablePackage>>(responseData);

                    var gameList = packages?
                                    .SelectMany(p => p.GamePackageInfo.GameList)
                                    .ToList();

                    if (gameList == null)
                    {
                        MessageBox.Show("No games found for the selected floor.");
                        return;
                    }
                    games = gameList;    

                    GameListBox.Items.Clear();
                    GameListBox.ItemsSource = games.Select(g => g.Title).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching game data: {ex.Message}");
            }
        }

        private void GameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayButton.Visibility = Visibility.Collapsed;
            UninstallButton.Visibility = Visibility.Collapsed;
            DownloadButton.Visibility = Visibility.Collapsed;
            UpdateButton.Visibility = Visibility.Collapsed;

            if (GameListBox.SelectedItem is string selectedGameTitle)
            {
                var selectedGame = games.FirstOrDefault(g => g.Title == selectedGameTitle);
                App.GameId = selectedGame.Id;
                if (selectedGame != null)
                {
                    
                    //GameImage.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(selectedGame.ImageUrl)));
                   // GameImage.Source = new BitmapImage(new Uri(selectedGame.ImageUrl, UriKind.Absolute));
                    if (!string.IsNullOrEmpty(selectedGame.ImageUrl))
                    {
                        GameImage.Source = new BitmapImage(new Uri(selectedGame.ImageUrl, UriKind.Absolute));
                    }
                    else
                    {
                        GameImage.Source = null;
                    }
                    string versionInfo = string.Join("\n", selectedGame.Versions.Select(v =>
                        $"Version: {v.Version} ({v.VersionDate:yyyy-MM-dd})"
                    ));
                    GameDescription.Text = selectedGame.Description + "\n" + versionInfo;

                    string gameFolder = Path.Combine(selectedDownloadFolder, selectedGameTitle.Replace(" ", ""));
                    string localVersion = GetLocalGameVersion(gameFolder);
                    //MessageBox.Show(localVersion);
                    string latestVersion = selectedGame.Versions.OrderByDescending(v => v.VersionDate).FirstOrDefault()?.Version ?? "1.0";

                    if (Directory.Exists(gameFolder))
                    {
                        PlayButton.Visibility = Visibility.Visible;
                        UninstallButton.Visibility = Visibility.Visible;

                        if (localVersion != null && localVersion != latestVersion)
                        {
                            UpdateButton.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        DownloadButton.Visibility = Visibility.Visible;
                    }

                }
                else
                {
                    GameDescription.Text = "Select a game to see details.";
                }
            }
        }

        private async void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            bool checkFloorDevice = await CheckFloorDeviceUri();
            if (checkFloorDevice != true) {
                return;
            }

            if (string.IsNullOrEmpty(selectedDownloadFolder))
            {
                MessageBox.Show("Please choose a download folder first!", "No Folder Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GameListBox.SelectedItem is string gameName)
            {
                //MessageBox.Show($"{gameName} selected!"); 

                //string gameName = selectedItem.Content.ToString();
                var selectedGame = games.FirstOrDefault(g => g.Title == gameName);
                string gameFolder = Path.Combine(selectedDownloadFolder, gameName.Replace(" ", ""));

                string exePath = FindGameExecutable(gameFolder);

                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                   await UpdateGamePlayCount(selectedGame.Id);

                    string arguments = $"--userId={App.UserId} --floorId={App.FloorId} --gameId={App.GameId}";

                    ProcessStartInfo startInfo = new ProcessStartInfo(exePath, arguments);
                    Process.Start(startInfo);
                }
                else
                {
                    MessageBox.Show("Game not found! Please download it first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    DownloadButton.Visibility = Visibility.Visible;
                    UninstallButton.Visibility = Visibility.Hidden;
                    PlayButton.Visibility = Visibility.Hidden;
                }
            }
        }

        private string FindGameExecutable(string gameFolder)
        {
            if (Directory.Exists(gameFolder))
            {
                string[] exeFiles = Directory.GetFiles(gameFolder, "*.exe", SearchOption.AllDirectories);
                return exeFiles.Length > 0 ? exeFiles[0] : null;
            }
            return null;
        }

        private async void DownloadGame_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedDownloadFolder))
            {
                MessageBox.Show("Please choose a download folder first!", "No Folder Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GameListBox.SelectedItem is string gameName)
            {
                //string gameName = selectedItem.Content.ToString();
                var selectedGame = games.FirstOrDefault(g => g.Title == gameName);
                string downloadUrl = selectedGame.DownloadUrl;
                string gameFolder = Path.Combine(selectedDownloadFolder, gameName.Replace(" ", ""));
                string latestVersion = selectedGame.Versions.OrderByDescending(v => v.VersionDate).FirstOrDefault()?.Version;

                //MessageBox.Show($"Game will be download to: {selectedDownloadFolder}");

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    MessageBox.Show("Download URL not found for the selected game.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await DownloadAndExtractGame(gameName, downloadUrl, gameFolder, latestVersion);
                GameListBox_SelectionChanged(null, null);
            }
        }

        private async Task DownloadAndExtractGame(string gameName, string downloadUrl, string gameFolder, string latestVersion)
        {
            try
            {
                if (!Directory.Exists(gameFolder))
                {
                    Directory.CreateDirectory(gameFolder);
                }

                string zipPath = Path.Combine(gameFolder, gameName + ".zip");

                // Progress Window pop up
                DownloadProgressWindow progressWindow = new DownloadProgressWindow();
                progressWindow.Show();

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();

                    long? totalBytes = response.Content.Headers.ContentLength;
                    byte[] buffer = new byte[8192];
                    long totalRead = 0;
                    int readBytes;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        while ((readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, readBytes);
                            totalRead += readBytes;

                            // Update Progress
                            if (totalBytes.HasValue)
                            {
                                int progressPercentage = (int)((totalRead * 100) / totalBytes.Value);
                                progressWindow.UpdateProgress(progressPercentage);
                            }
                        }
                    }
                }

                // Close Progress Window
                progressWindow.Close();

                // Extract ZIP
                ExtractZipFile(zipPath, gameFolder);
                File.Delete(zipPath);
                File.WriteAllText(Path.Combine(gameFolder, "version.txt"), latestVersion);

                MessageBox.Show($"{gameName} downloaded and extracted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download {gameName}. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExtractZipFile(string zipFilePath, string destinationPath)
        {
            try
            {
                using (FileStream fs = File.OpenRead(zipFilePath))
                using (ICSharpCode.SharpZipLib.Zip.ZipFile zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(fs))
                {
                    foreach (ZipEntry entry in zipFile)
                    {
                        if (!entry.IsFile)
                            continue; // Skip directories

                        string entryPath = Path.Combine(destinationPath, entry.Name);
                        Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                        using (Stream zipStream = zipFile.GetInputStream(entry))
                        using (FileStream outputStream = File.Create(entryPath))
                        {
                            zipStream.CopyTo(outputStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting ZIP: {ex.Message}", "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChooseDownloadLocation()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true, // Enables folder selection mode
                Title = "Select a Folder to Download Games",
                InitialDirectory = selectedDownloadFolder,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedDownloadFolder = dialog.FileName;
                SaveDownloadPath(selectedDownloadFolder);
                MessageBox.Show($"Download folder set to:\n{selectedDownloadFolder}", "Download Location", MessageBoxButton.OK, MessageBoxImage.Information);
                DownloadPathText.Text = $"{selectedDownloadFolder}";
            }
        }

        private void ChooseDownloadLocation_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show($"Current location: { selectedDownloadFolder}");
            ChooseDownloadLocation();
        }

        private static void SaveDownloadPath(string path)
        {
            try
            {
                var settings = new Settings { DownloadFolder = path };
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

                //string message = $"Saving to: {settingsFilePath}\n\nJSON Content:\n{json}";
                //MessageBox.Show(message, "Saving Settings", MessageBoxButton.OK, MessageBoxImage.Information);

                File.WriteAllText(settingsFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UninstallGame_Click(object sender, RoutedEventArgs e)
        {
            if (GameListBox.SelectedItem is string gameName)
            {
                string gameFolder = Path.Combine(selectedDownloadFolder, gameName.Replace(" ", ""));

                if (Directory.Exists(gameFolder))
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"Are you sure you want to uninstall {gameName}?",
                        "Confirm Uninstall",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Directory.Delete(gameFolder, true);
                            MessageBox.Show($"{gameName} has been uninstalled.", "Uninstall Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                            //UninstallButton.Visibility = Visibility.Collapsed;
                            GameListBox_SelectionChanged(null, null);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to uninstall {gameName}. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Game is not installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    UninstallButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        // get local version from txt file
        private string GetLocalGameVersion(string gameFolder)
        {
            string versionPath = Path.Combine(gameFolder, "version.txt");
            if (File.Exists(versionPath))
            {
                return File.ReadAllText(versionPath).Trim();
            }
            return null;
        }

        private async Task UpdateGamePlayCount(string gameId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", App.AccessToken);

                    string apiUrl = $"https://localhost:7174/api/game/update-game-count/{gameId}";

                    var response = await client.PutAsync(apiUrl, null);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Game play count updated for {gameId}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to update play count. Status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating play count: " + ex.Message);
            }
        }

        private void DeviceCheck_Click(object sender, RoutedEventArgs e)
        {
            CheckDeviceUri();
        }

        public void CheckDeviceUri ()
        {
            try
            {
                OpenNI.Initialize();

                var devices = OpenNI.GetDevices();



                var device = devices.FirstOrDefault();
                string uri = device.Uri;
                var match = Regex.Match(uri, @"{(?<guid>[0-9a-fA-F\-]{36})}");

                if (match.Success)
                {
                    guid = match.Groups["guid"].Value;
                    DeviceGUID.Text = guid;
                }
                else
                {
                    Console.WriteLine("Không tìm thấy GUID.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing OpenNI2: {ex.Message}");
            }
        }

        public async Task<bool> CheckFloorDeviceUri () 
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", App.AccessToken);

                    string apiUrl = $"https://localhost:7174/api/floors/{floorId}";

                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    string responseData = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(responseData);
                    string? uri = json["deviceInfo"]?["uri"]?.ToString();

                    if (!string.IsNullOrEmpty(uri))
                    {
                        if (uri == guid)
                        {
                            {
                                return true;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Your device number is not matching the one registered for this floor !");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting device: {ex.Message}");
                return false;
            }
            return false;
        }
    }
}

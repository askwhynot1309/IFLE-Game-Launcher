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

namespace IFLEGameLauncher
{
    public partial class MainWindow : Window
    {
        private string selectedDownloadFolder;
        private List<Game> games = new List<Game>();
        private static string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public MainWindow()
        {
            InitializeComponent();
            LoadGameData();
            selectedDownloadFolder = LoadDownloadPath();
            DownloadPathText.Text = selectedDownloadFolder;
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
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = "https://localhost:7000/api/get-game-info";

                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    string responseData = await response.Content.ReadAsStringAsync();

                    games = JsonConvert.DeserializeObject<List<Game>>(responseData);

                    GameListBox.Items.Clear();
                    GameListBox.ItemsSource = games.Select(g => g.Title).ToList();
                    //foreach (var game in games)
                    //{
                    //    GameListBox.Items.Add(game.Title);
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching game data: {ex.Message}");
            }
        }

        private void GameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListBox.SelectedItem is string selectedGameTitle)
            {
                var selectedGame = games.FirstOrDefault(g => g.Title == selectedGameTitle);
                if (selectedGame != null)
                {
                    GameImage.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("Images/balloon.jpg")));
                    GameDescription.Text = selectedGame.Description;
                }
                else
                {
                    GameDescription.Text = "Select a game to see details.";
                }
            }
        }

        private void UpdateGameDetails(string gameName)
        {
            var selectedGame = games.FirstOrDefault(g => g.Title == gameName);
            if (selectedGame != null)
            {
                string imagePath = Path.Combine("Images", $"{gameName.Replace(" ", "").ToLower()}.jpg");
                if (File.Exists(imagePath))
                {
                    GameImage.Source = new BitmapImage(new Uri(Path.GetFullPath(imagePath)));
                }
                else
                {
                    GameImage.Source = null;
                }

                GameDescription.Text = selectedGame.Description;
            }
            else
            {
                GameImage.Source = null;
                GameDescription.Text = "Select a game to see details.";
            }
        }

        private void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedDownloadFolder))
            {
                MessageBox.Show("Please choose a download folder first!", "No Folder Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GameListBox.SelectedItem is string gameName)
            {
                MessageBox.Show($"{gameName} selected!"); 

                //string gameName = selectedItem.Content.ToString();
                string gameFolder = Path.Combine(selectedDownloadFolder, gameName.Replace(" ", ""));

                string exePath = FindGameExecutable(gameFolder);

                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    Process.Start(exePath);
                }
                else
                {
                    MessageBox.Show("Game not found! Please download it first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                MessageBox.Show($"Game will be download to: {selectedDownloadFolder}");
                
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    MessageBox.Show("Download URL not found for the selected game.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await DownloadAndExtractGame(gameName, downloadUrl, gameFolder);
            }
        }


        //test download API
        private string GetDownloadUrl(string gameName)
        {
            return gameName switch
            {
                "Math Game" => "https://localhost:7000/api/downloadMathGame",
                "English Game" => "https://localhost:7000/api/downloadEnglishGame",
                "Run Game" => "https://localhost:7000/api/downloadRunGame",
                "Balloon Pop Game" => "https://localhost:7000/api/downloadBalloonGame",
                _ => null
            };
        }

        private async Task DownloadAndExtractGame(string gameName, string downloadUrl, string gameFolder)
        {
            try
            {
                if (!Directory.Exists(gameFolder))
                {
                    Directory.CreateDirectory(gameFolder);
                }

                string zipPath = Path.Combine(gameFolder, gameName + ".zip");

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();

                    //if (!response.Content.Headers.ContentType.MediaType.Contains("zip"))
                    //{
                    //    MessageBox.Show($"The downloaded file is not a valid ZIP. Check the API response.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //    return;
                    //}

                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(zipPath, fileBytes);
                }

                // Extract ZIP
                ExtractZipFile(zipPath, gameFolder);
                File.Delete(zipPath); 

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

        class Settings
        {
            public string DownloadFolder { get; set; }
        }
    }
}

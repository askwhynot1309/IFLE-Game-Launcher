﻿using System;
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
        private static string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public MainWindow()
        {
            InitializeComponent();
            selectedDownloadFolder = LoadDownloadPath();
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


        private void GameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListBox.SelectedItem is ListBoxItem selectedItem)
            {
                string gameName = selectedItem.Content.ToString();
                UpdateGameDetails(gameName);
            }
        }

        private void UpdateGameDetails(string gameName)
        {
            string path;
            switch (gameName)
            {
                case "Math Game":
                    path = System.IO.Path.GetFullPath("Images/math.jpg");
                    GameImage.Source = new BitmapImage(new Uri(path));
                    GameDescription.Text = "Solve math equations and improve your skills!";
                    break;

                case "English Game":
                    path = System.IO.Path.GetFullPath("Images/english.jpg");
                    GameImage.Source = new BitmapImage(new Uri(path));
                    GameDescription.Text = "Learn English by identifying objects and words!";
                    break;

                case "Run Game":
                    path = System.IO.Path.GetFullPath("Images/run.jpg");
                    GameImage.Source = new BitmapImage(new Uri(path));
                    GameDescription.Text = "Run, avoid obstacles, and reach the finish line!";
                    break;

                case "Balloon Pop Game":
                    path = System.IO.Path.GetFullPath("Images/balloon.jpg");
                    GameImage.Source = new BitmapImage(new Uri(path));
                    GameDescription.Text = "Pop the balloon, BEWARE of the bomb!";
                    break;

                default:
                    GameImage.Source = null;
                    GameDescription.Text = "Select a game to see details.";
                    break;
            }
        }

        private void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedDownloadFolder))
            {
                MessageBox.Show("Please choose a download folder first!", "No Folder Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GameListBox.SelectedItem is ListBoxItem selectedItem)
            {
                string gameName = selectedItem.Content.ToString();
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

            if (GameListBox.SelectedItem is ListBoxItem selectedItem)
            {
                string gameName = selectedItem.Content.ToString();
                string downloadUrl = GetDownloadUrl(gameName);
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

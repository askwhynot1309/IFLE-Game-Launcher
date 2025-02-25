using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace IFLEGameLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

        private void DownloadGame_Click(object sender, RoutedEventArgs e)
        {
            if (GameListBox.SelectedItem is ListBoxItem selectedItem)
            {
                string gameName = selectedItem.Content.ToString();
                MessageBox.Show($"Downloading {gameName}...", "Download", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a game first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            if (GameListBox.SelectedItem is ListBoxItem selectedItem)
            {
                string gameName = selectedItem.Content.ToString();
                try
                {
                    string gamePath = gameName switch
                    {
                        "Math Game" => "C:\\Users\\PC\\Desktop\\english-game\\exe\\english-game-unity.exe",
                        "English Game" => "Games/EnglishGame.exe",
                        "Run to Win" => "Games/RunToWin.exe",
                        "Jump Game" => "Games/JumpGame.exe",
                        _ => string.Empty
                    };

                    if (!string.IsNullOrEmpty(gamePath))
                    {
                        Process.Start(gamePath);
                    }
                    else
                    {
                        MessageBox.Show("Game path not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to launch game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a game first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

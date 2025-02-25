using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IFLEGameLauncher
{
    public partial class LoginWindow : Window
    {
        private readonly HttpClient _httpClient = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            // Basic Validation
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Call API for Login
            if (await LoginAsync(email, password))
            {
                //MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Open Main Game Window
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                this.Close();
            }
            else
            {
                MessageBox.Show("Login failed. Please check your input.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new
                {
                    email = email,
                    password = password
                };

                string jsonContent = JsonSerializer.Serialize(loginData);
                HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // API Request
                HttpResponseMessage response = await _httpClient.PostAsync("https://localhost:7000/api/login", content);
                
                // Check Response
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Login failed: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using IFLEGameLauncher.Model;
using Newtonsoft.Json;

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

            if (await PerformLogin(email, password))
            {
                // Open Main Game Window
                //MainWindow mainWindow = new MainWindow();
                //mainWindow.Show();

                OrganizationWindow orgWindow = new OrganizationWindow();
                orgWindow.Show();
                this.Close();

                this.Close();
            }
            else
            {
                MessageBox.Show("Login failed. Please check your input.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task<bool> PerformLogin(string email, string password)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var loginData = new
                    {
                        email = email,
                        password = password
                    };

                    string json = JsonConvert.SerializeObject(loginData);
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    Debug.WriteLine(content);

                    //string loginUrl = "http://160.187.240.95:8080/api/auth/login";
                    string loginUrl = "https://localhost:7174/api/auth/login";
                    HttpResponseMessage response = await client.PostAsync(loginUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(result);

                        //store tokens
                        string accessToken = loginResponse.AccessToken;
                        string refreshToken = loginResponse.RefreshToken;

                        App.AccessToken = accessToken;
                        App.RefreshToken = refreshToken;


                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message);
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

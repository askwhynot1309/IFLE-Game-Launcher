﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using IFLEGameLauncher.Model;

namespace IFLEGameLauncher
{
    public partial class FloorWindow : Window
    {
        private string organizationId;

        public FloorWindow(string orgId)
        {
            InitializeComponent();
            organizationId = orgId;
            LoadFloors();
        }

        private async void LoadFloors()
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", App.AccessToken);

                var response = await client.GetAsync($"https://localhost:7174/api/organizations/{organizationId}/floors");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var floors = JsonConvert.DeserializeObject<List<Floor>>(json);

                FloorListBox.ItemsSource = floors;
                //FloorListBox.DisplayMemberPath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading floors: {ex.Message}");
            }
        }

        private void FloorListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FloorListBox.SelectedItem is Floor selectedFloor)
            {
                MainWindow mainWindow = new MainWindow(selectedFloor.Id);
                mainWindow.Show();
                this.Close();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var orgWindow = new OrganizationWindow();
            orgWindow.Show();
            this.Close();
        }
    }
}

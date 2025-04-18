﻿using System;
using System.Collections.Generic;
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
    public partial class OrganizationWindow : Window
    {
        public OrganizationWindow()
        {
            InitializeComponent();
            LoadOrganizations();
        }

        private async void LoadOrganizations()
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", App.AccessToken);

                var response = await client.GetAsync("https://localhost:7174/api/organizations/own");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var organizations = JsonConvert.DeserializeObject<List<Organization>>(json);

                OrganizationListBox.ItemsSource = organizations;
                //OrganizationListBox.DisplayMemberPath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading organizations: {ex.Message}");
            }
        }

        private void OrganizationListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrganizationListBox.SelectedItem is Organization selectedOrg)
            {
                FloorWindow floorWindow = new FloorWindow(selectedOrg.Id);
                floorWindow.Show();
                this.Close();
            }
        }
    }
}

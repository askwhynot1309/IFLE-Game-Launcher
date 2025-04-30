using System;
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
using IFLEGameLauncher.API;

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
                string api_url = IFLE_API.OwnOrganizationAPI;

                var response = await client.GetAsync(api_url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var organizations = JsonConvert.DeserializeObject<List<Organization>>(json);

                OrganizationListBox.ItemsSource = organizations;
                //OrganizationListBox.DisplayMemberPath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy thông tin các tổ chức: {ex.Message}");
            }
        }

        private void OrganizationListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrganizationListBox.SelectedItem is Organization selectedOrg)
            {
                App.OrgId = selectedOrg.Id;
                FloorWindow floorWindow = new FloorWindow(selectedOrg.Id);
                floorWindow.Show();
                this.Close();
            }
        }
    }
}

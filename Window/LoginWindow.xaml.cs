using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using IFLEGameLauncher.Model;
using Newtonsoft.Json;
using IFLEGameLauncher.API;
using System.IdentityModel.Tokens.Jwt;

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
                MessageBox.Show("Vui lòng nhập địa chỉ email hợp lệ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải ít nhất 6 ký tự", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Đăng nhập thất bại", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    string loginUrl = IFLE_API.LoginAPI;
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

                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(App.AccessToken);

                        var userIdClaim = jwtToken.Claims.FirstOrDefault(claim =>
                            claim.Type == "userId" || claim.Type.EndsWith("/identity/claims/nameidentifier"));

                        string? userId = userIdClaim?.Value;
                        App.UserId = userId;

                        if (string.IsNullOrEmpty(userId))
                        {
                            Debug.WriteLine("Failed to get user ID from access token.");
                        }
                        bool checkActiveUser = await IsUserActiveAsync(userId);
                        if (checkActiveUser)
                        {
                            MessageBox.Show("Đã có người dùng khác đăng nhập bằng tài khoản này");
                            return false;
                        } else
                        {
                            await MarkUserActiveAsync(userId);
                        }
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
                MessageBox.Show("Đăng nhập thất bại: " + ex.Message);
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

        private async Task<bool> IsUserActiveAsync(string userId)
        {
            using (var client = new HttpClient())
            {
                string url = IFLE_API.CheckActiveUser(userId);
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ActiveUserResponse>(json);

                    return result?.IsActive ?? false;
                }
                catch (Exception ex)
                {
                    return false; 
                }
            }
        }

        private async Task<bool> MarkUserActiveAsync(string userId)
        {
            using (var client = new HttpClient())
            {
                var url = IFLE_API.ActiveUser(userId);
                try
                {
                    var response = await client.PostAsync(url, null);
                    response.EnsureSuccessStatusCode();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

    }
}

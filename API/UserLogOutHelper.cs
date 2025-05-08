using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IFLEGameLauncher.API
{
    public static class UserLogOutHelper
    {
        public static async Task<bool> LogOutActiveUser(string userId)
        {
            using (var client = new HttpClient())
            {
                var url = IFLE_API.LogOut(userId);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IFLEGameLauncher.Model;

namespace IFLEGameLauncher.API
{
    public static class IFLE_API
    {
        //public static string BaseUrl = "https://localhost:7174/api";
        public static string BaseUrl = "https://ifle-api.fusdeploy.site/api";
        public static string LoginAPI = $"{BaseUrl}/auth/login";
        public static string OwnOrganizationAPI = $"{BaseUrl}/organizations/own";
        public static string GetPlayableGamePackageAPI(string floorId)
        {
            return $"{BaseUrl}/floors/{floorId}/game-package/playable";
        }

        public static string UpdateGameCountAPI(string gameId)
        {
            return $"{BaseUrl}/game/update-game-count/{gameId}";
        }

        public static string GetFloorDetailsAPI(string floorId)
        {
            return $"{BaseUrl}/floors/{floorId}";
        }

        public static string GetOrganizationFloor(string organizationId)
        {
            return $"{BaseUrl}/organizations/{organizationId}/floors";
        }

        public static string CheckActiveUser(string userId)
        {
            return $"{BaseUrl}/active-user/check-active?userId={userId}";
        }

        public static string LogOut(string userId)
        {
            return $"{BaseUrl}/active-user/logout?userId={userId}";
        }

        public static string ActiveUser(string userId)
        {
            return $"{BaseUrl}/active-user?userId={userId}";
        }

    }
}

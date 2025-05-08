using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using IFLEGameLauncher.API;

namespace IFLEGameLauncher
{
    public partial class App : Application
    {
        public static string AccessToken { get; internal set; }
        public static string RefreshToken { get; internal set; }
        public static string UserId { get; set; }
        public static string FloorId { get; set; }
        public static string GameId { get; set; }
        public static string OrgId { get; set; }   
        public static float FloorWidth { get; set; }
        public static float FloorLength { get; set; }
        public static float CameraToFloor { get; set; }
        static App()
        {
            string nativePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Astra", Environment.Is64BitProcess ? "x64" : "x32");
            Environment.SetEnvironmentVariable("PATH", nativePath + ";" + Environment.GetEnvironmentVariable("PATH"));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Astra.Context.Initialize();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (!string.IsNullOrEmpty(App.UserId))
            {
                UserLogOutHelper.LogOutActiveUser(App.UserId).GetAwaiter().GetResult();
            }
            base.OnExit(e);

            Astra.Context.Terminate();
        }
    }

}

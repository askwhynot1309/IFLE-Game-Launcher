using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace IFLEGameLauncher
{
    public partial class App : Application
    {
        public static string AccessToken { get; internal set; }
        public static string RefreshToken { get; internal set; }
        public static string UserId { get; set; }
        public static string FloorId { get; set; }
        public static string GameId { get; set; }
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
            base.OnExit(e);

            Astra.Context.Terminate();
        }
    }

}

using System.Configuration;
using System.Data;
using System.Windows;

namespace IFLEGameLauncher
{
    public partial class App : Application
    {
        public static string AccessToken { get; internal set; }
        public static string RefreshToken { get; internal set; }
    }

}

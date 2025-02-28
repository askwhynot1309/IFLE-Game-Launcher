using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFLEGameLauncher
{
    public class Helper
    {
        private string GetDownloadUrl(string gameName)
        {
            return gameName switch
            {
                "Math Game" => "https://localhost:7000/api/downloadMathGame",
                "English Game" => "https://localhost:7000/api/downloadEnglishGame",
                "Run Game" => "https://localhost:7000/api/downloadRunGame",
                "Balloon Pop Game" => "https://localhost:7000/api/downloadBalloonGame",
                _ => null
            };
        }

    }
}

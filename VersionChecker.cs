using Rage;
using System;
using System.Net;
using System.Reflection;
using DisablePistolWhip;

namespace DisablePistolWhip
{
    internal static class VersionChecker
    {
        public static object LatestVersion { get; internal set; }

        public static bool IsUpdateAvailable()
        {
            string curVersion = Config.PluginVersion;
            Uri latestVersionUri = new("https://api-prod.lcpdfr.com/api/downloadsng/files/52806/version");
            WebClient webClient = new();
            string recievedData;
            try
            {
                recievedData = webClient.DownloadString(latestVersionUri).Trim();
            }
            catch (WebException)
            {
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~Disable Pistol Whip Warning", "~r~Failed to check for an update", "Please make sure you're ~y~connected~w~ to your WiFi Network or try to reload the plugin");
                Game.Console.Print();
                Game.Console.Print("===================================================== Disable Pistol Whip ===========================================");
                Game.Console.Print();
                Game.Console.Print("[WARNING!]: Failed to check for an update!");
                Game.Console.Print("[LOG]: Please make sure you are connected to the internet or try to reload the plugin.");
                Game.Console.Print();
                Game.Console.Print();
                Game.Console.Print("==================================================== Disable Pistol Whip ============================================");
                Game.Console.Print();
                Game.LogTrivial("Disable Pistol Whip [LOG]: Failed to check for an update. Please make sure you are connected to the internet or try to reload the plugin.");
                return false;
            }
            if (recievedData != Config.PluginVersion)
            {
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~Disable Pistol Whip Warning", "~y~A new update is available!", "Current Version: ~r~" + curVersion + "~w~<br>New Version: ~y~" + recievedData + "<br>~y~Please Update to the latest build for ~p~latest improvments~w~ and ~g~fixes~w~! :-)");
                Game.Console.Print();
                Game.Console.Print("===================================================== Disable Pistol Whip ===========================================");
                Game.Console.Print();
                Game.Console.Print("[WARNING!]: A new version of Disable Pistol Whip is ~y~NOW AVAILABLE~w~ to download! Update to latest build!");
                Game.Console.Print("[LOG]: Current Version: " + curVersion);
                Game.Console.Print("[LOG]: New Version: " + recievedData);
                Game.Console.Print();
                Game.Console.Print("");
                Game.Console.Print();
                Game.Console.Print("===================================================== Disable Pistol Whip aka `Stop with The F**king Pistol ` ===========================================");
                Game.Console.Print();
                return true;
            }
            else
            {
                Game.DisplayNotification("char_molly", "char_molly", "~w~Disable Pistol Whip Updates", "", "Stella Dimentrescu: We have Detected the ~g~latest~w~ build of ~o~Disable Pistol Whip~w~! Thank you for your business, citizen!");
                return false;
            }
        }
    }
}
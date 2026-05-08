using Rage;
using Rage.Attributes;

namespace DisablePistolWhip
{
    public static class ConsoleCommand
    {
        private static object current;

        [ConsoleCommand(Description = "Toggles the Disable Pistol Whip mod on or off.", Name = "dpw")]
        public static void Toggle()
        {
            try
            {
                EntryPoint.ToggleEnabled();
            }
            catch (System.Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] Console command error: {ex.Message}");
            }
        }

        [ConsoleCommand(Description = "Check for plugin updates", Name = "dpw_update")]
        public static void CheckUpdate()
        {
            try
            {
                Game.LogTrivial("[Disable Pistol Whip] Manually checking for updates...");
                VersionChecker.IsUpdateAvailable();
            }
            catch (System.Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] dpw_update error: {ex.Message}");
            }
        }

        [ConsoleCommand(Description = "Display current version and update status", Name = "dpw_version")]
        public static void ShowVersion()
        {
            try
            {
                
                if (VersionChecker.IsUpdateAvailable())
                {
                    Game.LogTrivial($"[Disable Pistol Whip] Latest version: v{VersionChecker.LatestVersion}");
                    Game.DisplayNotification($"~b~Disable Pistol Whip~w~\nCurrent: ~r~v{current}~w~\nLatest: ~g~v{VersionChecker.LatestVersion}");
                }
                else
                {
                    Game.DisplayNotification($"~g~Disable Pistol Whip v{current}~w~ (Up to date)");
                }
            }
            catch (System.Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] dpw_version error: {ex.Message}");
            }
        }

        [ConsoleCommand(Description = "Set the toggle key for Disable Pistol Whip (e.g. F7)", Name = "dpw_setkey")]
        public static void SetKey(string keyName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyName))
                {
                    Game.LogTrivial("[Disable Pistol Whip] dpw_setkey requires a key name (e.g. F7)");
                    return;
                }

                EntryPoint.SetToggleKeyName(keyName.Trim());
            }
            catch (System.Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] dpw_setkey error: {ex.Message}");
            }
        }

        [ConsoleCommand(Description = "Enable or disable in-game notifications (true/false)", Name = "dpw_notify")]
        public static void SetNotify(string value)
        {
            try
            {
                bool parsed;
                if (bool.TryParse(value, out parsed))
                {
                    EntryPoint.SetNotificationsEnabled(parsed);
                }
                else
                {
                    Game.LogTrivial($"[Disable Pistol Whip] Invalid value for dpw_notify: {value}");
                }
            }
            catch (System.Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] dpw_notify error: {ex.Message}");
            }
        }

        [ConsoleCommand(Description = "Add a weapon to the disabled weapons list (e.g. Pistol50, SMG, AssaultRifle)", Name = "dpw_addweapon")]
        public static void AddWeapon(string weaponName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(weaponName))
                {
                    Game.LogTrivial("[Disable Pistol Whip] dpw_addweapon requires a weapon name (e.g. Pistol50)");
                    return;
                }

                EntryPoint.AddDisabledWeapon(weaponName.Trim());
            }
            catch (System.Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] dpw_addweapon error: {ex.Message}");
            }
        }

        [ConsoleCommand(Description = "Display the config file path and verify it exists", Name = "dpw_path")]
        public static void ShowPath()
        {
            try
            {
                EntryPoint.ShowConfigPath();
            }
            catch (System.Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] dpw_path error: {ex.Message}");
            }
        }

        [ConsoleCommand(Description = "Reset to default disabled weapons", Name = "dpw_reset")]
        public static void ResetToDefaults()
        {
            EntryPoint.UserConfig.DisabledWeapons = "Pistol,CombatPistol,APPistol,StunGun";
            EntryPoint.SaveConfig();
        }

        [ConsoleCommand(Description = "View the list of disabled weapons", Name = "dpw_list")]
        public static void ListWeapons()
        {
            Game.LogTrivial("Disabled Weapons: " + EntryPoint.UserConfig.DisabledWeapons);
        }

        [ConsoleCommand(Description = "Remove a weapon from the disabled weapons list (e.g. Pistol50)", Name = "dpw_removeweapon")]
        public static void RemoveWeapon(string weaponName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(weaponName))
                {
                    Game.LogTrivial("[Disable Pistol Whip] dpw_removeweapon requires a weapon name (e.g. Pistol50)");
                    return;
                }

                EntryPoint.RemoveDisabledWeapon(weaponName.Trim());
            }
            catch (System.Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] dpw_removeweapon error: {ex.Message}");
            }
        }
    }
}

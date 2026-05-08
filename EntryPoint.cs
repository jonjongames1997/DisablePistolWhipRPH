using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;
using Rage;
using Rage.Attributes;

[assembly: Plugin("Disable Pistol Whip", Author = "JM Modifications", Description = "Disables pistol-whip melee attacks while holding a pistol-type weapon.")]

namespace DisablePistolWhip
{
    public static class EntryPoint
    {
        internal static readonly Config UserConfig = new();

        private static readonly string ConfigPath = "Plugins/DisablePistolWhip.ini";

        public static bool Enabled { get; private set; } = true;

        public static void Main()
        {
            Game.LogTrivial("[Disable Pistol Whip] Plugin loaded.");
            Game.DisplayNotification("char_molly", "char_molly", "Stella Dimentrescu", "Disable Pistol Whip", "by JM Modifications. Disable Pistol Whip ~g~successfully~w~ loaded. BE A THUG ABOUT IT, MAH BOI.");
            
            LoadConfig();
            ShowNotification($"Disable Pistol Whip: {(Enabled ? "Enabled" : "Disabled")} (Console: dpw)");

            GameFiber.StartNew(PistolWhipService.MainLoop, "DisablePistolWhipFiber");

            VersionChecker.IsUpdateAvailable();
        }

        private static void LoadConfig()
        {
            string fullPath = Path.GetFullPath(ConfigPath);
            Game.LogTrivial($"[Disable Pistol Whip] Looking for config at: {fullPath}");
            
            if (!File.Exists(ConfigPath))
            {
                Game.LogTrivial("[Disable Pistol Whip] Config file not found, creating with default values.");
                SaveConfig();
                return;
            }

            try
            {
                foreach (var line in File.ReadAllLines(ConfigPath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith(";"))
                        continue;

                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length != 2)
                        continue;

                    var key = parts[0].Trim();
                    var val = parts[1].Trim();

                    if (key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
                    {
                        if (bool.TryParse(val, out var parsed))
                            Enabled = parsed;
                    }
                    else if (key.Equals("ToggleKey", StringComparison.OrdinalIgnoreCase))
                    {
                        if (Enum.TryParse<Keys>(val, true, out var k))
                            UserConfig.ToggleKey = k;
                    }
                    else if (key.Equals("Notifications", StringComparison.OrdinalIgnoreCase))
                    {
                        if (bool.TryParse(val, out var parsed))
                            UserConfig.ShowNotification = parsed;
                    }
                    else if (key.Equals("DisabledWeapons", StringComparison.OrdinalIgnoreCase))
                    {
                        UserConfig.DisabledWeapons = val;
                    }
                    else if (key.Equals("CheckForUpdates", StringComparison.OrdinalIgnoreCase))
                    {
                        if (bool.TryParse(val, out var parsed))
                            UserConfig.CheckForUpdates = parsed;
                    }
                }

                Game.LogTrivial("[Disable Pistol Whip] Config loaded successfully.");
            }
            catch (IOException ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] LoadConfig IO error: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] LoadConfig access error: {ex.Message}");
            }
        }

        internal static void SaveConfig()
        {
            try
            {
                string fullPath = Path.GetFullPath(ConfigPath);
                string directory = Path.GetDirectoryName(fullPath);
                
                if (!Directory.Exists(directory))
                {
                    Game.LogTrivial($"[Disable Pistol Whip] Creating directory: {directory}");
                    Directory.CreateDirectory(directory);
                }
                
                string[] lines = new string[]
                {
                    "; DisablePistolWhip Configuration",
                    "; For support, Join the Discord: https://discord.gg/N9KgZx4KUn",
                    "",
                    "; Enable or disable the plugin",
                    "Enabled=" + Enabled.ToString().ToLower(),
                    "",
                    "; Key to toggle the plugin on/off (System.Windows.Forms.Keys)",
                    "ToggleKey=" + UserConfig.ToggleKey.ToString(),
                    "",
                    "; Show in-game notifications",
                    "Notifications=" + UserConfig.ShowNotification.ToString().ToLower(),
                    "",
                    "; Check for updates on plugin load",
                    "CheckForUpdates=" + UserConfig.CheckForUpdates.ToString().ToLower(),
                    "",
                    "; Comma-separated list of WeaponHash names",
                    "; Use 'dpw_preset' command or the in-game menu to quickly change presets",
                    "DisabledWeapons=" + (UserConfig.DisabledWeapons ?? "Pistol,CombatPistol,APPistol,StunGun"),
                };

                File.WriteAllLines(ConfigPath, lines);
                Game.LogTrivial($"[Disable Pistol Whip] Config saved successfully to: {fullPath}");
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] SaveConfig error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        private static void ShowNotification(string text)
        {
            if (UserConfig.ShowNotification)
            {
                Game.DisplayNotification(text);
            }
            else
            {
                Game.LogTrivial("[Disable Pistol Whip] " + text);
            }
        }

        public static void ToggleEnabled()
        {
            Enabled = !Enabled;
            SaveConfig();
            ShowNotification($"Disable Pistol Whip: {(Enabled ? "Enabled" : "Disabled")}");
            Game.LogTrivial($"[Disable Pistol Whip] Toggled to: {Enabled}");
        }

        public static void SetToggleKeyName(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
                return;

            var trimmed = keyName.Trim();
            if (Enum.TryParse<Keys>(trimmed, true, out var parsed))
            {
                UserConfig.ToggleKey = parsed;
                SaveConfig();
                ShowNotification($"Toggle key set to: {UserConfig.ToggleKey}");
                Game.LogTrivial($"[Disable Pistol Whip] Toggle key changed to: {UserConfig.ToggleKey}");
            }
            else
            {
                Game.LogTrivial($"[Disable Pistol Whip] Invalid toggle key name: {trimmed}");
                ShowNotification($"Invalid key: {trimmed}");
            }
        }

        public static void SetNotificationsEnabled(bool enabled)
        {
            UserConfig.ShowNotification = enabled;
            SaveConfig();
            ShowNotification($"Notifications {(UserConfig.ShowNotification ? "enabled" : "disabled")}");
        }

        public static void AddDisabledWeapon(string weaponName)
        {
            if (string.IsNullOrWhiteSpace(weaponName))
            {
                Game.LogTrivial("[Disable Pistol Whip] AddDisabledWeapon requires a weapon name (e.g. Pistol50, SMG)");
                ShowNotification("Weapon name cannot be empty");
                return;
            }

            var trimmed = weaponName.Trim();

            if (!Enum.TryParse<WeaponHash>(trimmed, true, out var weaponHash))
            {
                Game.LogTrivial($"[Disable Pistol Whip] Invalid weapon name: {trimmed}");
                ShowNotification($"Invalid weapon: {trimmed}");
                return;
            }

            var currentWeapons = string.IsNullOrWhiteSpace(UserConfig.DisabledWeapons)
                ? new System.Collections.Generic.List<string>()
                : UserConfig.DisabledWeapons.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => w.Trim())
                    .ToList();

            if (currentWeapons.Any(w => w.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                Game.LogTrivial($"[Disable Pistol Whip] Weapon already in disabled list: {trimmed}");
                ShowNotification($"{trimmed} is already disabled");
                return;
            }

            currentWeapons.Add(trimmed);
            UserConfig.DisabledWeapons = string.Join(",", currentWeapons);
            SaveConfig();

            Game.LogTrivial($"[Disable Pistol Whip] Added weapon to disabled list: {trimmed}");
            ShowNotification($"Added {trimmed} to disabled weapons");
        }

        public static void RemoveDisabledWeapon(string weaponName)
        {
            if (string.IsNullOrWhiteSpace(weaponName))
                return;

            var weapons = UserConfig.DisabledWeapons?.Split(',') ?? new string[0];
            var updatedWeapons = weapons
                .Select(w => w.Trim())
                .Where(w => !w.Equals(weaponName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            UserConfig.DisabledWeapons = string.Join(",", updatedWeapons);
            SaveConfig();
        }

        public static void ShowConfigPath()
        {
            string fullPath = Path.GetFullPath(ConfigPath);
            bool exists = File.Exists(ConfigPath);
            
            Game.LogTrivial($"[Disable Pistol Whip] Config path: {fullPath}");
            Game.LogTrivial($"[Disable Pistol Whip] File exists: {exists}");
            Game.DisplayNotification($"Config: {fullPath} (Exists: {exists})");
        }
    }
}
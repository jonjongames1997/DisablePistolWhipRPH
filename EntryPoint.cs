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

        private static readonly string ConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "DisablePistolWhip.ini");

        public static bool Enabled { get; private set; } = true;

        public static void Main()
        {
            Game.LogTrivial("[Disable Pistol Whip] Plugin loaded.");
            Game.DisplayNotification("char_molly", "char_molly", "Stella Dimentrescu", "Disable Pistol Whip", "by JM Modifications. Disable Pistol Whip ~g~successfully~w~ loaded. BE A THUG ABOUT IT, MAH BOI.");
            EnsureConfigExists();
            LoadConfig();
            ShowNotification($"Disable Pistol Whip: {(Enabled ? "Enabled" : "Disabled")} (Console toggle: dpw)");

            GameFiber.StartNew(PistolWhipService.MainLoop, "DisablePistolWhipFiber");
            IniReflector<Config> iniReflector = new(ConfigPath);
            iniReflector.Read(UserConfig, true);
        }

        private static void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                SaveConfig();
                return;
            }

            try
            {
                foreach (var line in File.ReadAllLines(ConfigPath))
                {
                    if (string.IsNullOrWhiteSpace(line))
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
                    else if (key.Equals("ToggleKey", StringComparison.OrdinalIgnoreCase) || key.Equals("ToggleKeyName", StringComparison.OrdinalIgnoreCase))
                    {
                        if (Enum.TryParse<Keys>(val, true, out var k))
                            UserConfig.ToggleKey = k;
                    }
                    else if (key.Equals("Notifications", StringComparison.OrdinalIgnoreCase) || key.Equals("NotificationsEnabled", StringComparison.OrdinalIgnoreCase))
                    {
                        if (bool.TryParse(val, out var parsed))
                            UserConfig.ShowNotification = parsed;
                    }
                }
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

        private static void SaveConfig()
        {
            string[] lines = new string[]
            {
                "; DisablePistolWhip configuration",
                "; Enabled = true/false",
                "Enabled=" + Enabled.ToString().ToLower(),
                "; ToggleKey = name of key from System.Windows.Forms.Keys (e.g. F7)",
                "ToggleKey=" + UserConfig.ToggleKey.ToString(),
                "; Notifications = true/false (show in-game notifications)",
                "Notifications=" + UserConfig.ShowNotification.ToString().ToLower(),
                "; DisabledWeapons = comma-separated list of weapon names or categories (pistol, smg, rifles, shotguns)",
                "DisabledWeapons=" + UserConfig.DisabledWeapons,
            };

            try
            {
                File.WriteAllLines(ConfigPath, lines);
            }
            catch (IOException ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] SaveConfig IO error: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] SaveConfig access error: {ex.Message}");
            }
        }

        private static void EnsureConfigExists()
        {
            if (!File.Exists(ConfigPath))
            {
                try
                {
                    File.WriteAllLines(ConfigPath, new[]
                    {
                        "; DisablePistolWhip configuration",
                        "; Set Enabled to true or false and save the file",
                        "Enabled=true"
                    });
                }
                catch (IOException ex)
                {
                    Game.LogTrivial($"[Disable Pistol Whip] EnsureConfigExists IO error: {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Game.LogTrivial($"[Disable Pistol Whip] EnsureConfigExists access error: {ex.Message}");
                }
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

            // Validate that the weapon name is a valid WeaponHash
            if (!Enum.TryParse<WeaponHash>(trimmed, true, out var weaponHash))
            {
                Game.LogTrivial($"[Disable Pistol Whip] Invalid weapon name: {trimmed}");
                ShowNotification($"Invalid weapon: {trimmed}");
                return;
            }

            // Get current disabled weapons list
            var currentWeapons = string.IsNullOrWhiteSpace(UserConfig.DisabledWeapons)
                ? new System.Collections.Generic.List<string>()
                : UserConfig.DisabledWeapons.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => w.Trim())
                    .ToList();

            // Check if weapon already exists in the list (case-insensitive)
            if (currentWeapons.Any(w => w.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                Game.LogTrivial($"[Disable Pistol Whip] Weapon already in disabled list: {trimmed}");
                ShowNotification($"{trimmed} is already disabled");
                return;
            }

            // Add the new weapon
            currentWeapons.Add(trimmed);
            UserConfig.DisabledWeapons = string.Join(",", currentWeapons);

            // Persist to INI
            SaveConfig();

            Game.LogTrivial($"[Disable Pistol Whip] Added weapon to disabled list: {trimmed}");
            ShowNotification($"Added {trimmed} to disabled weapons");
        }
    }
}
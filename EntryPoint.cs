using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using Rage;
using Rage.Attributes;
using DisablePistolWhip;

[assembly: Plugin("Disable Pistol Whip", Author = "JM Modifications", Description = "Disables pistol-whip melee attacks while holding a pistol-type weapon.")]

namespace DisablePistolWhip
{
    public static class EntryPoint
    {
        internal static readonly Config UserConfig = new();

        private static readonly string ConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "DisablePistolWhip.ini");

        public static bool Enabled { get; private set; } = true;
        private static DateTime _lastTogglePress = DateTime.MinValue;
        private const int ToggleDebounceMs = 500;
        private static readonly System.Collections.Generic.HashSet<WeaponHash> DisabledWeaponHashes = new System.Collections.Generic.HashSet<WeaponHash>();
        private static string DisabledWeaponsRaw = null;

        public static void Main()
        {
            Game.LogTrivial("[Disable Pistol Whip] Plugin loaded.");
            Game.DisplayNotification("char_molly", "char_molly", "Stella Dimentrescu", "Disable Pistol Whip", "by JM Modifications. Disable Pistol Whip ~g~successfully~w~ loaded. BE A THUG ABOUT IT, MAH BOI.");
            EnsureConfigExists();
            LoadConfig();
            ShowNotification($"Disable Pistol Whip: {(Enabled ? "Enabled" : "Disabled")} (Console toggle: dpw)");

            GameFiber.StartNew(PistolWhipService.MainLoop, "DisablePistolWhipFiber");
            IniReflector<Config> iniReflector = new("Plugins/DisablePistolWhip.ini");
            iniReflector.Read(UserConfig, true);

            while (true)
            {
                GameFiber.Yield();

                if (Game.IsKeyDown(UserConfig.ToggleKey))
                {
                    ShowNotification($"Disable Pistol Whip: {(Enabled ? "Enabled" : "Disabled")} (Console toggle: dpw)");
                }
            }
        }


        private static void MainLoop()
        {
            while (true)
            {
                try
                {
                    Ped player = Game.LocalPlayer.Character;

                    if (player == null || !player.Exists())
                    {
                        FiberSleep(500);
                        continue;
                    }

                    if (player.IsDead || !player.IsOnFoot)
                    {
                        FiberSleep(500);
                        continue;
                    }

                    var inv = player.Inventory;
                    if (inv == null)
                    {
                        FiberSleep(500);
                        continue;
                    }

                    var equipped = inv.EquippedWeapon;
                    WeaponHash currentHash = equipped != null ? equipped.Hash : 0;

                    // Use configured key from UserConfig (strongly-typed) and debounce toggles.
                    if (UserConfig.ToggleKey != System.Windows.Forms.Keys.None && Game.IsKeyDown(UserConfig.ToggleKey) && (DateTime.UtcNow - _lastTogglePress).TotalMilliseconds > ToggleDebounceMs)
                    {
                        _lastTogglePress = DateTime.UtcNow;
                        ToggleEnabled();
                        FiberSleep(300);
                    }

                    // update disabled weapon set if config changed
                    if (DisabledWeaponsRaw != UserConfig.DisabledWeapons)
                    {
                        DisabledWeaponsRaw = UserConfig.DisabledWeapons;
                        DisabledWeaponHashes.Clear();
                        if (!string.IsNullOrWhiteSpace(DisabledWeaponsRaw))
                        {
                            var parts = DisabledWeaponsRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var p in parts)
                            {
                                var name = p.Trim();
                                if (string.IsNullOrEmpty(name)) continue;
                                // allow groups like 'smgs', 'rifles', 'shotguns'
                                foreach (var candidate in ExpandWeaponEntry(name))
                                {
                                    if (Enum.TryParse<WeaponHash>(candidate, true, out var h))
                                        DisabledWeaponHashes.Add(h);
                                }
                            }
                        }
                    }

                    if (Enabled && DisabledWeaponHashes.Contains(currentHash))
                    {
                        Game.DisableControlAction(0, GameControl.MeleeAttackLight, true);
                        Game.DisableControlAction(0, GameControl.MeleeAttackHeavy, true);
                        Game.DisableControlAction(0, GameControl.MeleeAttackAlternate, true);

                        GameFiber.Yield();
                    }
                    else
                    {
                        FiberSleep(200);
                    }
                }
                catch (Exception ex)
                {
                    Game.LogTrivial($"[Disable Pistol Whip] Exception: {ex.Message}");
                    FiberSleep(500);
                }
            }
        }

        private static readonly System.Collections.Generic.HashSet<WeaponHash> PistolHashes = new System.Collections.Generic.HashSet<WeaponHash>();

        static EntryPoint()
        {
            string[] pistolNames = new[]
            {
                "Pistol",
                "PistolMk2",
                "CombatPistol",
                "APPistol",
                "SNSPistol",
                "SNSPistolMk2",
                "HeavyPistol",
                "VintagePistol",
                "MarksmanPistol",
                "Revolver",
                "RevolverMk2",
                "DoubleActionRevolver",
                "NavyRevolver",
                "CeramicPistol",
                "StunGun",
                "FlareGun",
                "Pistol50",
                "MachinePistol",
                "MicroSMG",
                "SMG",
                "SMGMk2",
                "AssaultSMG"
            };

            foreach (var name in pistolNames)
            {
                if (Enum.TryParse<WeaponHash>(name, out var parsed))
                {
                    PistolHashes.Add(parsed);
                }
            }
        }

        private static bool IsPistol(WeaponHash hash)
        {
            return PistolHashes.Contains(hash) || DisabledWeaponHashes.Contains(hash);
        }

        // Expand a config entry: either a category or a specific enum name
        private static IEnumerable<string> ExpandWeaponEntry(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry)) yield break;

            var lower = entry.Trim().ToLowerInvariant();
            switch (lower)
            {
                case "pistols":
                case "pistol":
                    yield return "Pistol";
                    yield break;
                case "smgs":
                case "smg":
                    yield return "MicroSMG";
                    yield return "SMG";
                    yield return "SMGMk2";
                    yield return "AssaultSMG";
                    yield break;
                case "rifles":
                case "assault":
                    yield return "CarbineRifle";
                    yield return "AssaultRifle";
                    yield return "SpecialCarbine";
                    yield break;
                case "shotguns":
                    yield return "PumpShotgun";
                    yield return "SawnOffShotgun";
                    yield return "BullpupShotgun";
                    yield break;
                default:
                    yield return entry.Trim();
                    yield break;
            }
        }

        private static void LoadConfig()
        {
            // Read simple key=value INI used by this plugin. Only catch IO-related exceptions.
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
                        if (Enum.TryParse<System.Windows.Forms.Keys>(val, true, out var k))
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
            // write a simple INI with a comment for users. Catch IO-related errors only.
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
            if (Enum.TryParse<System.Windows.Forms.Keys>(trimmed, true, out var parsed))
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

        private static void FiberSleep(int ms)
        {
            try
            {
                GameFiber.Sleep(ms);
            }
            catch (InvalidOperationException)
            {
                System.Threading.Thread.Sleep(ms);
            }
        }
    }
}
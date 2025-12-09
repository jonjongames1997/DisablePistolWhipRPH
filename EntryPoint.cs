using System;
using System.IO;
using System.Reflection;
using Rage;
using Rage.Attributes;

[assembly: Plugin("Disable Pistol Whip", Author = "JM MM", Description = "Disables pistol-whip melee attacks while holding a pistol.")]

namespace DisablePistolWhip
{
    public static class EntryPoint
    {
        private static readonly string ConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "DisablePistolWhip.ini");

        public static bool Enabled { get; private set; } = true;

        public static void Main()
        {
            Game.LogTrivial("[Disable Pistol Whip] Plugin loaded.");
            // Ensure an INI exists with defaults and helpful comments
            EnsureConfigExists();
            LoadConfig();
            ShowNotification($"Disable Pistol Whip: {(Enabled ? "Enabled" : "Disabled")} (Console toggle: dpw)");

            GameFiber.StartNew(MainLoop, "DisablePistolWhipFiber");
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

                    if (Enabled && IsPistol(currentHash))
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
                "CeramicPistol"
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
            return PistolHashes.Contains(hash);
        }

        private static void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    SaveConfig();
                    return;
                }

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
                        bool parsed;
                        if (bool.TryParse(val, out parsed))
                            Enabled = parsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] LoadConfig error: {ex.Message}");
            }
        }

        private static void SaveConfig()
        {
            try
            {
                // write a simple INI with a comment for users
                string[] lines = new string[]
                {
                    "; DisablePistolWhip configuration",
                    "; Enabled = true/false",
                    "Enabled=" + Enabled.ToString().ToLower()
                };

                File.WriteAllLines(ConfigPath, lines);
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] SaveConfig error: {ex.Message}");
            }
        }

        private static void EnsureConfigExists()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    File.WriteAllLines(ConfigPath, new[]
                    {
                        "; DisablePistolWhip configuration",
                        "; Set Enabled to true or false and save the file",
                        "Enabled=true"
                    });
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] EnsureConfigExists error: {ex.Message}");
            }
        }

        private static void ShowNotification(string text)
        {
            try
            {
                Game.DisplayNotification(text);
            }
            catch
            {
                Game.LogTrivial("[Disable Pistol Whip] " + text);
            }
        }

        // Public API for console commands to toggle the mod
        public static void ToggleEnabled()
        {
            Enabled = !Enabled;
            SaveConfig();
            ShowNotification($"Disable Pistol Whip: {(Enabled ? "Enabled" : "Disabled")}");
            Game.LogTrivial($"[Disable Pistol Whip] Toggled to: {Enabled}");
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

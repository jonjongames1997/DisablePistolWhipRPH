using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Rage;

namespace DisablePistolWhip
{
    internal static class PistolWhipService
    {
        private static DateTime _lastTogglePress = DateTime.MinValue;
        private const int ToggleDebounceMs = 500;

        private static readonly HashSet<WeaponHash> DisabledWeaponHashes = new HashSet<WeaponHash>();
        private static string DisabledWeaponsRaw = null;

        private static readonly HashSet<WeaponHash> PistolHashes = new HashSet<WeaponHash>();

        static PistolWhipService()
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

        public static void MainLoop()
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
                    if (EntryPoint.UserConfig.ToggleKey != Keys.None && Game.IsKeyDown(EntryPoint.UserConfig.ToggleKey) && (DateTime.UtcNow - _lastTogglePress).TotalMilliseconds > ToggleDebounceMs)
                    {
                        _lastTogglePress = DateTime.UtcNow;
                        EntryPoint.ToggleEnabled();
                        FiberSleep(300);
                    }

                    // update disabled weapon set if config changed
                    if (DisabledWeaponsRaw != EntryPoint.UserConfig.DisabledWeapons)
                    {
                        DisabledWeaponsRaw = EntryPoint.UserConfig.DisabledWeapons;
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

                    if (EntryPoint.Enabled && DisabledWeaponHashes.Contains(currentHash))
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
    }
}

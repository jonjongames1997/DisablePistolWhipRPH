using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DisablePistolWhip
{
    internal static class PistolWhipService
    {
        private static DateTime _lastTogglePress = DateTime.MinValue;
        private const int ToggleDebounceMs = 500;

        private static readonly HashSet<WeaponHash> DisabledWeaponHashes = new HashSet<WeaponHash>();
        private static string DisabledWeaponsRaw = null;

        private static readonly HashSet<WeaponHash> PistolHashes = new HashSet<WeaponHash>();

        // R key spam detection
        private static DateTime _lastRKeyPress = DateTime.MinValue;
        private static DateTime _lastSpamNotification = DateTime.MinValue;
        private static int _rKeyPressCount = 0;
        private const int SpamThreshold = 3; // Number of R presses to trigger notification
        private const int SpamWindowMs = 2000; // Time window to detect spam
        private const int SpamNotificationCooldownMs = 5000; // Cooldown between notifications

        private static readonly string[] SpamMessages = new[]
        {
            "~r~Nice try, champ! ~w~Pistol whipping is ~y~disabled~w~.",
            "~r~Nope! ~w~That's not gonna work here.",
            "~y~Stop it. ~w~Get some help. Pistol whip is ~r~OFF~w~.",
            "~b~Stella says: ~w~\"No pistol whipping, ~r~tough guy~w~.\"",
            "~o~Really? ~w~You thought ~r~THAT~w~ would work?",
            "~g~Pro tip: ~w~Pistol whip is ~r~disabled~w~. Try shooting instead.",
            "~r~BONK! ~w~Just kidding, pistol whip is ~y~turned off~w~.",
            "~p~ERROR 404: ~w~Pistol whip not found.",
            "~r~[DENIED] ~w~This feature has been ~y~yeeted~w~.",
            "~b~BE A THUG ABOUT IT! ~w~But not with pistol whipping.",
            "~y~Spamming R won't help, pal. ~w~It's ~r~disabled~w~.",
            "~o~Achievement Unlocked: ~w~\"Persistent Button Masher\"",
            "~r~You're determined, I'll give you that. ~w~Still ~y~no~w~.",
            "~g~The definition of insanity ~w~is trying the same thing and expecting different results.",
            "~p~Stella is watching you struggle. ~w~Pistol whip = ~r~OFF~w~."
        };

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

        private static void CheckRKeySpam(WeaponHash currentHash)
        {
            // Only check if holding a disabled weapon and plugin is enabled
            if (!EntryPoint.Enabled || !DisabledWeaponHashes.Contains(currentHash))
            {
                _rKeyPressCount = 0;
                return;
            }

            // Check if R key is pressed
            if (Game.IsKeyDown(Keys.R))
            {
                var now = DateTime.UtcNow;
                var timeSinceLastPress = (now - _lastRKeyPress).TotalMilliseconds;

                // Reset counter if too much time has passed
                if (timeSinceLastPress > SpamWindowMs)
                {
                    _rKeyPressCount = 1;
                }
                else
                {
                    _rKeyPressCount++;
                }

                _lastRKeyPress = now;

                // Check if spam threshold reached and cooldown expired
                if (_rKeyPressCount >= SpamThreshold)
                {
                    var timeSinceLastNotification = (now - _lastSpamNotification).TotalMilliseconds;
                    
                    if (timeSinceLastNotification > SpamNotificationCooldownMs)
                    {
                        ShowRandomSpamMessage();
                        _lastSpamNotification = now;
                        _rKeyPressCount = 0; // Reset counter after showing message
                    }
                }
            }
        }

        private static void ShowRandomSpamMessage()
        {
            var random = new Random();
            var message = SpamMessages[random.Next(SpamMessages.Length)];
            Game.DisplayNotification(message);
            Game.LogTrivial("[Disable Pistol Whip] Player attempted pistol whip spam - notification shown.");
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

                    // Use configured key from UserConfig and debounce toggles.
                    if (EntryPoint.UserConfig.ToggleKey != Keys.None && Game.IsKeyDown(EntryPoint.UserConfig.ToggleKey) && (DateTime.UtcNow - _lastTogglePress).TotalMilliseconds > ToggleDebounceMs)
                    {
                        _lastTogglePress = DateTime.UtcNow;
                        EntryPoint.ToggleEnabled();
                        FiberSleep(300);
                    }

                    // update disabled weapon set if config changed // 
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

                        // Check for R key spam
                        CheckRKeySpam(currentHash);

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

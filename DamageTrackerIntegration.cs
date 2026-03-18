using Rage;
using System;
using System.Linq;

namespace DisablePistolWhip
{
    public static class DamageTrackerIntegration
    {
        private static bool isInitialized = false;
        private static bool damageTrackerAvailable = false;

        public static void Initialize()
        {
            if (isInitialized)
                return;

            try
            {
                // Check if Damage Tracker Framework is available
                var damageTrackerType = Type.GetType("DamageTrackerFramework.API.DamageTracker, DamageTrackerFramework");

                if (damageTrackerType != null)
                {
                    damageTrackerAvailable = true;
                    RegisterDamageEvents();
                    Game.LogTrivial("[Disable Pistol Whip] Damage Tracker Framework integration enabled.");
                }
                else
                {
                    Game.LogTrivial("[Disable Pistol Whip] Damage Tracker Framework not found. Running in standalone mode.");
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] Failed to initialize Damage Tracker Framework: {ex.Message}");
                damageTrackerAvailable = false;
            }
            finally
            {
                isInitialized = true;
            }
        }

        private static void RegisterDamageEvents()
        {
            try
            {
                // Use reflection to subscribe to damage events if DTF is available
                var damageTrackerType = Type.GetType("DamageTrackerFramework.API.DamageTracker, DamageTrackerFramework");
                if (damageTrackerType == null)
                    return;

                var onPedDamagedEvent = damageTrackerType.GetEvent("OnPedDamaged");
                if (onPedDamagedEvent != null)
                {
                    var handlerDelegate = Delegate.CreateDelegate(
                        onPedDamagedEvent.EventHandlerType,
                        typeof(DamageTrackerIntegration).GetMethod(nameof(OnPedDamaged),
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    );
                    onPedDamagedEvent.AddEventHandler(null, handlerDelegate);
                    Game.LogTrivial("[Disable Pistol Whip] Subscribed to OnPedDamaged event.");
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] Error registering damage events: {ex.Message}");
            }
        }

        private static void OnPedDamaged(object sender, object eventArgs)
        {
            if (!EntryPoint.Enabled)
                return;

            try
            {
                // Use reflection to access event args properties
                var argsType = eventArgs.GetType();
                var damageTypeProperty = argsType.GetProperty("DamageType");
                var weaponProperty = argsType.GetProperty("Weapon");
                var victimProperty = argsType.GetProperty("Victim");
                var attackerProperty = argsType.GetProperty("Attacker");

                if (damageTypeProperty == null || weaponProperty == null)
                    return;

                var damageType = damageTypeProperty.GetValue(eventArgs);
                var weapon = weaponProperty.GetValue(eventArgs);

                // Check if it's a melee damage with a pistol-type weapon
                if (damageType != null && damageType.ToString() == "Melee" && weapon != null)
                {
                    var weaponHash = (WeaponHash)weapon;

                    if (IsWeaponDisabled(weaponHash))
                    {
                        Game.LogTrivial($"[Disable Pistol Whip] DTF: Melee damage detected with {weaponHash}");

                        // Log the event for statistics/debugging
                        if (victimProperty != null && attackerProperty != null)
                        {
                            var victim = victimProperty.GetValue(eventArgs);
                            var attacker = attackerProperty.GetValue(eventArgs);

                            Game.LogTrivial($"[Disable Pistol Whip] DTF: Attacker: {attacker}, Victim: {victim}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] OnPedDamaged error: {ex.Message}");
            }
        }

        private static bool IsWeaponDisabled(WeaponHash weapon)
        {
            if (string.IsNullOrWhiteSpace(EntryPoint.UserConfig.DisabledWeapons))
                return false;

            var disabledWeapons = EntryPoint.UserConfig.DisabledWeapons
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim());

            return disabledWeapons.Any(w => 
                w.Equals(weapon.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsDamageTrackerAvailable => damageTrackerAvailable;
    }
}
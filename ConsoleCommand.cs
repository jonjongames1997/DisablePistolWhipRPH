using Rage;
using Rage.Attributes;

namespace DisablePistolWhip
{
    public static class ConsoleCommand
    {
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
    }
}

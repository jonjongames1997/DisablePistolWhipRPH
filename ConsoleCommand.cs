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
    }
}

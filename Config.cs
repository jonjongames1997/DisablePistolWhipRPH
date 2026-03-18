using Rage;
using System;
using System.IO;
using System.Windows.Forms;
using DisablePistolWhip;

namespace DisablePistolWhip
{

    internal class Config
    {
        internal Keys ToggleKey = Keys.F7;
        internal bool StartEnabled = true;
        internal bool ShowNotification = true;
        internal string DisabledWeapons = "Pistol,CombatPistol,APPistol,StunGun";
        internal bool CheckForUpdates = true;
        internal string Language = "auto"; // "auto", "en-US", "es-ES", "fr-FR", "de-DE", etc.

        public static readonly string PluginVersion = "1.0.3.0";
    }
}
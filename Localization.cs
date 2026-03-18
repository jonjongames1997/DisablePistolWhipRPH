using Rage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DisablePistolWhip
{
    internal static class Localization
    {
        private static Dictionary<string, string> _strings = new Dictionary<string, string>();
        private static string _currentLanguage = "en-US";
        private static readonly string LocalizationFolder = "Plugins/DisablePistolWhip/Localization";

        // Default English strings (fallback)
        private static readonly Dictionary<string, string> DefaultStrings = new Dictionary<string, string>
        {
            // General
            ["PluginLoaded"] = "Plugin loaded.",
            ["PluginEnabled"] = "Disable Pistol Whip: Enabled",
            ["PluginDisabled"] = "Disable Pistol Whip: Disabled",
            ["LoadSuccessNotification"] = "by JM Modifications. Disable Pistol Whip ~g~successfully~w~ loaded. BE A THUG ABOUT IT, MAH BOI.",
            
            // Notifications
            ["NotificationEnabled"] = "Disable Pistol Whip: ~g~Enabled",
            ["NotificationDisabled"] = "Disable Pistol Whip: ~r~Disabled",
            ["NotificationToggled"] = "Disable Pistol Whip: {0}",
            ["NotificationWeaponAdded"] = "Added {0} to disabled weapons",
            ["NotificationWeaponExists"] = "{0} is already disabled",
            ["NotificationInvalidWeapon"] = "Invalid weapon: {0}",
            ["NotificationWeaponEmpty"] = "Weapon name cannot be empty",
            ["NotificationToggleKeySet"] = "Toggle key set to: {0}",
            ["NotificationInvalidKey"] = "Invalid key: {0}",
            ["NotificationNotificationsEnabled"] = "Notifications: ~g~Enabled",
            ["NotificationNotificationsDisabled"] = "Notifications: ~r~Disabled",
            ["NotificationConfigPath"] = "Config: {0} (Exists: {1})",
            
            // Weapon List
            ["DisabledWeaponsNone"] = "~r~None",
            ["DisabledWeaponsTitle"] = "~b~Disabled Weapons:~w~\n{0}",
            
            // Version Checker
            ["VersionCheckStarting"] = "Checking for updates... (Current: v{0})",
            ["VersionUpdateAvailable"] = "Update available! Latest version: v{0}",
            ["VersionUpToDate"] = "You are running the latest version (v{0})",
            ["VersionCheckFailed"] = "Version check failed (network error): {0}",
            ["VersionParseError"] = "Failed to parse version from server: {0}",
            ["VersionUpdateNotification"] = "~y~New version available!~w~\nCurrent: ~r~v{0}~w~\nLatest: ~g~v{1}~w~\n\nVisit LCPDFR.com to download.",
            ["VersionUpToDateNotification"] = "~g~Disable Pistol Whip~w~ is up to date! (v{0})",
            ["VersionCheckNotCompleted"] = "~y~Version check not completed yet. Try again in a moment.",
            ["VersionCurrentAndLatest"] = "~b~Disable Pistol Whip~w~\nCurrent: ~r~v{0}~w~\nLatest: ~g~v{1}",
            ["VersionCurrent"] = "~g~Disable Pistol Whip v{0}~w~ (Up to date)",
            
            // Config
            ["ConfigNotFound"] = "Config file not found, creating with default values.",
            ["ConfigLoaded"] = "Config loaded successfully.",
            ["ConfigSaved"] = "Config saved to: {0}",
            ["ConfigLoadError"] = "LoadConfig IO error: {0}",
            ["ConfigSaveError"] = "SaveConfig error: {0}",
            ["ConfigLookingFor"] = "Looking for config at: {0}",
            ["ConfigPathInfo"] = "Config path: {0}",
            ["ConfigFileExists"] = "File exists: {0}",
                  
            // Console Commands
            ["CommandToggleUsage"] = "Toggles the Disable Pistol Whip mod on or off.",
            ["CommandUpdateUsage"] = "Check for plugin updates",
            ["CommandVersionUsage"] = "Display current version and update status",
            ["CommandSetKeyUsage"] = "Set the toggle key for Disable Pistol Whip (e.g. F7)",
            ["CommandNotifyUsage"] = "Enable or disable in-game notifications (true/false)",
            ["CommandAddWeaponUsage"] = "Add a weapon to the disabled weapons list (e.g. Pistol50, SMG, AssaultRifle)",
            ["CommandPathUsage"] = "Display the config file path and verify it exists",
            ["CommandSetKeyRequired"] = "dpw_setkey requires a key name (e.g. F7)",
            ["CommandAddWeaponRequired"] = "dpw_addweapon requires a weapon name (e.g. Pistol50)",
            ["CommandInvalidNotifyValue"] = "Invalid value for dpw_notify: {0}",
            ["CommandManualUpdateCheck"] = "Manually checking for updates...",
            ["CommandCurrentVersion"] = "Current version: v{0}",
            ["CommandLatestVersion"] = "Latest version: v{0}",
            
            // Errors
            ["ErrorException"] = "Exception: {0}",
            ["ErrorCommandError"] = "Console command error: {0}",
            ["ErrorMenuLoop"] = "Menu loop error: {0}",
        };

        internal static void Initialize()
        {
            // Detect system language
            _currentLanguage = CultureInfo.CurrentCulture.Name;
            Game.LogTrivial($"[Disable Pistol Whip] System language detected: {_currentLanguage}");

            // Try to load language file
            LoadLanguage(_currentLanguage);

            // If not found, try language without region (e.g., "es" instead of "es-MX")
            if (_strings.Count == 0 && _currentLanguage.Contains("-"))
            {
                string languageOnly = _currentLanguage.Split('-')[0];
                Game.LogTrivial($"[Disable Pistol Whip] Trying language without region: {languageOnly}");
                LoadLanguage(languageOnly);
            }

            // Fallback to English if no translation found
            if (_strings.Count == 0)
            {
                Game.LogTrivial("[Disable Pistol Whip] No localization file found, using English defaults.");
                _strings = new Dictionary<string, string>(DefaultStrings);
                _currentLanguage = "en-US";
            }

            Game.LogTrivial($"[Disable Pistol Whip] Localization initialized: {_currentLanguage} ({_strings.Count} strings loaded)");
        }

        private static void LoadLanguage(string language)
        {
            string filePath = Path.Combine(LocalizationFolder, $"{language}.xml");

            if (!File.Exists(filePath))
            {
                Game.LogTrivial($"[Disable Pistol Whip] Localization file not found: {filePath}");
                return;
            }

            try
            {
                XDocument doc = XDocument.Load(filePath);
                _strings.Clear();

                foreach (var element in doc.Root.Elements("String"))
                {
                    string key = element.Attribute("ID")?.Value;
                    string value = element.Value;

                    if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                    {
                        _strings[key] = value;
                    }
                }

                _currentLanguage = language;
                Game.LogTrivial($"[Disable Pistol Whip] Loaded localization: {language} ({_strings.Count} strings)");
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] Error loading localization file {filePath}: {ex.Message}");
                _strings.Clear();
            }
        }

        internal static string GetString(string key, params object[] args)
        {
            if (_strings.TryGetValue(key, out string value))
            {
                try
                {
                    return args.Length > 0 ? string.Format(value, args) : value;
                }
                catch (FormatException)
                {
                    Game.LogTrivial($"[Disable Pistol Whip] Format error for key: {key}");
                    return value;
                }
            }

            // Fallback to default English
            if (DefaultStrings.TryGetValue(key, out string defaultValue))
            {
                try
                {
                    return args.Length > 0 ? string.Format(defaultValue, args) : defaultValue;
                }
                catch (FormatException)
                {
                    return defaultValue;
                }
            }

            Game.LogTrivial($"[Disable Pistol Whip] Missing localization key: {key}");
            return $"[MISSING: {key}]";
        }

        internal static string CurrentLanguage => _currentLanguage;

        internal static void ExportDefaultLanguageFile()
        {
            try
            {
                if (!Directory.Exists(LocalizationFolder))
                {
                    Directory.CreateDirectory(LocalizationFolder);
                }

                string filePath = Path.Combine(LocalizationFolder, "en-US.xml");

                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XComment(" Disable Pistol Whip - English (United States) Localization "),
                    new XComment(" Translate the text inside <String> tags to your language "),
                    new XComment(" DO NOT modify the ID attributes "),
                    new XElement("Localization",
                        new XAttribute("Language", "en-US"),
                        new XAttribute("Author", "JM Modifications"),
                        DefaultStrings.Select(kvp =>
                            new XElement("String",
                                new XAttribute("ID", kvp.Key),
                                kvp.Value
                            )
                        )
                    )
                );

                doc.Save(filePath);
                Game.LogTrivial($"[Disable Pistol Whip] Exported default localization file: {filePath}");
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[Disable Pistol Whip] Error exporting localization file: {ex.Message}");
            }
        }
    }
}
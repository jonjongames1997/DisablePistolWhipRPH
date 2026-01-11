# Changelog

## [v1.0.1.1] - 2026-01-10

### Added

- Added StunGun as a default disabled weapon (will appear after initial INI generation).

### Changed

- Persist toggle key when set via command: the plugin now writes the configured key to the same INI file the runtime reflector uses so keybinds survive restarts.
- `IniReflector` now uses the same `ConfigPath` as manual `SaveConfig`/`LoadConfig` functions to avoid drifting config locations.

### Fixed

- Fixed config persistence for toggle key and notification settings when changed at runtime.
- Minor cleanup: removed redundant try/catch blocks and improved logging for IO errors.


## [v1.0.1] - 2025-12-19 - Previous Release

### Added

- Added StunGun as a default disabledweapon (You can add more to the config (If not exist, launch the plugin at least once for it to generate the ini).)

### Changed

- Removed unnecessary try catches (Thanks to Haze for the advice)

- Seperated the main loop into a seperate C# class (Thanks to Haze)

- It now uses the System.Windows.Forms
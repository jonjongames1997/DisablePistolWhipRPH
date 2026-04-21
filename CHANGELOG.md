# Changelog

All notable changes to Disable Pistol Whip will be documented in this file.

## [1.0.3.1] - 2026-04-21

- Added 15 more humorous messages to make it a total of 30 random humorous messages

## [1.0.3.0] - 2026-04-20

### Added
- **R Key Spam Detection System**: Interactive feedback when players attempt pistol whipping
  - Detects when player spams the R key (reload/melee) while holding a disabled weapon
  - Displays random humorous notifications from a pool of 15 unique messages
  - Smart spam detection with 2-second window and 3-press threshold
  - 5-second cooldown between notifications to prevent spam
  - Easter eggs and references to Stella Dimentrescu
  - Automatic reset when weapon is changed or spam window expires

### Changed
- Updated `PistolWhipService.cs` with R key spam detection logic
  - Added `CheckRKeySpam()` method to monitor player input
  - Integrated spam detection into main service loop
  - Added 15 unique notification messages with color-coded text
- Enhanced logging to include spam attempt tracking

### Improved
- Better user feedback when pistol whip is disabled
- More engaging player experience with personality-driven messages
- Reduced frustration through clear communication of disabled feature

### Messages
Sample notification messages include:
- `"Nice try, champ! Pistol whipping is disabled."`
- `"Stella says: 'No pistol whipping, tough guy.'"`
- `"Pro tip: Pistol whip is disabled. Try shooting instead."`
- `"Achievement Unlocked: 'Persistent Button Masher'"`
- Plus 11 more unique variations!

---

## [1.0.2.0] - Previous Release

### Features
- Console commands for plugin management
- Configurable toggle key
- Customizable weapon disable list
- In-game notifications
- Automatic update checking
- INI-based configuration system

### Console Commands
- `dpw` - Toggle plugin on/off
- `dpw_update` - Check for updates
- `dpw_version` - Display version information
- `dpw_setkey` - Set toggle key
- `dpw_notify` - Enable/disable notifications
- `dpw_addweapon` - Add weapon to disabled list
- `dpw_path` - Show config file path
- `dpw_reset` - Reset to defaults
- `dpw_list` - List disabled weapons

---

## Installation

1. Extract all files to your RAGE Plugin Hook directory
2. Configure via `Plugins/DisablePistolWhip.ini`

## Requirements

- RAGE Plugin Hook
- .NET Framework 4.7.2 or higher
- GTA V
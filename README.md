# Disable Pistol Whip

Disable Pistol Whip is a lightweight plugin for Rage-based scripting environments that disables pistol-whip melee controls while the player is holding a pistol. It is intended for users who prefer to prevent accidental melee attacks when firing pistols.

Key features
- Disable light/heavy/alternate melee controls when a pistol is equipped
- Toggle at runtime via a console command or configurable keyboard key
- Simple INI-based configuration created automatically on first run
- No hard dependency on `System.Windows.Forms` — key handling validated at runtime when available

Requirements
- RagePluginHook

Installation

Drag and Drop "plugins" folder into main gta directory

Usage
- By default the plugin is enabled. When a pistol is equipped the plugin disables melee attack controls so the player cannot pistol-whip while holding a pistol.
- Console commands (type in the in-game console):
  - `dpw` — Toggle the plugin on or off (state is persisted to `DisablePistolWhip.ini`).
  - `dpw_setkey <KeyName>` — Change the keyboard toggle key (e.g. `dpw_setkey F8`). The key is stored as a string and validated at runtime if possible.
  - `dpw_notify <true|false>` — Enable or disable in-game notifications.

Configuration
- The plugin stores settings in `DisablePistolWhip.ini` next to the plugin DLL. Example contents:

```
; DisablePistolWhip configuration
; Enabled = true/false
Enabled=true
; ToggleKey = name of key (e.g. F7)
ToggleKey=F7
; Notifications = true/false
Notifications=true
```

Notes
- Keyboard toggle uses reflection to parse the key name against `System.Windows.Forms.Keys` at runtime; if that assembly isn't available the plugin will still accept the key name but cannot validate it. Console commands always work.
- The plugin writes a simple INI and will overwrite it when settings change. If you want to preserve manual comments, back up the file before editing.

Troubleshooting
- Plugin not working: verify `Enabled=true` in `DisablePistolWhip.ini`.
- Keyboard toggle not responding: try the console command `dpw` or set a different key with `dpw_setkey`.
- Check the game log for messages starting with `[Disable Pistol Whip]` for diagnostics.

Contributing
- Want to add features or fix a bug? Fork the repository, make changes, and open a pull request. Please include a short description of the change and, if appropriate, a changelog entry.

License
- MIT Licennse

Credits
- Plugin author: project metadata (assembly attributes)

Contact
- Use the project issue tracker for support.

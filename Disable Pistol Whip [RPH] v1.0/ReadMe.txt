Disable Pistol Whip is a lightweight plugin for Rage-based scripting environments that disables pistol-whip melee controls while the player is holding a pistol. 
It is intended for users who prefer to prevent accidental melee attacks when firing pistols.

Key features

- Disable light/heavy/alternate melee controls when a pistol is equipped
- Toggle at runtime via a console command or configurable keyboard key
- Simple INI-based configuration created automatically on first run
- No hard dependency on System.Windows.Forms — key handling validated at runtime when available

Requirements:

- RAGEPluginHook

Installation

Drag and Drop "plugins" folder into main gta directory

Usage

By default the plugin is enabled. When a pistol is equipped the plugin disables melee attack controls so the player cannot pistol-whip while holding a pistol.
Console commands (type in the in-game console):
dpw — Toggle the plugin on or off (state is persisted to DisablePistolWhip.ini).
dpw_setkey <KeyName> — Change the keyboard toggle key (e.g. dpw_setkey F8). The key is stored as a string and validated at runtime if possible.
dpw_notify <true|false> — Enable or disable in-game notifications.

Contributing

Want to add features or fix a bug? Fork the repository, make changes, and open a pull request. 
Please include a short description of the change and, if appropriate, a changelog entry.

Ini file uses MarcelWRLD's IniReflector system. The code used for the ini belongs to MarcelWRLD.

For any issues, Use the project issue tracker for support. Support will not be given on Discord or LCPDFR. 
GitHub is where I mostly checking every day. 
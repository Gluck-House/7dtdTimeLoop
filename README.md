# TimeLoop

TimeLoop is a server-side 7 Days to Die mod that pauses day progression unless your configured player conditions are met.

The current repository state builds against 7 Days to Die V2.5 dedicated server assemblies.

## What it does

TimeLoop supports four operating modes:

- `always`: always loop the day
- `whitelist`: time only passes when an authorized player is online
- `threshold`: time only passes when at least `MinPlayers` players are online
- `whitelisted_threshold`: time only passes when at least `MinPlayers` authorized players are online

It also supports:

- per-player authorization
- loop limits
- skip-days
- multiple locales
- server console commands for runtime changes

## Installation

1. Build the mod or download a release archive.
2. Copy the `timeloop/` folder into your server `Mods/` directory.
3. Start the server once.
4. Edit `TimeLooper.xml` generated inside the mod folder if you want to change defaults.

If you build from this repo, `./build.sh` creates:

- `TimeLoop/build/TimeLoop/` containing the unpacked mod files
- `TimeLoop/build/TimeLoop.zip` containing a top-level `timeloop/` folder ready to upload or extract into `Mods/`

## Quick Start

To enable the mod and require at least two players for time to pass:

```text
tl_enable 1
tl_mode 2
tl_min 2
```

## Configuration

The configuration file is created automatically on first launch if it does not exist.

Example:

```xml
<?xml version="1.0" encoding="utf-8"?>
<TimeLoopConfig xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Enabled>true</Enabled>
  <Mode>whitelist</Mode>
  <Players>
    <PlayerModel ID="Steam_76561198061215936" Name="Yui" Whitelisted="false" />
  </Players>
  <MinPlayers>1</MinPlayers>
  <DaysToSkip>0</DaysToSkip>
  <LoopLimit>0</LoopLimit>
  <Language>en_us</Language>
</TimeLoopConfig>
```

Notes:

- `LoopLimit=0` means unlimited loops.
- `DaysToSkip=0` disables skip-days.
- Locale files live in `i18n/`.

## Console Commands

The mod exposes these server console commands:

- `tl_enable [0|1]`
  Aliases: `timeloop_enable`, `timeloop`
- `tl_mode [0|1|2|3]`
  Alias: `timeloop_mode`
- `tl_auth <platform_id|player_name> <0|1>`
  Aliases: `tl_authorize`, `timeloop_auth`, `timeloop_authorize`
- `tl_min [amount]`
  Aliases: `tl_minplayers`, `timeloop_minplayers`
- `tl_list [all|auth|unauth]`
  Alias: `timeloop_list`
- `tl_ll [amount]`
  Aliases: `tl_looplimit`, `timeloop_looplimit`
- `tl_skipdays [days]`
  Alias: `timeloop_skipdays`
- `tl_state`
  Alias: `timeloop_state`
- `tl_locale [locale]`
  Alias: `timeloop_locale`

Omitting arguments on most commands prints the current state.

## Building

Requirements:

- .NET SDK 8.0 or greater
- a copy of the 7 Days to Die dedicated server assemblies

### Get the game assemblies

The project expects local game references in `deps/`:

- `0Harmony.dll`
- `Assembly-CSharp.dll`
- `LogLibrary.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`

See [deps/README.md](deps/README.md) for the supported ways to populate that folder.

The CI workflow pins the game dependency set using the committed values in `.github/7dtd-version.env`.
If you want CI to move to a different 7 Days to Die build, update that file in the same pull request.

### Build

From the repo root:

```bash
./build.sh
```

If you want to invoke MSBuild directly instead:

```bash
dotnet build TimeLoop/TimeLoop.sln
```

### Build directly into a Mods folder

```bash
dotnet build TimeLoop/TimeLoop.sln -p:OutputPath="/path/to/7DaysToDie/Mods/TimeLoop/"
```

## Development Notes

- `deps/` is local-only and ignored by git except for its README.
- `.cache/` and `.tools/` are local helper directories created by the download/build workflow.
- `build.sh` uses `zip` when available and falls back to `python3` if `zip` is not installed.

## Attribution

TimeLoop was originally created by leehil and first published at:

- https://github.com/lehimebe/7dtdTimeLoop

It was later forked and maintained at:

- https://github.com/yuyuimoe/7dtdTimeLoop

This repository continues maintenance from that lineage at:

- https://github.com/Gluck-House/7dtdTimeLoop

## License

This repository is licensed under the terms in [LICENSE](LICENSE).

# Build Dependencies

This folder is for local build-time game assemblies only.

The DLLs in this folder are ignored by git and should not be committed.

## Required files

Copy these files here:

- `0Harmony.dll`
- `Assembly-CSharp.dll`
- `LogLibrary.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`

Where they come from:

- `0Harmony.dll`: `<Game Folder>/Mods/0_TFP_Harmony`
- all others: `<Game Folder>/7DaysToDie_Data/Managed`

Using the dedicated server files is recommended.

## Automated download

From the repo root:

```bash
STEAMCMD_MODE=docker ./scripts/download_7dtd_server.sh
```

That script will:

- bootstrap a local `steamcmd` when needed
- download the 7 Days to Die dedicated server into `.cache/7dtd-dedicated-server`
- copy the required DLLs into this folder

Useful overrides:

```bash
BRANCH=latest_experimental ./scripts/download_7dtd_server.sh
STEAM_USER=your_user STEAM_PASS=your_pass ./scripts/download_7dtd_server.sh
SERVER_DIR=/path/to/existing/server ./scripts/download_7dtd_server.sh
DEPS_DIR=/custom/deps/path ./scripts/download_7dtd_server.sh
```

## Notes

- Docker mode avoids needing a host `steamcmd` install.
- Host `steamcmd` on Linux may require 32-bit glibc/Steam runtime support.
- CI pins the expected game build in `.github/7dtd-version.env` and caches `deps/` against that file.

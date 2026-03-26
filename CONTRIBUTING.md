# Contributing

## Local setup

1. Populate `deps/` with the required 7 Days to Die assemblies.
2. Run `./build.sh` from the repository root.
3. Test the resulting mod from `TimeLoop/build/TimeLoop/`.

To download the required assemblies automatically:

```bash
STEAMCMD_MODE=docker ./scripts/download_7dtd_server.sh
```

## Notes

- `deps/`, `.cache/`, `.tools/`, and `TimeLoop/build/` are local-only and ignored by git.
- CI is responsible for producing the distributable `TimeLoop.zip` and `TimeLoop.tar.gz` archives.
- Keep changes portable. Avoid absolute filesystem paths and machine-specific assumptions in docs or scripts.
- The project currently builds with .NET SDK 8 while targeting `netstandard2.1` for the game-facing assembly.

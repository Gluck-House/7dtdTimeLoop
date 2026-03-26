#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
dotnet_cmd="${DOTNET:-dotnet}"
build_output_dir="${BUILD_OUTPUT_DIR:-$repo_root/TimeLoop/build/TimeLoop}"

if ! command -v "$dotnet_cmd" >/dev/null 2>&1; then
    if [ -x "$HOME/.local/share/dotnet/dotnet" ]; then
        dotnet_cmd="$HOME/.local/share/dotnet/dotnet"
    else
        echo "dotnet SDK not found. Install .NET SDK 8+ or set DOTNET=/full/path/to/dotnet." >&2
        exit 1
    fi
fi

"$dotnet_cmd" build "$repo_root/TimeLoop/TimeLoop.sln" "$@"

if [ ! -d "$build_output_dir" ]; then
    echo "Build output directory not found: $build_output_dir" >&2
    exit 1
fi

echo "Build output available at: $build_output_dir"

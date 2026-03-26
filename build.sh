#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
dotnet_cmd="${DOTNET:-dotnet}"
build_output_dir="${BUILD_OUTPUT_DIR:-$repo_root/TimeLoop/build/TimeLoop}"
zip_root_dir="${ZIP_ROOT_DIR:-timeloop}"
zip_path="${ZIP_PATH:-$repo_root/TimeLoop/build/TimeLoop.zip}"

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

staging_dir="$(mktemp -d)"
trap 'rm -rf "$staging_dir"' EXIT

mkdir -p "$(dirname "$zip_path")" "$staging_dir/$zip_root_dir"
cp -R "$build_output_dir"/. "$staging_dir/$zip_root_dir/"

rm -f "$zip_path"

if command -v zip >/dev/null 2>&1; then
    (
        cd "$staging_dir"
        zip -rq "$zip_path" "$zip_root_dir"
    )
elif command -v python3 >/dev/null 2>&1; then
    STAGING_DIR="$staging_dir" ZIP_ROOT_DIR="$zip_root_dir" ZIP_PATH="$zip_path" python3 <<'PY'
import os
import pathlib
import zipfile

staging_dir = pathlib.Path(os.environ["STAGING_DIR"])
zip_root_dir = pathlib.Path(os.environ["ZIP_ROOT_DIR"])
zip_path = pathlib.Path(os.environ["ZIP_PATH"])
source_root = staging_dir / zip_root_dir

with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED) as zf:
    zf.write(source_root, zip_root_dir.as_posix() + "/")
    for path in sorted(source_root.rglob("*")):
        arcname = path.relative_to(staging_dir).as_posix()
        if path.is_dir():
            zf.write(path, arcname + "/")
        else:
            zf.write(path, arcname)
PY
else
    echo "Neither 'zip' nor 'python3' is available to create $zip_path." >&2
    exit 1
fi

echo "Created archive: $zip_path"

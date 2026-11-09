#!/usr/bin/env bash
set -euo pipefail

site_dir="${1:?Usage: prepare-pages.sh <published wwwroot> <base path, e.g. /work-todo/>}"
base_path="${2:?Usage: prepare-pages.sh <published wwwroot> <base path, e.g. /work-todo/>}"
index="$site_dir/index.html"

sed -i "s|<base href=\"/\" />|<base href=\"$base_path\" />|" "$index"
grep -q "<base href=\"$base_path\" />" "$index" || {
    echo "Base path was not applied: no <base href=\"/\" /> found in $index" >&2
    exit 1
}

find "$site_dir" -type f \( -name '*.br' -o -name '*.gz' \) -delete

cp "$index" "$site_dir/404.html"
touch "$site_dir/.nojekyll"

#!/bin/bash
# Runs AMD's Windows FidelityFX_SC.exe from WSL, translating /mnt/c paths.
exe="$(dirname "$0")/../ffxsdk/sdk/tools/binary_store/FidelityFX_SC.exe"
args=()
for a in "$@"; do
  if [[ "$a" == *"/mnt/"[a-z]"/"* ]]; then
    prefix="${a%%/mnt/*}"
    path="/mnt/${a#*/mnt/}"
    a="${prefix}$(wslpath -w "$path")"
  fi
  args+=("$a")
done
exec "$exe" "${args[@]}"

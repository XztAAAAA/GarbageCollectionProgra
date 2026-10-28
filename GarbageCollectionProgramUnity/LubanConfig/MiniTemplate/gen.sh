#!/bin/bash

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
LUBAN_DLL="$SCRIPT_DIR/Luban/Luban/Luban.dll"
CONF_ROOT="$SCRIPT_DIR"
OUTPUT_CODE_DIR="$SCRIPT_DIR/../../Assets/_Project/Scripts/Luban"
OUTPUT_DATA_DIR="$SCRIPT_DIR/../../Assets/Resources/Luban"

cd "$SCRIPT_DIR"

mkdir -p "$OUTPUT_CODE_DIR" "$OUTPUT_DATA_DIR"

dotnet "$LUBAN_DLL" \
    -t all \
    -c cs-simple-json \
    -d json \
    --conf "$CONF_ROOT/luban.conf" \
    -x "outputCodeDir=$OUTPUT_CODE_DIR" \
    -x "outputDataDir=$OUTPUT_DATA_DIR"

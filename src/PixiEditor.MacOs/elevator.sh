#!/bin/bash

# Get the real path to the updater
EXECUTABLE="$1"
shift

# Variables for postExecute
POST_EXECUTE=""

# Rebuild the argument list without --postExecute and its argument
ARGS=()
while [[ $# -gt 0 ]]; do
    case "$1" in
        --postExecute)
            shift
            POST_EXECUTE="$1"
            shift
            ;;
        *)
            ARGS+=("$1")
            shift
            ;;
    esac
done

# Run the updater
"$EXECUTABLE" "${ARGS[@]}"

# Run the postExecute command if set
if [[ -n "$POST_EXECUTE" ]]; then
    eval "$POST_EXECUTE"
fi
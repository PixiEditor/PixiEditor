#!/usr/bin/env bash
# Find flatpak even if PATH is not set properly
FLATPAK_BIN="$(command -v flatpak || echo /usr/bin/flatpak)"
exec "$FLATPAK_BIN" run --file-forwarding net.pixieditor.PixiEditor @@ "$@" @@

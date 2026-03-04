#!/usr/bin/env bash

# Resolve workspace path (Dev Containers mount your repo under /workspaces/<name>)
WORKSPACE_DIR="${WORKSPACE_DIR:-${PWD}}"

# Point to your repo's mise.toml (adjust if stored elsewhere)
MISE_FILE="$WORKSPACE_DIR/mise.toml"

if [[ -f "$MISE_FILE" ]]; then
  /usr/local/bin/mise trust "$MISE_FILE"
else
  echo "WARN: $MISE_FILE not found. Skipping mise install."
fi

eval "$(mise activate)"

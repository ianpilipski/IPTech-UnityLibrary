#!/bin/bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WORKSPACE_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"

cp "$SCRIPT_DIR/lerna.json" "$WORKSPACE_DIR/lerna.json"
trap 'rm -f "$WORKSPACE_DIR/lerna.json"' EXIT

cd "$WORKSPACE_DIR"
npx lerna publish from-package --registry http://localhost:4873 --no-push --no-private

# Reset the package.json files to their original state after publishing (lerna injects gitHead into them)
git checkout -- packages/**/package.json



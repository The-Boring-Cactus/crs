#!/usr/bin/env bash
# deploy.sh — Build and publish CRS Reporter for production (Linux/macOS)
#
# Usage:
#   ./deploy.sh [output-dir]
#
# Default output: ./dist
#
# Requirements:
#   - Node.js + npm  (for the Vue UI)
#   - .NET 10 SDK    (for the C# server)
#
# After running this script, start the server with:
#   cd dist && CRS_ENV=production ./Server
#
# Optional env vars at runtime:
#   CRS_PORT=8080    — listen on a different port (default: 9876)
#   CRS_ENV=production — disables developer error pages
#
# SPA deep-link note:
#   Vue Router uses HTML5 history mode. Direct navigation to /pages/... works
#   only if something rewrites unknown paths to index.html. The embedded GenHTTP
#   server does NOT do this automatically. For production, place nginx or Caddy
#   in front:
#
#   nginx snippet:
#     location / {
#       proxy_pass         http://127.0.0.1:9876;
#       proxy_http_version 1.1;
#       proxy_set_header   Upgrade $http_upgrade;
#       proxy_set_header   Connection "upgrade";
#       proxy_set_header   Host $host;
#       # SPA fallback — comment out if you prefer the server to return 404
#       proxy_intercept_errors on;
#       error_page 404 = /index.html;
#     }
#
# ─────────────────────────────────────────────────────────────────────────────

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="${1:-$SCRIPT_DIR/dist}"

echo ""
echo "╔══════════════════════════════════════════╗"
echo "║   CRS Reporter — Production Build        ║"
echo "╚══════════════════════════════════════════╝"
echo ""
echo "  Output: $OUTPUT_DIR"
echo ""

# ── 1. Vue UI ─────────────────────────────────────────────────────────────────
echo "▶ [1/2] Building Vue UI for production..."
cd "$SCRIPT_DIR/ui"
npm ci --prefer-offline
npm run build   # vite outputs to ../Resources (configured in vite.config.mjs)
echo "  ✓ Vue build complete → $SCRIPT_DIR/Resources"
echo ""

# ── 2. .NET server ────────────────────────────────────────────────────────────
echo "▶ [2/2] Publishing .NET server (Release)..."
cd "$SCRIPT_DIR"
dotnet publish Server/Server.csproj \
    --configuration Release \
    --output "$OUTPUT_DIR" \
    --no-self-contained
echo "  ✓ Server published → $OUTPUT_DIR"
echo ""

# ── Done ──────────────────────────────────────────────────────────────────────
echo "╔══════════════════════════════════════════╗"
echo "║   Build complete!                        ║"
echo "╚══════════════════════════════════════════╝"
echo ""
echo "  Start the server:"
echo "    cd \"$OUTPUT_DIR\""
echo "    CRS_ENV=production ./Server"
echo ""
echo "  Or with a custom port:"
echo "    CRS_PORT=8080 CRS_ENV=production ./Server"
echo ""

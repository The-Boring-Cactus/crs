#!/usr/bin/env bash
# Entrypoint for the CRS Reporter all-in-one image: starts PostgreSQL, creates
# the application database/role on first run, starts the CRS server, and — if
# it isn't configured yet — calls the app's own /api/setup/complete endpoint
# to create the schema and the initial admin user. Reuses the app's real
# password-hashing logic instead of reimplementing it here.
set -Eeo pipefail

log() { echo "[entrypoint] $*"; }

CRS_PORT="${CRS_PORT:-9876}"
CRS_CONFIG_DIR="${CRS_CONFIG_DIR:-/config}"

for v in POSTGRES_PASSWORD CRS_ADMIN_PASSWORD; do
    if [ "${!v}" = "changeme" ]; then
        log "WARNING: ${v} is left at the default 'changeme'. Set it with -e ${v}=... for anything beyond local experimentation."
    fi
done

# ---------------------------------------------------------------------------
# Keep config.json on its own persistent volume so it survives image
# upgrades/recreation independently of the /app layer.
# ---------------------------------------------------------------------------
mkdir -p "$CRS_CONFIG_DIR"
chown -R postgres:postgres "$CRS_CONFIG_DIR" 2>/dev/null || true
if [ ! -e /app/config.json ]; then
    ln -s "$CRS_CONFIG_DIR/config.json" /app/config.json
fi

# ---------------------------------------------------------------------------
# Start PostgreSQL (the postgresql-common package already created a default
# cluster during `apt-get install`; /var/lib/postgresql is a volume so its
# data persists across container recreation).
# ---------------------------------------------------------------------------
PG_VERSION="$(pg_lsclusters -h | awk '{print $1}' | sort -n | tail -1)"
if [ -z "$PG_VERSION" ]; then
    log "ERROR: no PostgreSQL cluster found (pg_lsclusters returned nothing)."
    exit 1
fi
PG_CLUSTER="main"

log "Starting PostgreSQL ${PG_VERSION}/${PG_CLUSTER}..."
pg_ctlcluster "$PG_VERSION" "$PG_CLUSTER" start

shutdown() {
    log "Shutting down..."
    if [ -n "${SERVER_PID:-}" ] && kill -0 "$SERVER_PID" 2>/dev/null; then
        kill -TERM "$SERVER_PID" 2>/dev/null || true
        wait "$SERVER_PID" 2>/dev/null || true
    fi
    pg_ctlcluster "$PG_VERSION" "$PG_CLUSTER" stop -m fast || true
    exit 0
}
trap shutdown SIGTERM SIGINT

# Wait for PostgreSQL to accept connections.
for i in $(seq 1 30); do
    if gosu postgres pg_isready -q; then break; fi
    sleep 1
done

# ---------------------------------------------------------------------------
# Create the application role + database (idempotent — safe on every start).
# ---------------------------------------------------------------------------
if ! gosu postgres psql -tAc "SELECT 1 FROM pg_roles WHERE rolname='${POSTGRES_USER}'" | grep -q 1; then
    log "Creating database role '${POSTGRES_USER}'..."
    gosu postgres psql -c "CREATE ROLE \"${POSTGRES_USER}\" LOGIN PASSWORD '${POSTGRES_PASSWORD}';"
else
    # Keep the role's password in sync with POSTGRES_PASSWORD on every start.
    gosu postgres psql -c "ALTER ROLE \"${POSTGRES_USER}\" WITH PASSWORD '${POSTGRES_PASSWORD}';" >/dev/null
fi
if ! gosu postgres psql -tAc "SELECT 1 FROM pg_database WHERE datname='${POSTGRES_DB}'" | grep -q 1; then
    log "Creating database '${POSTGRES_DB}'..."
    gosu postgres psql -c "CREATE DATABASE \"${POSTGRES_DB}\" OWNER \"${POSTGRES_USER}\";"
fi

# ---------------------------------------------------------------------------
# Start the CRS application server.
# ---------------------------------------------------------------------------
log "Starting CRS server on port ${CRS_PORT}..."
/app/Server &
SERVER_PID=$!

for i in $(seq 1 60); do
    if curl -sf "http://127.0.0.1:${CRS_PORT}/api/setup/status" >/dev/null 2>&1; then
        break
    fi
    if ! kill -0 "$SERVER_PID" 2>/dev/null; then
        log "ERROR: CRS server exited before it came up."
        wait "$SERVER_PID"
        exit 1
    fi
    sleep 1
done

# ---------------------------------------------------------------------------
# First-run setup: create the schema and the initial admin user via the
# app's own /api/setup/complete endpoint (so the real password-hashing path
# is used, not a reimplementation of it here). No-ops on later restarts.
# ---------------------------------------------------------------------------
STATUS_JSON="$(curl -sf "http://127.0.0.1:${CRS_PORT}/api/setup/status" || echo '{}')"
CONFIGURED="$(echo "$STATUS_JSON" | jq -r '.configured // false')"

if [ "$CONFIGURED" != "true" ]; then
    log "Running first-time setup (admin user: ${CRS_ADMIN_USERNAME})..."
    SETUP_PAYLOAD="$(jq -n \
        --arg dbHost "localhost" \
        --argjson dbPort 5432 \
        --arg dbName "$POSTGRES_DB" \
        --arg dbUser "$POSTGRES_USER" \
        --arg dbPass "$POSTGRES_PASSWORD" \
        --arg adminUser "$CRS_ADMIN_USERNAME" \
        --arg adminFullName "$CRS_ADMIN_FULLNAME" \
        --arg adminEmail "$CRS_ADMIN_EMAIL" \
        --arg adminPassword "$CRS_ADMIN_PASSWORD" \
        '{
            Database: { Type: "postgresql", Host: $dbHost, Port: $dbPort, DatabaseName: $dbName, Username: $dbUser, Password: $dbPass },
            AdminUser: { Username: $adminUser, FullName: $adminFullName, Email: $adminEmail },
            AdminPassword: $adminPassword
        }')"

    RESULT="$(curl -sf -X POST "http://127.0.0.1:${CRS_PORT}/api/setup/complete" \
        -H "Content-Type: application/json" \
        -d "$SETUP_PAYLOAD" || echo '{"success":false,"message":"request to /api/setup/complete failed"}')"

    if [ "$(echo "$RESULT" | jq -r '.success // false')" = "true" ]; then
        log "Setup complete — admin user '${CRS_ADMIN_USERNAME}' created."
    else
        log "WARNING: setup did not complete successfully: $(echo "$RESULT" | jq -r '.message // "unknown error"')"
    fi
else
    log "Already configured — skipping first-run setup."
fi

wait "$SERVER_PID"

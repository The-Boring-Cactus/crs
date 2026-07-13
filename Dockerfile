# syntax=docker/dockerfile:1
#
# CRS Reporter — single image containing the Vue UI, the .NET 10 server, and a
# PostgreSQL server. Everything needed to run the app lives in one container;
# PostgreSQL is only reachable from inside it (nothing binds 5432 externally).
#
# Build:
#   docker build -t crs-reporter .
#
# Run (first start creates the database and the initial admin user from the
# environment variables below — see the ENV block in the final stage, or pass
# your own with `docker run -e` / an env file):
#   docker run -d --name crs \
#     -p 9876:9876 \
#     -e POSTGRES_PASSWORD=change-me \
#     -e CRS_ADMIN_PASSWORD=change-me-too \
#     -v crs-db-data:/var/lib/postgresql \
#     -v crs-config:/config \
#     crs-reporter
#
# Mount both volumes for persistence across container recreation:
#   - /var/lib/postgresql  → the database
#   - /config              → config.json (DB connection + admin user metadata)
# Losing /config while keeping /var/lib/postgresql is recoverable but requires
# manual intervention (the Users table already has the admin row, but the
# container will try to re-run first-time setup and fail on the duplicate
# username) — keep both volumes together.

# ---------------------------------------------------------------------------
# Stage 1: build the Vue UI (vite.config.mjs outputs to ../Resources)
# ---------------------------------------------------------------------------
FROM node:20-bookworm-slim AS ui-build
WORKDIR /src/ui
COPY ui/package.json ui/package-lock.json ./
RUN npm ci
COPY ui/ ./
RUN npm run build

# ---------------------------------------------------------------------------
# Stage 2: publish the .NET server
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS server-build
WORKDIR /src
COPY . .
# Overlay the freshly built UI on top of whatever Resources/ is committed.
COPY --from=ui-build /src/Resources ./Resources
RUN dotnet publish Server/Server.csproj \
        --configuration Release \
        --output /app/publish \
        --no-self-contained

# ---------------------------------------------------------------------------
# Stage 3: runtime — .NET runtime + PostgreSQL server, single container
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/runtime:10.0

ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        postgresql postgresql-contrib gosu curl jq ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=server-build /app/publish ./
COPY docker/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# --- Fixed runtime settings ---
ENV CRS_PORT=9876 \
    CRS_ENV=production \
    CRS_CONFIG_DIR=/config

# --- Configurable via `docker run -e` / --env-file / compose ---
# Change these for anything beyond local experimentation.
ENV POSTGRES_DB=crs_db \
    POSTGRES_USER=crs_user \
    POSTGRES_PASSWORD=changeme \
    CRS_ADMIN_USERNAME=admin \
    CRS_ADMIN_FULLNAME=Administrator \
    CRS_ADMIN_EMAIL=admin@example.com \
    CRS_ADMIN_PASSWORD=changeme

VOLUME ["/var/lib/postgresql", "/config"]
EXPOSE 9876

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=5 \
    CMD curl -sf "http://127.0.0.1:${CRS_PORT}/api/setup/status" || exit 1

ENTRYPOINT ["/entrypoint.sh"]

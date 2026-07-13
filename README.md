# CRS Reporter

CRS Reporter is a self-hosted reporting and dashboarding system. Users connect to their
databases, query data with SQL or a custom scripting language, and turn the results into
tables, charts, and interactive dashboards — without needing to know full C# or stand up a
BI stack.

It ships as a single deployable unit: a Vue 3 single-page app served by a self-hosted .NET
backend, talking to whichever relational database you point it at.

## What you can do with it

- **Query your data.** Connect to PostgreSQL, MySQL, SQL Server, or Oracle databases and
  run SQL from the built-in SQL Editor, with results rendered as a table, pivot table, or
  chart.
- **Write report scripts in FunctEngine.** A small C#-like scripting language purpose-built
  for reporting: pull data with `ExecuteQuery`, transform it with array/table/statistics
  functions, and emit `Table()`, `Chart()`, `StatReport()`, `Value()`, `Markdown()`, and
  `Formula()` (MathJax) output — all from one script. See `FUNCTIONS_MANUAL.md` for the
  full function reference (math, string, array, statistics, date/time, financial, and
  design-of-experiments functions).
- **Build dashboards.** A draggable, resizable dashboard builder with text, charts (line,
  bar, pie, scatter, radar, etc. — rendered with ECharts or, optionally, Bokeh), data
  tables, tree tables, images, SQL-backed widgets, and script-backed widgets that can
  auto-refresh on an interval.
- **Edit data directly.** A spreadsheet-style dataset editor (`MyExcel`) for editing tabular
  data by hand.
- **Share dashboards publicly.** Generate a public share link that renders a read-only,
  unauthenticated view of a dashboard — including live SQL/script widgets and
  variable-driven filters.
- **Manage users and connections.** Multi-user accounts (JWT-based auth) and reusable,
  optionally shared database connections.

## How it's built

- **Frontend:** Vue 3 + Pinia + Vite, CodeMirror-based script/SQL editors, ECharts (and an
  optional Bokeh renderer) for charts, Tailwind-based UI components. Source in `ui/`.
- **Backend:** .NET 10, self-hosted with [GenHTTP](https://genhttp.org) (no ASP.NET Core
  dependency) — a WebSocket endpoint at `/srv` for authenticated app traffic, plus REST
  endpoints for setup, auth, and public dashboard sharing. Source in `Server/`.
- **FunctEngine:** a custom interpreter (tokenizer → parser → AST → executor) in
  `FunctEngine/`, extended by pluggable function-library projects (`MathFunctions`,
  `StringUtilities`, `DateTimeFunctions`, `FinancialFunctions`, `DoeFunctions`,
  `DataTableFunctions`, `TimeSeriesFunctions`, `NonParametricFunctions`).
- **Database access:** Dapper + native drivers (Npgsql, MySqlConnector,
  Microsoft.Data.SqlClient, Oracle.ManagedDataAccess) — the app's own metadata (users,
  dashboards, reports, saved connections, datasets) lives in whichever database you
  configure during setup.

## Getting started

However you run it, first launch is the same: open the app in a browser and its **Setup
Wizard** walks you through configuring the database connection and creating the initial
admin user — *unless* you're using the Docker image, which can do that step for you
non-interactively from environment variables (see below).

### Option 1 — Docker (simplest)

The included `Dockerfile` builds one image containing the UI, the .NET server, *and* a
PostgreSQL server, so there's nothing else to stand up.

```bash
docker build -t crs-reporter .

docker run -d --name crs \
  -p 9876:9876 \
  -e POSTGRES_PASSWORD=change-me \
  -e CRS_ADMIN_PASSWORD=change-me-too \
  -v crs-db-data:/var/lib/postgresql \
  -v crs-config:/config \
  crs-reporter
```

On first start, the container creates the PostgreSQL database and an initial admin user
from environment variables (all optional except the two passwords, which default to
`changeme` and log a warning if left unset):

| Variable | Purpose | Default |
|---|---|---|
| `POSTGRES_DB` | database name | `crs_db` |
| `POSTGRES_USER` | app DB role | `crs_user` |
| `POSTGRES_PASSWORD` | app DB role password | `changeme` ⚠️ |
| `CRS_ADMIN_USERNAME` | initial admin login | `admin` |
| `CRS_ADMIN_FULLNAME` | admin display name | `Administrator` |
| `CRS_ADMIN_EMAIL` | admin email | `admin@example.com` |
| `CRS_ADMIN_PASSWORD` | initial admin password | `changeme` ⚠️ |
| `CRS_PORT` | app listen port (container-internal) | `9876` |

Publish the container's port as **host port 9876** (`-p 9876:9876`) — the UI currently
expects to reach the API on that port. Mount both volumes shown above so the database and
`config.json` survive container recreation; losing `/config` while keeping the database
volume will make the container try (and fail) to re-run setup, since the admin user would
already exist. Restarting the container is safe — it detects an existing setup and skips
straight to starting the app.

The image exposes a Docker `HEALTHCHECK` against `/api/setup/status`, so `docker ps` shows
`healthy` once the app is actually serving traffic, not just once the process has started.

### Option 2 — Deployment scripts (bring your own database)

Use this if you already run PostgreSQL/MySQL/SQL Server/Oracle and just want the app
binaries. Requires Node.js + npm and the .NET 10 SDK.

```bash
# Linux/macOS
./deploy.sh [output-dir]      # defaults to ./dist

# Windows
deploy.cmd [output-dir]       # defaults to dist\
```

This builds the Vue UI (into `Resources/`, which the server embeds and serves) and
publishes the .NET server as a framework-dependent build into the output directory. Then:

```bash
cd dist
CRS_ENV=production ./Server        # Linux/macOS
# or
set CRS_ENV=production && Server.exe   # Windows
```

Open `http://localhost:9876` (or `CRS_PORT` if you set one) and complete the Setup Wizard —
it creates the schema in your database and the initial admin user interactively.

**Reverse proxy note:** the SPA uses HTML5 history-mode routing. Direct navigation to a deep
link (e.g. `/pages/dashboard`) needs something in front rewriting unknown paths to
`index.html` — see the nginx/IIS notes at the top of `deploy.sh` / `deploy.cmd`.

### Option 3 — Local development

For working on the app itself:

```bash
# Backend (from the repo root)
dotnet run --project Server

# Frontend, in a separate terminal
cd ui
npm install
npm run dev
```

`ui/.env` points the dev build at `http://localhost:9876` for API/WebSocket calls (the
backend allows cross-origin requests in development), so the Vite dev server (port 5173)
and the .NET backend (port 9876) run side by side without a proxy.

## Configuration reference

| Env var | Applies to | Purpose |
|---|---|---|
| `CRS_PORT` | server | HTTP listen port (default `9876`) |
| `CRS_ENV` | server | set to `production` to disable developer error pages |

Database connection, SMTP, and admin-user metadata are stored in `config.json`, created by
the Setup Wizard (or the Docker entrypoint) next to the server executable.

## License

MIT — see `LICENSE`.

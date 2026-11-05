# FootballSystem

![CI](https://github.com/ninederGdat/FootballSystem/actions/workflows/ci.yml/badge.svg)

A full-stack football data application. A scheduled ETL worker pulls club, player, match, lineup and transfer data from Fotmob into PostgreSQL (Supabase), a read-only ASP.NET Core API serves it, and a React web app presents it.

The system currently tracks two clubs owned by the same group: **Chelsea FC** (Fotmob id `8455`) and **RC Strasbourg Alsace** (`9848`). Players loaned or transferred between them are handled explicitly, so the same data can be viewed from either club's point of view.

**Live demo:** <https://footballsystem.pages.dev> (the API runs on a free plan, so the first request after idle time can be slow)

---

## Features

- **Dashboard** – transfer spending vs revenue for the selected season, permanent signings and sales, loans in and out, contract extensions, squad snapshot, upcoming fixtures, recent results and recent form.
- **Matches** – list with club, competition, season, status and sort filters; match detail with a formation-based lineup (starters and substitutes) and an event timeline (goals and cards).
- **Players** – list filtered by club context, position, nationality, status and relation to the club (`current`, `loaned_in`, `loaned_out`, `transferred`); player detail with loan information and appearance history that shows which club the player played for.
- **Transfers** – per-club view, all-clubs view and an internal Chelsea ↔ Strasbourg view; filters by direction, loan and contract extension; season statistics.
- **Active Club Context** – a sidebar switcher that sets the club the whole web app is viewing (remembered in the browser). Matches and Transfers also allow "All"; Players always has one club.

### Data rules worth knowing

- A player's relation to a club is computed by one shared function in `FootballSystem.Shared`, so the API and the sync worker agree on what "loaned in", "loaned out" and "transferred" mean.
- Contract extensions are not transfers. They are excluded from buy, sell, loan and fee statistics, counted separately as `contractExtensionCount`, and shown in the list with an `EXTENSION` tag.
- Transfer statistics are season-scoped. Seasons are defined in `FootballApi/appsettings.json` (`Seasons`), each with a code, a name and a date range.
- Players who appear in transfers or lineups but are not in a tracked squad are stored as stubs (`is_stub`) and enriched later, so a missing player never blocks a sync phase.

---

## Architecture

```text
GitHub Actions (cron) ──► FotmobSync ──► Supabase (PostgreSQL)
                                              ▲
Cloudflare Pages (React) ──► Render (FootballApi, ASP.NET Core)
```

| Project                 | Role                                                                                                        |
| ----------------------- | ----------------------------------------------------------------------------------------------------------- |
| `FotmobSync`            | ETL worker (.NET). Extracts from Fotmob, maps raw payloads to clean models, upserts into Supabase.          |
| `FootballApi`           | Read-only ASP.NET Core controller API for the frontend. Uses the Supabase anon key.                         |
| `FootballWeb`           | React single-page app.                                                                                      |
| `FootballSystem.Shared` | Models and logic shared by the worker and the API (Supabase models, client factory, player relation rules). |
| `FotmobSync.Tests`      | xUnit tests (seasons, competitions, player relations, transfer statistics, external clients).               |
| `docs/`                 | Database schema, business logic and JSON samples.                                                           |

### Sync workflow

`ClubRefreshWorkflow` runs these phases for all tracked clubs. A failed phase is logged and does not stop the later ones.

1. **Teams** – club records.
2. **Squads** – distinct players across all tracked clubs.
3. **Enrich stub players** – fills in players created as stubs, in batches.
4. **Transfers** – transfer history from every tracked club, merged so a move between two tracked clubs is stored once.
5. **Transfer status** – resolves each player's `transfer_status` from the merged transfers, with the tracked clubs known up front.
6. **Matches** – schedule and results.
7. **Lineup backfill** – lineups, `lineup_players` and match events for finished matches that are missing them.

Player detail pages are scraped with Playwright, because the data lives in the page's `__NEXT_DATA__` script tag. Everything else uses plain `HttpClient`. `FotmobSync` talks to Supabase directly and does not reference the API's repositories.

### API endpoints

| Endpoint                                  | Purpose                                                                                                                           |
| ----------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `GET /api/teams`                          | Tracked clubs (name, logo).                                                                                                       |
| `GET /api/competitions`                   | Competitions.                                                                                                                     |
| `GET /api/seasons`                        | Configured seasons.                                                                                                               |
| `GET /api/matches`                        | Paged matches. Filters: `teamId`, `competitionId`, `season`, `status`, `opponent`, `fromDate`, `toDate`; `sort=asc\|desc`.        |
| `GET /api/matches/{matchId}`              | Match detail.                                                                                                                     |
| `GET /api/matches/{matchId}/lineup`       | Lineup for a match.                                                                                                               |
| `GET /api/players`                        | Paged players. `teamId` is required. Filters: `search`, `positionCode`, `nationality`, `status`, `transferStatus` (relation).     |
| `GET /api/players/{playerId}`             | Player profile.                                                                                                                   |
| `GET /api/players/{playerId}/appearances` | Paged appearance history.                                                                                                         |
| `GET /api/transfers`                      | Paged transfers. Filters: `teamId`, `direction` (`in`\|`out`), `internalOnly`, `onLoan`, `contractExtension`, `season`, and more. |
| `GET /api/transfers/statistics`           | Season statistics for a club (`teamId`, `season`, `onLoan`, `internalOnly`).                                                      |

Swagger is enabled in the Development environment only.

---

## Tech stack

- **Backend:** .NET 10 (C#), ASP.NET Core, Worker Service, Quartz.NET, Playwright
- **Database:** PostgreSQL on Supabase (`supabase-csharp` / `postgrest-csharp`)
- **Frontend:** React 18, TypeScript, Vite, TanStack Query, React Router, Tailwind CSS
- **Hosting:** Cloudflare Pages (web), Render (API, Docker), GitHub Actions (scheduled sync)
- **Data source:** Fotmob

---

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Node.js 22 and npm
- A [Supabase](https://supabase.com/) project with the schema described in `docs/database_schema.md`
- A Fotmob token for the sync worker

The repository contains no SQL scripts, migrations, triggers or database functions. Create the schema from `docs/database_schema.md` and check the deployed constraints yourself before running the sync or the API.

### 1. Configure

Set these as environment variables (or in `appsettings.Development.json`):

```env
Supabase__Url=your_supabase_url
Supabase__AnonKey=your_supabase_anon_key
Supabase__ServiceRoleKey=your_supabase_service_role_key   # sync worker only
Fotmob__XMasToken=your_fotmob_token                       # sync worker only
```

Never give the service role key to the API or the frontend.

### 2. Install the Playwright browser (sync worker)

```bash
dotnet build FotmobSync
pwsh FotmobSync/bin/Debug/net10.0/playwright.ps1 install
```

### 3. Run

```bash
# Sync worker: scheduled with Quartz, or one pass and exit
dotnet run --project FotmobSync
dotnet run --project FotmobSync -- --run-once

# API
dotnet run --project FootballApi

# Web
cd FootballWeb
npm install
cp .env.example .env.local   # set VITE_API_BASE_URL to the API URL
npm run dev
```

The tracked clubs are set in `FotmobSync/Services/FotmobSyncRunner.cs`, and the web app's club list is in `FootballWeb/src/lib/teams.ts`.

### 4. Test

```bash
dotnet test FotmobSync.Tests
```

---

## Deployment

| Component                  | Hosting                           | Notes                                                         |
| -------------------------- | --------------------------------- | ------------------------------------------------------------- |
| Web (`FootballWeb`)        | Cloudflare Pages                  | <https://footballsystem.pages.dev>                            |
| API (`FootballApi`)        | Render (free web service, Docker) | <https://footballsystem.onrender.com>, health check `/health` |
| Sync worker (`FotmobSync`) | GitHub Actions (`--run-once`)     | `.github/workflows/fotmob-sync.yml`                           |
| Database                   | Supabase (PostgreSQL)             | Shared by the API and the worker                              |

> The API runs on Render's free plan and spins down after 15 minutes without traffic. The first request after an idle period can take a while.

**FootballApi (Render)**

- Runtime: Docker; Dockerfile path `FootballApi/Dockerfile`; build context is the repository root (`.`); leave _Root Directory_ empty.
- Health check path: `/health`. The API reads its port from the `PORT` variable that Render provides.
- Environment variables:

| Key                       | Description                                                                         |
| ------------------------- | ----------------------------------------------------------------------------------- |
| `Supabase__Url`           | Supabase project URL                                                                |
| `Supabase__AnonKey`       | Supabase anon key (the API only reads data)                                         |
| `Cors__AllowedOrigins__0` | Deployed frontend origin, e.g. `https://footballsystem.pages.dev` (no trailing `/`) |

**FootballWeb (Cloudflare Pages)**

- Root directory: `FootballWeb`; build command: `npm run build`; output directory: `dist`.
- Build variables: `VITE_API_BASE_URL` (the API base URL, no trailing `/`) and `NODE_VERSION=22`.
- `FootballWeb/public/_redirects` contains `/* /index.html 200` so client-side routes work after a refresh.

**FotmobSync (GitHub Actions)**

- Workflow: `.github/workflows/fotmob-sync.yml`, scheduled weekly (Monday 23:00 UTC) plus manual `workflow_dispatch`.
- Repository secrets: `FOTMOB_XMAS_TOKEN`, `SUPABASE_URL`, `SUPABASE_ANON_KEY`, `SUPABASE_SERVICE_ROLE_KEY`.

---

## Roadmap

- Track player form per match and add an LLM-based layer on top of stored per-match stats.
- A former-players view derived from the transfers table.
- A reconciliation job for transfer data.
- Academy and young-player tracking (no reliable data source yet).

## Data source

Data comes from Fotmob through its unofficial endpoints and page data. The project is for learning and portfolio use, and the endpoints can change without notice.

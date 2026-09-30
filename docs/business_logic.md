# FootballSystem — Core Entities & Business Workflows

## 1. Core Entities

The system's data architecture is organized into three primary groups within the PostgreSQL database.

---

## Master Data Group

### Competitions

Stores information about football tournaments, such as:

- V-League
- Premier League

### Formations

Defines tactical schemes, including:

- 4-3-3
- 3-5-2

### Positions

Categorizes pitch positions, including:

- Position coordinates (`x`, `y`)
- Visual formation rendering support

### PositionRoles

Defines specific tactical roles for players, such as:

- Trequartista
- Box-to-Box Midfielder

---

## Core Entities Group

### Teams

Maintains club records, including:

- Team names
- Logos
- Coaching staff information

### Players

Stores comprehensive player profiles with key data points for AI analysis, including:

- Player status
- Market value
- Tactical profile
- Position metadata

Player status includes `status`, `injury_description`, and `transfer_status`. The `transfers` table stores the transfer history used to resolve tracked-team player status.

---

## Match & Lineup Group

### Matches

Records:

- Match schedules
- Historical results
- Opponent details

### Lineups

Manages match-day squad selections linked to tactical formations.

### LineupPlayers

The most granular entity in the system.

Tracks:

- Which player occupied a specific position/role
- Exact playing time
- Tactical assignment during the match

### MatchEvents

Stores mapped FotMob goals and cards, preserving each source event as `raw_payload`. Other event types are currently skipped by the mapper.

---

# 2. Key Business Workflows

The project focuses on automating the football data lifecycle through several critical workflows.

---

## End-to-End ETL (Extract - Transform - Load)

### Extract

Fetches team and match-detail JSON from FotMob (`/api/data/teams` and `/api/data/matchDetails`). Player details are extracted from the player page's `__NEXT_DATA__` using Playwright.

### Transform

Uses specialized **Mappers** to map raw models into Supabase clean/upsert models. This repository does not include SQL migrations or DDL to verify every deployed constraint.

### Load

Executes **Upsert operations** to reliably populate the Supabase database.

---

## Player Detail Browser Extraction

### Playwright-Based Scraping

Uses a headless Playwright browser to load `https://www.fotmob.com/players/{playerId}` and extract the `__NEXT_DATA__` script.

### Stable Data Extraction

Reads `props.pageProps.data` when its player ID matches, with `props.pageProps.fallback["player:{playerId}"]` as a fallback. The active player sync uses this browser client; the code does not guarantee a CAPTCHA/Turnstile bypass.

---

## Automated Synchronization (Scheduled Refresh)

### Quartz.NET Background Jobs

Uses Quartz.NET `DailyFotmobSyncJob`. The cron defaults to `0 0/30 * * * ?` (every 30 minutes); `RunOnStartup` defaults to true, and checked-in appsettings sets the scheduler delay to 0 seconds. `--run-once` skips Quartz and invokes the sync runner once. The separate `Worker` background service only logs periodically and is not registered by `Program.cs`.

### Dependency Synchronization

Player detail sync upserts the player's primary position and a position role. Lineup sync maps `positionId` to a position code and resolves a default role where `is_default` is true.

### Club Refresh Workflow

`ClubRefreshWorkflow` loads a team snapshot and runs team sync, match sync, squad sync, transfer sync, then transfer-status resolution/sync. `FotmobSyncRunner` currently configures only team ID `8455`.

Match sync upserts fixtures, then backfills finished matches for that team that do not yet have a lineup. `MatchLineupWorkflow` loads match details, syncs the lineup, tracked-team starters/substitutes, and supported events. Match event mapping currently handles goals and cards only.

Squad sync excludes the coach group, fetches each player's detail through the browser client, and waits six seconds between player requests. Player profile sync preserves an existing transfer status; the following workflow resolves status from the latest transfer record, including expired loans.

---

## Automated Data Integrity Maintenance

### Timestamp Behavior

Timestamp assignments are performed by the player and lineup sync paths.

No SQL trigger/function definitions are present in this repository; database-side timestamp behavior must be verified against the deployed schema.

### Delete Behavior

No delete-rule definitions are present in the source files documented here.

Confirm any cascade, set-null, or restrict behavior in the deployed database.

---

## Data Delivery Interface

### FootballApi

The `FootballApi` project is an ASP.NET Core controller-based API that:

- Provides consistent and cleaned data to the frontend
- Shares the same database with the worker service
- Serves as the centralized data access layer

Implemented read routes include:

- `GET /api/matches` (filters: `fromDate`, `toDate`, `season`, `opponent`, `status`, `page`, `pageSize`) and `GET /api/matches/{matchId}`
- `GET /api/matches/{matchId}/lineup`
- `GET /api/players` (filters: `search`, `teamId`, `positionCode`, `nationality`, `transferStatus`, `page`, `pageSize`), `GET /api/players/{playerId}`, and `GET /api/players/{playerId}/appearances`
- `GET /api/competitions` and `GET /api/seasons`
- `GET /api/transfers` (supports player, club, transfer-type, loan, contract-extension, date/season, and pagination filters)
- `GET /api/transfers/statistics` (filters: `season`, `teamId`, `onLoan`)

Season definitions come from the validated `Seasons` options. Match and transfer season filters constrain any supplied date range to the configured season boundaries.

---

# System Architecture Summary

The FootballSystem project is designed around:

- Automated football data collection
- Data normalization
- Tactical modeling
- Referential integrity
- Scheduled synchronization
- Stable API delivery

The architecture combines:

- PostgreSQL
- Supabase
- Quartz.NET
- Playwright
- ASP.NET Core controllers

to synchronize FotMob data and serve it through FootballApi. AI analysis and database-side constraints are not implemented or verifiable from the source files documented here.

## Open Questions

- `QuartzSyncOptions.CronSchedule` defaults to every 30 minutes, while its XML summary comment says every 15 minutes; verify the intended schedule.

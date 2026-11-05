# FootballSystem Database Architecture

This page describes the PostgreSQL (Supabase) schema as it exists in the live database, captured on 2026-11-05 and saved in `db/schema.sql` (tables, constraints and indexes). `db/schema.sql` is the source of truth for DDL; this page explains what the tables are for and how the sync code and API use them.

The C# models in `FootballSystem.Shared/Models/Clean/*.cs` map these tables. The `*Upsert` classes are write projections, not full table definitions: lineup/lineup-player/event/transfer upserts omit generated IDs, and event/transfer upserts also omit clean-model timestamps. Known model differences:

- `LineupPlayerUpsert.RoleId` is `long?` versus `int?` on `LineupPlayerClean` (the column is `bigint`).
- `MatchEventUpsert.Minute` is nullable versus non-nullable on `MatchEventClean` (the column is `NOT NULL`).
- `TransferUpsert.ContractExtension` is nullable versus non-nullable on `TransferClean` (the column is `NOT NULL DEFAULT false`).
- `players.is_stub`, `players.current_team_id`, `lineups.is_complete` and the `lineup_sync_attempts` table exist in the database; check that the clean models map them where the code needs them.

The schema covers:

- PostgreSQL on Supabase hosting
- Real primary keys, foreign keys, unique indexes and query indexes
- Tactical football domain structures
- Match event modeling
- AI-ready extensibility

The schema is divided into three primary logical groups, plus one sync bookkeeping table:

1. Master Data
2. Core Entities
3. Match & Lineup Data
4. Sync bookkeeping

---

# 1. Master Data Group

These tables store foundational reference data shared across the entire system.

| Table            | Fields                                                                       | Description                                                      |
| ---------------- | ---------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| `competitions`   | `competition_id`, `name`, `code`, `last_updated`                             | Stores competition metadata.                                     |
| `positions`      | `position_code`, `position_name`, `x_coord`, `y_coord`, `is_goalkeeper`      | Defines position names and formation coordinates.                |
| `position_roles` | `id`, `position_code`, `role_name`, `role_short`, `is_premium`, `is_default` | Defines specialized and default roles associated with positions. |
| `formations`     | `id`, `name`, `description`, `is_popular`                                    | Stores formations referenced when syncing lineups.               |

## `competitions`

```text
competition_id: bigint, primary key
name: text, not null
code: text, not null, unique
last_updated: timestamptz, default now()
```

Represents a football competition or tournament (Premier League, UEFA Champions League, V-League, ...).

`code` is unique, and `MatchService.EnsureCompetitionsAsync` currently sets it to the competition ID as text.

---

## `positions`

```text
position_code: text, primary key
position_name: text, not null
x_coord: double precision, not null
y_coord: double precision, not null
is_goalkeeper: boolean, default false
```

Defines the standard tactical positions. The coordinates are used for formation visualization, player positioning, tactical reconstruction and lineup rendering.

`players.preferred_position_code`, `lineup_players.position_code` and `position_roles.position_code` reference `positions.position_code` through foreign keys.

---

## `position_roles`

```text
id: bigint, identity (generated always), primary key
position_code: text, foreign key -> positions.position_code
role_name: text, not null
role_short: text
is_premium: boolean, default false
is_default: boolean, not null, default false
```

Defines specialized tactical roles for a position:

```text
CM
├── Box-to-Box
├── Deep Lying Playmaker
├── Carrilero
└── Mezzala
```

Database rules:

- `uk_position_role`: unique `(position_code, role_name)`.
- `ux_position_roles_default`: unique `position_code` where `is_default = true`, so each position has at most one default role.

The sync resolver loads default roles and resolves them by `position_code`.

---

## `formations`

```text
id: bigint, identity (generated always), primary key
name: text, not null, unique
description: text
is_popular: boolean, default true
```

Stores tactical formations (`4-3-3`, `4-2-3-1`, `3-5-2`, `4-4-2`). The lineup sync looks up a formation by `name` (unique) and stores its ID in `lineups.formation_id`.

---

# 2. Core Entities Group

| Table       | Fields                                                                                                                                                                                                                                                                                                                      | Description                         |
| ----------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| `teams`     | `team_id`, `name`, `logo_url`, `coach_name`, `coach_nationality`                                                                                                                                                                                                                                                            | Team and coach metadata.            |
| `players`   | `player_id`, `team_id`, `name`, `shirt_number`, `date_of_birth`, `nationality`, `contract_until`, `market_value`, `status`, `injury_description`, `created_at`, `last_updated`, `preferred_position_code`, `transfer_status`, `is_stub`, `current_team_id`                                                                  | Player profile and transfer status. |
| `transfers` | `id`, `player_id`, `player_name`, `transfer_date`, `has_incomplete_timestamp`, `from_club_id`, `from_club_name`, `to_club_id`, `to_club_name`, `transfer_type`, `on_loan`, `contract_extension`, `fee_value`, `fee_text`, `market_value`, `period_start`, `period_end`, `is_system_generated`, `created_at`, `last_updated` | Transfer history and loan periods.  |

---

## `teams`

```text
team_id: bigint, primary key
name: text, not null
logo_url: text
coach_name: text
coach_nationality: text
```

Represents a football club: club information, logo and coaching staff metadata.

---

## `players`

```text
player_id: bigint, primary key
team_id: bigint, foreign key -> teams.team_id
name: text, not null
shirt_number: integer
date_of_birth: date
nationality: text
contract_until: date
market_value: numeric
status: text, default 'Healthy'
injury_description: text
created_at: timestamptz, default now()
last_updated: timestamptz, default now()
preferred_position_code: text, foreign key -> positions.position_code
transfer_status: text, not null, default 'current'
is_stub: boolean, not null, default false
current_team_id: bigint (no foreign key)
```

Indexes: `ix_players_team_id`, `ix_players_current_team_id`.

### Relationships

```text
One Team
    │
    └──────────► Many Players
```

Linked to `teams`, `positions`, `lineup_players` and `match_events` (as player or assist player).

### Description

Represents a football player and their current squad information.

- Player sync preserves an existing `transfer_status`; the status is resolved separately from transfer history.
- `is_stub` marks players stored as minimal records (default `false`).
- `current_team_id` is indexed but has no foreign key, so it may point to a club that is not in `teams`.
- `team_id` is nullable and has a foreign key to `teams`.

### AI-Critical Fields

```text
market_value
status
preferred_position_code
```

These can support player scouting, tactical recommendation, squad availability analysis, player similarity and valuation analysis.

---

## `transfers`

```text
id: bigint, serial (sequence transfers_id_seq), primary key
player_id: bigint, not null
player_name: text, not null
transfer_date: timestamptz, not null
has_incomplete_timestamp: boolean, not null, default false
from_club_id: bigint, not null
from_club_name: text, not null
to_club_id: bigint, not null
to_club_name: text, not null
transfer_type: text, not null
on_loan: boolean, not null
contract_extension: boolean, not null, default false
fee_value: numeric
fee_text: text
market_value: numeric
period_start: timestamptz
period_end: timestamptz
is_system_generated: boolean, not null, default false
created_at: timestamptz, not null, default now()
last_updated: timestamptz, not null, default now()
```

Database rules:

- `uq_transfers_natural_key`: unique `(player_id, transfer_date, to_club_id)`. This is the sync upsert conflict target.
- No foreign keys: from/to clubs may be teams that are not stored in `teams`, and `player_id` is not linked to `players`.
- Indexes: `idx_transfers_player_id`, `idx_transfers_from_club_id`, `idx_transfers_to_club_id`, and `idx_transfers_period_end` (partial, only `on_loan = true`, used to find expiring loans).

`transfer_type` is not constrained by a CHECK; allowed values are enforced by the mapper.

---

# 3. Match & Lineup Group

These tables manage matches, tactical lineups, player participation and match events.

---

## `matches`

```text
match_id: bigint, primary key
team_id: bigint, foreign key -> teams.team_id
opponent_team_id: bigint (no foreign key)
opponent_name: text, not null
competition_id: bigint, foreign key -> competitions.competition_id
competition_name: text
match_date: timestamptz, not null
home_or_away: text, CHECK in ('home', 'away')
score_home: integer
score_away: integer
status: text, not null, default 'UPCOMING'
last_updated: timestamptz, default now()
```

Stores schedules, results, opponent details, competition metadata and status for the tracked team.

### Opponent Design

`opponent_team_id` has no foreign key: opponents are an external/optional team reference. Sync stores the opponent identifier and name from FotMob, and the API uses `opponent_name` for search and display.

```text
matches
    │
    ├── team_id ──────────► teams.team_id (foreign key)
    │
    ├── competition_id ───► competitions.competition_id (foreign key)
    │
    └── opponent_team_id ─► external / optional team reference (no foreign key)
```

Indexes: only the primary key. There is no index on `team_id`, `competition_id` or `match_date` yet; consider adding them if match queries slow down.

---

## `lineups`

```text
id: bigint, identity (generated always), primary key
match_id: bigint, foreign key -> matches.match_id
type: text, not null
formation_id: bigint, foreign key -> formations.id
is_complete: boolean, not null, default false
created_at: timestamptz, default now()
updated_at: timestamptz, default now()
```

Database rules:

- `uq_lineups_match`: unique `match_id`, so there is at most one lineup per match. This is the sync upsert conflict target.

### Relationships

```text
One Match
    │
    └──────────► One Lineup
                    │
                    └──────────► Many Lineup Players
```

The lineup stores the formation used, lineup type, completeness flag and creation/update timestamps.

---

## `lineup_players`

```text
id: bigint, identity (generated always), primary key
lineup_id: bigint, foreign key -> lineups.id
player_id: bigint, foreign key -> players.player_id
position_code: text, foreign key -> positions.position_code
role_id: bigint, foreign key -> position_roles.id
shirt_number: integer
is_starter: boolean, default true
minute_in: integer
minute_out: integer
custom_x: double precision
custom_y: double precision
```

Database rules:

- `uq_lineup_player`: unique `(lineup_id, player_id)`. This is the sync upsert conflict target.
- There is no separate index on `player_id`; per-player appearance queries rely on foreign-key lookups without index support.

### Description

The most granular tactical entity in the lineup model. It represents one player's participation in a specific match lineup:

- Player
- Tactical position and role
- Playing time
- Pitch coordinates
- Starting/substitute status

```text
Starter / Substitute  ->  is_starter
Playing time          ->  minute_in, minute_out
Position              ->  position_code
Tactical Role         ->  role_id
Pitch Position        ->  custom_x, custom_y
```

The combination of `is_starter`, `minute_in` and `minute_out` allows the system to reconstruct participation during a match, supporting substitution tracking, playing-time calculation, starting XI analysis and tactical lineup reconstruction.

---

# 4. Match Events

## `match_events`

```text
id: bigint, serial (sequence match_events_id_seq), primary key
match_id: bigint, not null, foreign key -> matches.match_id
fotmob_event_id: bigint, not null, unique
event_type: text, not null
event_order: integer, not null
minute: integer, not null
stoppage_time: integer
team_id: bigint, foreign key -> teams.team_id
player_id: bigint, foreign key -> players.player_id
assist_player_id: bigint, foreign key -> players.player_id
description_key: text
raw_payload: jsonb, not null
created_at: timestamptz, not null, default now()
last_updated: timestamptz, not null, default now()
```

Indexes:

- `match_events_fotmob_event_id_key`: unique `fotmob_event_id` (sync upsert conflict target)
- `idx_match_events_match_event_type`: `(match_id, event_type)`
- `idx_match_events_player_id`: `player_id`
- `idx_match_events_team_id`: `team_id`

### Description

Stores detailed events occurring during a football match. The table preserves both normalized event information and the original FotMob payload.

`event_type` is a plain `text` column with no CHECK constraint or database enum; the mapper is the only place that validates it. Currently mapped event types are:

```text
goal
yellow
yellow_red
red
own_goal
```

The mapper ignores event types other than FotMob `Goal` and `Card`; it does not map added-time or substitution events into `match_events`.

---

## Event Identification

Events are upserted by `fotmob_event_id`, which is unique in the database, so duplicates are prevented. `event_order` stores the source array index and is used as a stable tie-breaker in timeline reads.

---

## Event Timing

```text
minute
stoppage_time
```

For example `minute = 90` and `stoppage_time = 4` represent `90+4`. This preserves additional-time information instead of flattening the event into a single integer.

---

## Event Participants

```text
team_id
player_id
assist_player_id
```

These columns are nullable and each has a foreign key (`team_id` -> `teams`, `player_id` and `assist_player_id` -> `players`). Sync assigns `team_id` only for the followed team and assigns player/assist IDs only when the referenced players exist locally, which keeps the foreign keys valid. Opponent names remain available in `raw_payload`.

---

## Raw Event Preservation

The event's source JSON is stored in `raw_payload`, a `jsonb NOT NULL` column. The mapper assigns the original event JSON text to it.

It allows the system to:

- Preserve source information
- Debug mapper behavior
- Reprocess events
- Add new normalized fields later
- Compare normalized data with the original FotMob response
- Query inside the payload with PostgreSQL JSON operators

---

## Match Event Performance

The API orders match events by `minute` and then `event_order`. Existing indexes cover lookups by match (through the `(match_id, event_type)` index), player and team. There is no index on `(match_id, minute, event_order)`; the sort happens on the rows of a single match, which is small.

```text
Match Timeline
    │
    ├── Goals
    ├── Cards
    ├── Own Goals
```

---

# 5. Sync Bookkeeping

## `lineup_sync_attempts`

```text
match_id: bigint, primary key (no foreign key)
attempts: integer, not null, default 0
last_attempt_at: timestamptz, not null, default now()
```

Records how many times lineup sync was attempted for a match and when the last attempt happened. It supports the lineup backfill for finished matches without lineups, so a match that keeps failing is not retried endlessly.

---

# 6. Entity Relationships

The relationships below are enforced by database foreign keys unless marked otherwise.

## Team → Players

```text
One Team ──► Many Players        (players.team_id, foreign key)
```

`players.current_team_id` is an additional indexed team reference without a foreign key.

## Competition → Matches

```text
One Competition ──► Many Matches (matches.competition_id, foreign key)
```

## Team → Matches

```text
One Team ──► Many Matches        (matches.team_id, foreign key)
```

The tracked team is represented by `matches.team_id`.

## Match → Lineup

```text
One Match ──► One Lineup         (lineups.match_id, foreign key + unique index)
```

## Lineup → LineupPlayers

```text
One Lineup ──► Many Lineup Players  (lineup_players.lineup_id, foreign key)
```

A player appears at most once per lineup (`uq_lineup_player`).

## Position → PositionRoles

```text
One Position ──► Many Tactical Roles (position_roles.position_code, foreign key)
```

Each position has at most one default role.

## Match → MatchEvents

```text
One Match ──► Many Match Events  (match_events.match_id, foreign key)
```

## Player → MatchEvents

```text
One Player
    │
    ├──► Many Events as Player        (match_events.player_id)
    └──► Many Events as Assist Player (match_events.assist_player_id)
```

## Relationships without foreign keys

```text
matches.opponent_team_id      external / optional team reference
players.current_team_id       team reference, indexed
transfers.player_id           not linked to players
transfers.from_club_id        not linked to teams
transfers.to_club_id          not linked to teams
lineup_sync_attempts.match_id not linked to matches
```

---

# 7. Referential Integrity

The database declares foreign keys for `players`, `matches`, `lineups`, `lineup_players`, `match_events` and `position_roles` (see section 6). None of them declares an `ON DELETE` rule, so all use the default `NO ACTION`:

- Deleting a team, player, match, lineup, position, role or formation that is still referenced is rejected.
- Nothing cascades and nothing is set to null automatically.
- The application does not implement deletes for these entities.

`transfers`, `lineup_sync_attempts` and the external references listed above are intentionally loose, because they can refer to clubs or matches that are not stored locally.

---

# 8. Database Automation

Column defaults are known: `now()` for `created_at` / `last_updated` / `updated_at` where listed above, `'Healthy'` for `players.status`, `'current'` for `players.transfer_status`, `'UPCOMING'` for `matches.status`.

Whether PostgreSQL triggers or functions refresh `last_updated` on update is not confirmed: the captured schema contains tables and indexes only. In code, `PlayerMapper`/`PlayerSyncService` set player timestamps and `LineupSyncService` sets lineup timestamps before upsert. To check for triggers on the live database:

```sql
select event_object_table, trigger_name, action_timing, event_manipulation
from information_schema.triggers
where trigger_schema = 'public'
order by 1, 2;
```

---

# 9. Tactical Design Philosophy

The schema is designed around tactical football modeling rather than simple sports statistics.

```text
Positions
    │
    ▼
PositionRoles
    │
    ▼
Formations
    │
    ▼
Lineups
    │
    ▼
LineupPlayers
```

enables:

- Formation visualization
- Tactical reconstruction
- Role-based analysis
- Match replay
- Player positioning
- Playing-time analysis
- Substitution analysis
- AI tactical reasoning

`match_events` extends this model from static lineup information into a chronological match representation:

```text
Formation + Player Position + Tactical Role + Playing Time + Match Events
    ↓
Match Tactical Context
```

---

# 10. Match Event Design Philosophy

`match_events` acts as the event layer of the FootballSystem data model, following two principles.

## 1. Normalized Queryable Data

Frequently required fields are dedicated columns: `match_id`, `event_type`, `event_order`, `minute`, `stoppage_time`, `team_id`, `player_id`, `assist_player_id`. Indexes on `(match_id, event_type)`, `player_id` and `team_id` support the common queries.

## 2. Raw Source Preservation

The original FotMob payload is preserved in `raw_payload` (`jsonb`):

```text
FotMob Raw Data
       │
       ▼
raw_payload
       │
       ▼
Mapper
       │
       ▼
Normalized Event Fields
```

This makes the ETL pipeline more resilient to changes in the source API.

---

# 11. ETL & Synchronization Architecture

The tables are populated by the FootballSystem synchronization pipeline.

```text
FotMob team/match APIs and player page
    │
    ▼
Snapshots / extracted player JSON
    │
    ▼
Mappers and sync services
    │
    ├──► Team, player, and transfer data
    ├──► Match data
    └──► Lineup and supported match-event data
            │
            ▼
       Clean / upsert models
            │
            ▼
          Supabase
            │
            ▼
      FootballApi controllers
            │
            └──► Frontend clients
```

Upsert conflict targets, all backed by unique indexes in the database:

| Table            | Conflict target                          | Index                              |
| ---------------- | ---------------------------------------- | ---------------------------------- |
| `players`        | `player_id`                              | `players_pkey`                     |
| `matches`        | `match_id`                               | `matches_pkey`                     |
| `lineups`        | `match_id`                               | `uq_lineups_match`                 |
| `lineup_players` | `(lineup_id, player_id)`                 | `uq_lineup_player`                 |
| `match_events`   | `fotmob_event_id`                        | `match_events_fotmob_event_id_key` |
| `transfers`      | `(player_id, transfer_date, to_club_id)` | `uq_transfers_natural_key`         |

For match events the pipeline follows:

```text
FotMob Match Detail `content.matchFacts.events.events[]`
    │
    ▼
MatchEventMapper (Goal/Card only)
    │
    ├── event_type, minute, stoppage_time
    ├── team_id, player_id, assist_player_id
    └── description_key, raw_payload
    │
    ▼
      match_events
```

`MatchEventMapper` maps FotMob Goal and Card records to `goal`, `own_goal`, `yellow`, `yellow_red` or `red`; other event types are skipped.

---

# 12. AI-Ready Extensions

The current schema is designed to support future AI modules. Potential future additions include:

```sql
performance_vector VECTOR
```

for storing performance embeddings, tactical embeddings, player similarity vectors and semantic representations.

Potential use cases: AI scouting, semantic player search, similar player recommendation, tactical clustering, player role classification and match tactical analysis.

---

# 13. Architectural Summary

```text
Real primary keys, foreign keys and unique indexes
          ↓
Tactical Football Modeling
          ↓
Match Event Modeling
          ↓
Automated ETL Synchronization (upsert on unique indexes)
          ↓
Football API data delivery
          ↓
Potential future AI extensions
```

The major domain layers are:

```text
Master Data
    │
    ├── Competitions
    ├── Positions
    ├── PositionRoles
    └── Formations
          │
          ▼
Core Entities
    │
    ├── Teams
    ├── Players
    └── Transfers
          │
          ▼
Match & Tactical Data
    │
    ├── Matches
    ├── Lineups
    ├── LineupPlayers
    ├── MatchEvents
    └── LineupSyncAttempts (sync bookkeeping)
          │
          ▼
Football API
          │
          └──► Frontend
```

---

# Open Questions

- Check whether any database trigger or function updates `last_updated` automatically (query in section 8).
- `matches` has no index besides its primary key; add indexes on `team_id`, `competition_id` or `match_date` if match queries become slow.
- `lineup_players.player_id` has no index of its own; add one if the player appearances query becomes slow.
- Decide whether `event_type` and `transfer_type` should get CHECK constraints, or stay validated by the mappers only.
- Confirm that the clean models map `players.is_stub`, `players.current_team_id` and `lineups.is_complete` where the code needs them.

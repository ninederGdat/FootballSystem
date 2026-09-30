# FootballSystem Database Architecture

This page describes the table and column mappings in `FootballSystem.Shared/Models/Clean/*.cs` and how current services use them. No SQL scripts, EF migrations, or schema configuration are present in this repository, so database constraints and PostgreSQL types not declared by the models are listed as open questions rather than assumed.

The `*Upsert` classes are write projections, not full table definitions: lineup/lineup-player/event/transfer upserts omit generated IDs, and event/transfer upserts also omit clean-model timestamps. `LineupPlayerUpsert.RoleId` is `long?` versus `int?` on `LineupPlayerClean`; `MatchEventUpsert.Minute` is nullable versus non-nullable on `MatchEventClean`; `TransferUpsert.ContractExtension` is nullable versus non-nullable on `TransferClean`.

- PostgreSQL
- Supabase hosting
- Relational identifiers used by application queries
- Tactical football domain structures
- Match event modeling
- AI-ready extensibility

The schema is divided into three primary logical groups:

1. Master Data
2. Core Entities
3. Match & Lineup Data

---

# 1. Master Data Group

These tables store foundational reference data shared across the entire system.

| Table            | Fields                                                                       | Description                                                      |
| ---------------- | ---------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| `competitions`   | `competition_id`, `name`, `code`, `last_updated`                             | Stores competition metadata.                                     |
| `positions`      | `position_code`, `position_name`, `x_coord`, `y_coord`, `is_goalkeeper`      | Defines position names and formation coordinates.                |
| `position_roles` | `id`, `position_code`, `role_name`, `role_short`, `is_premium`, `is_default` | Defines specialized and default roles associated with positions. |
| `formations`     | `id`, `name`, `description`, `is_popular`                                    | Stores formations referenced when syncing lineups.               |

The core entity and match tables are listed in their respective groups below.

## `competitions`

### Fields

```text
competition_id: long (no `[PrimaryKey]` attribute)
name: string
code: string
last_updated: DateTime
```

### Description

Represents a football competition or tournament.

Examples:

- Premier League
- UEFA Champions League
- V-League

`MatchService.EnsureCompetitionsAsync` currently sets `code` to the competition ID as text. The clean model does not declare a unique constraint.

---

## `positions`

### Fields

```text
position_code: string (no `[PrimaryKey]` attribute)
position_name: string
x_coord: double
y_coord: double
is_goalkeeper: bool
```

### Description

Defines the standard tactical positions used throughout the system.

The coordinate fields are used for:

- Formation visualization
- Player positioning
- Tactical reconstruction
- Match lineup rendering

The API and sync code look up positions by `position_code`; players and lineup-player records also store that code. Foreign-key enforcement is not declared in the clean models.

---

## `position_roles`

### Fields

```text
id: int (`[PrimaryKey]`)
position_code: string
role_name: string
role_short: string?
is_premium: bool
is_default: bool
```

### Relationships

```text
Position
    │
    └──────────► Many Position Roles
```

### Description

Defines specialized tactical roles associated with a football position.

Examples:

```text
CM
├── Box-to-Box
├── Deep Lying Playmaker
├── Carrilero
└── Mezzala
```

`PositionRoleClean` declares `id` as its primary key and maps `is_default`. The sync resolver loads default roles and resolves them by `position_code`. A uniqueness constraint on `(position_code, role_name)` is not declared in code.

---

## `formations`

### Fields

```text
id: long (no `[PrimaryKey]` attribute)
name: string
description: string
is_popular: bool
```

### Description

Stores tactical formations used by teams.

Examples:

```text
4-3-3
4-2-3-1
3-5-2
4-4-2
```

The lineup sync looks up a formation by `name` and stores its ID in `lineups.formation_id`. The clean model does not declare a uniqueness constraint on `name`.

---

# 2. Core Entities Group

These tables manage the primary football entities within the system.

| Table       | Fields                                                                                                                                                                                                                                                                                                                      | Description                         |
| ----------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| `teams`     | `team_id`, `name`, `logo_url`, `coach_name`, `coach_nationality`                                                                                                                                                                                                                                                            | Team and coach metadata.            |
| `players`   | `player_id`, `team_id`, `name`, `shirt_number`, `date_of_birth`, `nationality`, `contract_until`, `market_value`, `status`, `injury_description`, `created_at`, `last_updated`, `preferred_position_code`, `transfer_status`                                                                                                | Player profile and transfer status. |
| `transfers` | `id`, `player_id`, `player_name`, `transfer_date`, `has_incomplete_timestamp`, `from_club_id`, `from_club_name`, `to_club_id`, `to_club_name`, `transfer_type`, `on_loan`, `contract_extension`, `fee_value`, `fee_text`, `market_value`, `period_start`, `period_end`, `is_system_generated`, `created_at`, `last_updated` | Transfer history and loan periods.  |

---

## `teams`

### Fields

```text
team_id: long (no `[PrimaryKey]` attribute)
name: string
logo_url: string?
coach_name: string?
coach_nationality: string?
```

### Description

Represents a football club within the system.

Stores:

- Club information
- Logo
- Coaching staff metadata

A team can have many players and matches.

---

## `players`

### Fields

```text
player_id: long (no `[PrimaryKey]` attribute)
team_id: long
name: string
shirt_number: int?
date_of_birth: DateOnly?
nationality: string?
contract_until: DateOnly?
market_value: decimal?
status: string
injury_description: string?
created_at: DateTime
last_updated: DateTime
preferred_position_code: string?
transfer_status: string
```

### Relationships

```text
One Team
    │
    └──────────► Many Players
```

Linked to:

- `teams`
- `positions`
- `lineup_players`
- `match_events`

### Description

Represents a football player and their current squad information.

`PlayerClean` also maps `injury_description` and `transfer_status`. Player sync preserves an existing transfer status; transfer status is resolved separately from transfer history. The model does not declare the foreign keys shown by some application lookups.

### AI-Critical Fields

The following fields are important for future football intelligence features:

```text
market_value
status
preferred_position_code
```

These can support:

- Player scouting
- Tactical recommendation
- Squad availability analysis
- AI-based player similarity
- Player valuation analysis

## `transfers`

### Fields

```text
id: long (`[PrimaryKey]`)
player_id: long
player_name: string
transfer_date: DateTime
has_incomplete_timestamp: bool
from_club_id: long
from_club_name: string
to_club_id: long
to_club_name: string
transfer_type: string
on_loan: bool
contract_extension: bool
fee_value: decimal?
fee_text: string?
market_value: decimal?
period_start: DateTime?
period_end: DateTime?
is_system_generated: bool
created_at: DateTime
last_updated: DateTime
```

### Description

`TransferClean` maps the `transfers` table. Sync upserts with `(player_id, transfer_date, to_club_id)` as its conflict target; the corresponding database constraint is not declared in code.

---

# 3. Match & Lineup Group

These tables manage operational football data, including matches, tactical lineups, player participation, and match events.

---

## `matches`

### Fields

```text
match_id: long (no `[PrimaryKey]` attribute)
team_id: long?
opponent_team_id: long?
opponent_name: string
competition_id: long?
competition_name: string?
match_date: DateTime
home_or_away: string?
score_home: int?
score_away: int?
status: string
last_updated: DateTime?
```

### Description

Stores schedules, results, opponent details, competition metadata, and status for the tracked team.

### Opponent Design

`MatchClean.OpponentTeamId` is nullable. Whether it has a database foreign key is not verifiable from the model.

Sync stores the opponent identifier and name from FotMob.

Therefore:

```text
matches
    │
    ├── team_id ──────────► teams.team_id
    │
    └── opponent_team_id ─► External / optional team reference
```

The API uses `opponent_name` for match search and display.

---

## `lineups`

### Fields

```text
id: long (`[PrimaryKey]`)
match_id: long
type: string
formation_id: long?
created_at: DateTime
updated_at: DateTime
```

### Relationships

```text
One Match
    │
    └──────────► One Lineup
                    │
                    └──────────► Many Lineup Players
```

### Description

Represents the lineup type and formation fetched for a specific match.

Sync upserts a lineup using `match_id` as the conflict target. The model does not establish whether that column is unique in the database:

```text
match_id (upsert conflict target)
```

The lineup stores:

- Formation used
- Lineup type
- Creation timestamp
- Last update timestamp

The `formation_id` references the tactical formation used by the lineup.

---

## `lineup_players`

### Fields

```text
id: long (`[PrimaryKey]`)
lineup_id: long?
player_id: long
position_code: string?
role_id: int? in `LineupPlayerClean`; long? in `LineupPlayerUpsert`
shirt_number: int?
is_starter: bool
minute_in: int?
minute_out: int?
custom_x: double?
custom_y: double?
```

### Description

The most granular tactical entity in the lineup model.

Represents one player's participation within a specific match lineup.

Maps:

- Player
- Tactical position
- Tactical role
- Playing time
- Pitch coordinates
- Starting/substitute status

### Tactical Responsibilities

Tracks:

```text
Starter / Substitute
        │
        ▼
minute_in
        │
        ▼
minute_out

Position
        │
        ▼
position_code

Tactical Role
        │
        ▼
role_id

Pitch Position
        │
        ├── custom_x
        └── custom_y
```

### Playing Time

The combination of:

```text
is_starter
minute_in
minute_out
```

allows the system to reconstruct player participation during a match.

This supports:

- Substitution tracking
- Playing-time calculation
- Starting XI analysis
- Player availability during different match periods
- Tactical lineup reconstruction

---

# 4. Match Events

## `match_events`

### Fields

```text
id: long (`[PrimaryKey]`)
match_id: long
fotmob_event_id: long (upsert conflict target; uniqueness constraint unverified)
event_type: string
event_order: int
minute: int in `MatchEventClean`; int? in `MatchEventUpsert`
stoppage_time: int?
team_id: long?
player_id: long?
assist_player_id: long?
description_key: string?
raw_payload: string
created_at: DateTime
last_updated: DateTime
```

### Description

Stores detailed events occurring during a football match.

The table is designed to preserve both normalized event information and the original Fotmob payload.

The clean model stores `event_type` as a string and the mapper emits the values below. Any database enum or CHECK constraint is not verifiable from this repository.

Currently mapped event types are:

```text
goal
yellow
yellow_red
red
own_goal
```

The mapper currently ignores event types other than FotMob `Goal` and `Card`; it does not map added-time or substitution events into `match_events`.

---

## Event Identification

The event sync upserts by `fotmob_event_id`; the model does not declare a unique constraint. `event_order` stores the source array index and is used as a stable tie-breaker in timeline reads.

```text
fotmob_event_id (upsert conflict target)
```

Duplicate prevention depends on the deployed database supporting the conflict target.

---

## Event Timing

The event timing model consists of:

```text
minute
stoppage_time
```

For example:

```text
minute = 90
stoppage_time = 4
```

represents:

```text
90+4
```

This allows the system to preserve additional-time information instead of flattening the event into a single integer.

---

## Event Participants

The following fields identify entities involved in an event:

```text
team_id
player_id
assist_player_id
```

These model fields are nullable. Sync assigns `team_id` only for the followed team and assigns player/assist IDs only when the referenced players exist locally; opponent names remain available in `raw_payload`.

---

## Raw Event Preservation

The event's serialized source JSON is stored in the model field:

```text
raw_payload (string in `MatchEventClean` and `MatchEventUpsert`)
```

The mapper assigns the original event JSON text to this field. The PostgreSQL column type is not declared in the repository.

It allows the system to:

- Preserve source information
- Debug Mapper behavior
- Reprocess events
- Add new normalized fields later
- Compare normalized data with the original Fotmob response

This is especially useful because the `event_type` is interpreted and validated at the Mapper layer rather than being strictly constrained by PostgreSQL.

---

## Match Event Performance

The API orders match events by `minute` and then `event_order`. No index definition is present in the repository.

```text
Match Timeline
    │
    ├── Goals
    ├── Cards
    ├── Own Goals
```

---

# 5. Entity Relationships

The relationships below describe identifiers queried by application code. No database foreign-key or delete rules can be confirmed from the available clean models.

## Team → Players

```text
One Team
    │
    └──────────► Many Players
```

A team can contain many players.

A player's current team is represented by:

```text
players.team_id
```

---

## Competition → Matches

```text
One Competition
    │
    └──────────► Many Matches
```

A competition can contain many matches.

The relationship is represented by:

```text
matches.competition_id
```

---

## Team → Matches

```text
One Team
    │
    └──────────► Many Matches
```

The tracked team is represented by:

```text
matches.team_id
```

The application does not implement team deletion; deployed database delete behavior is unknown.

---

## Match → Lineup

```text
One Match
    │
    └──────────► One Lineup
```

The API and sync code assume one lineup per match; the database uniqueness constraint is unverified.

```text
match_id (upsert conflict target; database uniqueness unverified)
```

---

## Lineup → LineupPlayers

```text
One Lineup
    │
    └──────────► Many Lineup Players
```

Each lineup can contain multiple players.

---

## Position → PositionRoles

```text
One Position
    │
    └──────────► Many Tactical Roles
```

Example:

```text
CM
├── Box-to-Box
├── Deep Lying Playmaker
├── Carrilero
└── Mezzala
```

---

## Match → MatchEvents

```text
One Match
    │
    └──────────► Many Match Events
```

A match can contain multiple events.

Examples:

```text
Match
 │
 ├── Goal
 ├── Yellow Card
 ├── Own Goal
 └── Red Card
```

The relationship is represented by:

```text
match_events.match_id
```

---

## Player → MatchEvents

```text
One Player
    │
    ├──────────► Many Events as Player
    │
    └──────────► Many Events as Assist Player
```

A player can participate in an event through either:

```text
player_id
```

or:

```text
assist_player_id
```

---

# 6. Referential Integrity

The clean models declare scalar ID columns, but no foreign-key attributes. Services and repositories query related records using identifiers; those code-level relationships do not prove database foreign keys or `ON DELETE` behavior. No SQL schema, migration, trigger, or function files are present in this repository, so the previous CASCADE, SET NULL, and RESTRICT claims cannot be verified here.

---

# 7. Database Automation

The models expose timestamp columns, but database defaults and triggers are not declared in the code. `PlayerMapper`/`PlayerSyncService` set player timestamps and `LineupSyncService` sets lineup timestamps before upsert. Timestamp behavior for other records, and any deployed database-side automation, must be verified against the live schema.

---

# 8. Tactical Design Philosophy

The schema is designed around tactical football modeling rather than simple sports statistics.

The combination of:

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

The addition of `match_events` extends this model from static lineup information into a chronological match representation.

The combined structure allows future analysis such as:

```text
Formation
    +
Player Position
    +
Tactical Role
    +
Playing Time
    +
Match Events
    ↓
Match Tactical Context
```

---

# 9. Match Event Design Philosophy

`match_events` acts as the event layer of the FootballSystem data model.

The design follows two principles:

## 1. Normalized Queryable Data

Frequently required fields are stored as dedicated columns:

```text
match_id
event_type
event_order
minute
stoppage_time
team_id
player_id
assist_player_id
```

This allows efficient querying and analysis.

## 2. Raw Source Preservation

The original Fotmob payload is preserved in:

```text
raw_payload: string in the clean/upsert models; PostgreSQL type unverified
```

Therefore the system maintains both:

```text
Fotmob Raw Data
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

This design makes the ETL pipeline more resilient to changes in the source API.

---

# 10. ETL & Synchronization Architecture

The mapped tables are populated by the FootballSystem synchronization pipeline.

Current data flow:

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

For match events, the pipeline specifically follows:

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

`MatchEventMapper` currently maps FotMob Goal and Card records to `goal`, `own_goal`, `yellow`, `yellow_red`, or `red`; other event types are skipped.

---

# 11. AI-Ready Extensions

The current schema is designed to support future AI modules.

Potential future additions include:

```sql
performance_vector VECTOR
```

for storing:

- Performance embeddings
- Tactical embeddings
- Player similarity vectors
- Semantic representations

Potential use cases:

- AI scouting
- Semantic player search
- Similar player recommendation
- Tactical clustering
- Player role classification
- Match tactical analysis

The existing relational structure provides the foundation for combining structured football data with future vector-based AI features.

---

# 12. Architectural Summary

The current application models and syncs:

```text
Code-level identifiers and lookup relationships
          ↓
Tactical Football Modeling
          ↓
Match Event Modeling
          ↓
Automated ETL Synchronization
          ↓
Database constraints and automation require live-schema verification
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
    └── MatchEvents
          │
          ▼
Football API
          │
          └──► Frontend
```

The implementation models teams, players, transfers, match schedules, lineups, and supported match events. Database constraints, timestamp triggers, and PostgreSQL column types must be checked against the deployed schema.

---

# Open Questions

- Verify deployed primary/foreign keys, PostgreSQL column types, timestamp defaults/triggers, and delete behavior; these are not defined by repository SQL or migrations.
- Verify that deployed unique constraints exist for explicit sync conflict targets: `player_id` on `players`, `match_id` on `matches` and `lineups`, `(lineup_id, player_id)` on `lineup_players`, `fotmob_event_id` on `match_events`, and `(player_id, transfer_date, to_club_id)` on `transfers`.

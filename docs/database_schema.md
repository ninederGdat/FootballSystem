# FootballSystem Database Architecture

Based on the current system architecture and SQL schema design, the `FootballSystem` database is built using:

- PostgreSQL
- Supabase hosting
- Strong relational modeling
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

| Table            | Fields                                                                       | Description                                                                                |
| ---------------- | ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| `competitions`   | `competition_id` (PK), `name`, `code`, `last_updated`                        | Stores tournament metadata such as Premier League or V-League.                             |
| `positions`      | `position_code` (PK), `position_name`, `x_coord`, `y_coord`, `is_goalkeeper` | Defines tactical pitch positions with visual coordinates used for formation visualization. |
| `position_roles` | `id` (PK), `position_code` (FK), `role_name`, `role_short`, `is_premium`     | Defines specialized tactical roles associated with each position.                          |
| `formations`     | `id` (PK), `name`, `description`, `is_popular`                               | Stores tactical formations such as `4-3-3`, `4-2-3-1`, or `3-5-2`.                         |

## `competitions`

### Fields

```text
competition_id (PK)
name
code (UNIQUE)
last_updated
```

### Description

Represents a football competition or tournament.

Examples:

- Premier League
- UEFA Champions League
- V-League

The `code` field is unique and can be used as a stable competition identifier.

---

## `positions`

### Fields

```text
position_code (PK)
position_name
x_coord
y_coord
is_goalkeeper
```

### Description

Defines the standard tactical positions used throughout the system.

The coordinate fields are used for:

- Formation visualization
- Player positioning
- Tactical reconstruction
- Match lineup rendering

The table is referenced by both `players`, `position_roles`, and `lineup_players`.

---

## `position_roles`

### Fields

```text
id (PK)
position_code (FK)
role_name
role_short
is_premium
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

The combination of `position_code` and `role_name` is unique.

This allows the system to distinguish between multiple tactical interpretations of the same base position.

---

## `formations`

### Fields

```text
id (PK)
name (UNIQUE)
description
is_popular
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

Formations are referenced by the `lineups` table.

---

# 2. Core Entities Group

These tables manage the primary football entities within the system.

---

## `teams`

### Fields

```text
team_id (PK)
name
logo_url
coach_name
coach_nationality
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
player_id (PK)
team_id (FK)
name
shirt_number
date_of_birth
nationality
contract_until
market_value
status
created_at
last_updated
preferred_position_code (FK)
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

The `team_id` establishes the player's current team association.

The `preferred_position_code` references the player's preferred tactical position.

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

---

# 3. Match & Lineup Group

These tables manage operational football data, including matches, tactical lineups, player participation, and match events.

---

## `matches`

### Fields

```text
match_id (PK)
team_id (FK)
opponent_team_id
opponent_name
competition_id (FK)
competition_name
match_date
home_or_away
score_home
score_away
status
last_updated
```

### Relationships

Linked to:

- `teams` through `team_id`
- `competitions` through `competition_id`

### Description

Stores match schedules and historical match information for a tracked team.

Stores:

- Match schedules
- Historical results
- Opponent information
- Competition metadata
- Match status

### Opponent Design

`opponent_team_id` is currently stored as a plain `bigint` without a foreign key constraint.

This is intentional because the system may synchronize matches where the opponent has not yet been imported into the `teams` table.

Therefore:

```text
matches
    │
    ├── team_id ──────────► teams.team_id
    │
    └── opponent_team_id ─► External / optional team reference
```

`opponent_name` is retained as a denormalized field so match data remains usable even when the opponent is not present in the local `teams` table.

---

## `lineups`

### Fields

```text
id (PK)
type
created_at
updated_at
formation_id (FK)
match_id (FK, UNIQUE)
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

Represents the official tactical lineup associated with a specific match.

Each match owns at most one lineup record because:

```text
UNIQUE (match_id)
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
id (PK)
player_id (FK)
position_code (FK)
shirt_number
is_starter
minute_in
minute_out
custom_x
custom_y
lineup_id (FK)
role_id (FK)
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
id (PK)
match_id (FK)
fotmob_event_id (UNIQUE)
event_type
event_order
minute
stoppage_time
team_id (FK, NULLABLE)
player_id (FK, NULLABLE)
assist_player_id (FK, NULLABLE)
description_key
raw_payload
created_at
last_updated
```

### Description

Stores detailed events occurring during a football match.

The table is designed to preserve both normalized event information and the original Fotmob payload.

Supported event categories are determined by the application Mapper rather than a database enum or CHECK constraint.

Examples:

```text
goal
card_yellow
card_red
own_goal
added_time
```

Additional event types can therefore be introduced without requiring a database migration.

---

## Event Identification

Each Fotmob event is identified using:

```text
fotmob_event_id (UNIQUE)
```

This allows the synchronization pipeline to identify existing events and avoid duplicate records.

The `event_order` field preserves the ordering of events within a match.

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

They are intentionally nullable.

This is particularly important for events such as:

```text
added_time
```

where there may be no specific player associated with the event.

`team_id` uses:

```text
ON DELETE SET NULL
```

because an event may reference an opponent that has not been imported into the local `teams` table.

---

## Raw Event Preservation

The original Fotmob event payload is stored in:

```text
raw_payload jsonb
```

This provides a raw-data preservation layer for the ETL pipeline.

It allows the system to:

- Preserve source information
- Debug Mapper behavior
- Reprocess events
- Add new normalized fields later
- Compare normalized data with the original Fotmob response

This is especially useful because the `event_type` is interpreted and validated at the Mapper layer rather than being strictly constrained by PostgreSQL.

---

## Match Event Performance

A composite index is created for timeline-oriented queries:

```sql
CREATE INDEX idx_match_events_match_event
ON public.match_events (match_id, event_type);
```

This optimizes queries that retrieve events for a match while filtering or grouping by event type.

Typical use cases include:

```text
Match Timeline
    │
    ├── Goals
    ├── Yellow Cards
    ├── Red Cards
    ├── Own Goals
    └── Added Time
```

---

# 5. Entity Relationships

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

Deleting a team cascades to its associated matches.

---

## Match → Lineup

```text
One Match
    │
    └──────────► One Lineup
```

Each match owns at most one tactical lineup.

This is enforced through:

```text
UNIQUE (match_id)
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
 ├── Substitution-related Event
 ├── Own Goal
 └── Added Time
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

The database uses PostgreSQL foreign keys to maintain relational consistency.

## ON DELETE CASCADE

Deleting a team automatically removes dependent match records:

```text
Team
 │
 └──► Matches
        │
        ├──► Lineups
        │      │
        │      └──► LineupPlayers
        │
        └──► MatchEvents
```

Deleting a player automatically removes dependent:

```text
LineupPlayers
MatchEvents
```

This prevents orphaned player participation and event records.

---

## ON DELETE SET NULL

Some relationships intentionally preserve the dependent record while removing the reference.

### Match → Competition

```text
matches.competition_id
        │
        └── ON DELETE SET NULL
```

If a competition is deleted, the match remains available but its competition reference becomes `NULL`.

### MatchEvent → Team

```text
match_events.team_id
        │
        └── ON DELETE SET NULL
```

This is particularly important when an event references an opponent that may not exist in the local `teams` table.

### MatchEvent → Assist Player

```text
match_events.assist_player_id
        │
        └── ON DELETE SET NULL
```

The event remains available even if the assisting player's record is removed.

### Lineup → Formation

```text
lineups.formation_id
        │
        └── ON DELETE SET NULL
```

The lineup remains available even if the referenced formation is removed.

### Players → Preferred Position

```text
players.preferred_position_code
        │
        └── ON DELETE SET NULL
```

The player record remains available even if the preferred position is removed.

### LineupPlayers → Role

```text
lineup_players.role_id
        │
        └── ON DELETE SET NULL
```

The lineup participation record remains available even if the tactical role is removed.

---

## ON DELETE RESTRICT

Position definitions are protected from deletion while they are referenced.

The following relationships use `RESTRICT`:

```text
position_roles.position_code
lineup_players.position_code
players.preferred_position_code
```

The purpose is to prevent the removal of a position that is still required by tactical data.

---

# 7. Database Automation

The database uses PostgreSQL automation features for timestamp maintenance.

## Players

The following field is automatically updated:

```text
players.last_updated
```

The trigger:

```text
trigger_players_last_updated
```

executes:

```text
update_players_last_updated()
```

before every update to a player.

Application services therefore do not need to manually maintain this field.

---

## Lineups

The following field is automatically updated:

```text
lineups.updated_at
```

The trigger:

```text
trigger_lineups_updated_at
```

executes:

```text
update_lineups_updated_at()
```

before every update to a lineup.

---

## Other Timestamp Fields

The schema also contains:

```text
competitions.last_updated
matches.last_updated
match_events.last_updated
created_at
```

These currently use PostgreSQL defaults where defined, but do not have the same automatic update triggers as `players.last_updated` and `lineups.updated_at`.

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
raw_payload jsonb
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

The database is designed to work with the FootballSystem synchronization pipeline.

Current data flow:

```text
Fotmob API
    │
    ▼
Extract
    │
    ▼
Raw Payload
    │
    ▼
Mapper
    │
    ├──► Player Data
    ├──► Match Data
    ├──► Lineup Data
    └──► Match Event Data
            │
            ▼
       Clean Models
            │
            ▼
   Supabase PostgreSQL
            │
            ▼
       Football API
            │
            ├──► Frontend
            │
            └──► AI Services
```

For match events, the pipeline specifically follows:

```text
Fotmob Match Events
        │
        ▼
Parse / Extract
        │
        ▼
Event Mapper
        │
        ├── event_type
        ├── minute
        ├── stoppage_time
        ├── team_id
        ├── player_id
        └── assist_player_id
        │
        ▼
match_events
        │
        └── raw_payload
```

The Mapper is responsible for interpreting and validating `event_type`.

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

The FootballSystem database emphasizes:

```text
Strong Relational Integrity
          ↓
Tactical Football Modeling
          ↓
Match Event Modeling
          ↓
Automated ETL Synchronization
          ↓
PostgreSQL Automation
          ↓
Scalable Architecture
          ↓
AI Extensibility
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
    └── Players
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
          ├──► Frontend
          │
          └──► AI Services
```

The current schema prioritizes:

```text
Consistency
     ↓
Tactical Accuracy
     ↓
Match Context
     ↓
Event Traceability
     ↓
Automation
     ↓
AI Readiness
```

The introduction of `match_events` is an important extension of the original architecture because the system now models not only **who played and where they played**, but also **what happened during the match and when it happened**.

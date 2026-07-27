# FootballSystem Database Architecture

Based on the current system architecture and SQL schema design, the `FootballSystem` database is built using:

- PostgreSQL
- Supabase hosting
- Strong relational modeling
- Tactical football domain structures
- AI-ready extensibility

The schema is divided into three primary logical groups:

1. Master Data
2. Core Entities
3. Match & Lineup Data

---

# 1. Master Data Group

These tables store foundational reference data shared across the entire system.

| Table | Fields | Description |
|-------|--------|-------------|
| `competitions` | `competition_id` (PK), `name`, `code`, `last_updated` | Stores tournament metadata such as Premier League or V-League. |
| `positions` | `position_code` (PK), `position_name`, `x_coord`, `y_coord`, `is_goalkeeper` | Defines tactical pitch positions with visual coordinates used for formation visualization. |
| `position_roles` | `id` (PK), `position_code` (FK), `role_name`, `role_short`, `is_premium`, `is_default` | Defines specialized tactical roles associated with each position. One role can be marked as the default role for mapping. |
| `formations` | `id` (PK), `name`, `description`, `is_popular` | Stores tactical formations such as `4-3-3`, `4-2-3-1`, or `3-5-2`. |

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
preferred_position_code (FK)
injury_description
created_at
last_updated
```

### Relationships

- One Team → Many Players
- Linked to:
  - `teams`
  - `positions`

### AI-Critical Fields

The following fields are important for future football intelligence features:

```text
market_value
status
preferred_position_code
injury_description
```

These support:

- Player scouting
- Tactical recommendation
- Injury tracking
- Squad availability analysis
- AI-based player similarity

---

# 3. Match & Lineup Group

These tables manage operational football data.

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

- `teams`
- `competitions`

### Responsibilities

Stores:

- Match schedules
- Historical results
- Opponent information
- Competition metadata
- Match status

---

## `lineups`

### Fields

```text
id (PK)
match_id (FK, UNIQUE)
formation_id (FK)
type
created_at
updated_at
```

### Description

Represents the official tactical lineup used for a specific match.

Each match owns exactly one lineup record.

Links:

- Match
- Formation

Stores:

- Formation used
- Lineup type
- Metadata timestamps

---

## `lineup_players`

### Fields

```text
id (PK)
lineup_id (FK)
player_id (FK)
position_code (FK)
role_id (FK)
shirt_number
is_starter
minute_in
minute_out
custom_x
custom_y
```

### Description

The most granular tactical entity in the database.

Represents one player's participation within a lineup.

Maps:

- Player
- Tactical position
- Tactical role
- Playing time
- Pitch coordinates

### Tactical Responsibilities

Tracks:

- Starter/Substitute status
- Minute entered
- Minute substituted off
- Tactical role assignment
- Position mapping
- Custom tactical coordinates

---

# 4. Entity Relationships

## Team → Players

```text
One Team
    │
    └──────────► Many Players
```

---

## Competition → Matches

```text
One Competition
    │
    └──────────► Many Matches
```

---

## Match → Lineup

```text
One Match
    │
    └──────────► One Lineup
```

Each match owns exactly one tactical lineup.

---

## Lineup → LineupPlayers

```text
One Lineup
    │
    └──────────► Many Lineup Players
```

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

# 5. Referential Integrity

The database uses PostgreSQL foreign keys to maintain consistency.

## ON DELETE CASCADE

Deleting a:

- Team
- Player

automatically removes dependent records from:

- Players
- Matches
- LineupPlayers

preventing orphaned tactical data.

---

## ON DELETE RESTRICT

Deleting a:

- Position

is prevented if it is referenced by:

- Players
- PositionRoles
- LineupPlayers

ensuring tactical consistency.

---

# 6. Database Automation

The database uses PostgreSQL automation features.

## Timestamp Automation

The following fields are automatically maintained:

```text
last_updated
updated_at
```

Typical trigger examples:

```sql
update_players_last_updated
update_lineups_updated_at
```

Application services should not manually update these timestamps.

---

# 7. Tactical Design Philosophy

The schema is designed around tactical football modeling rather than simple sports statistics.

The combination of:

- Positions
- PositionRoles
- Formations
- Lineups
- LineupPlayers

enables:

- Formation visualization
- Tactical reconstruction
- Role-based analysis
- Match replay
- Player positioning
- AI tactical reasoning

---

# 8. AI-Ready Extensions

The current schema is designed to support future AI modules.

Potential future additions include:

```sql
performance_vector VECTOR
```

for storing:

- Performance embeddings
- Tactical embeddings
- Player similarity vectors

Potential use cases:

- AI scouting
- Semantic player search
- Similar player recommendation
- Tactical clustering

---

# 9. Architectural Summary

The FootballSystem database emphasizes:

- Strong relational integrity
- Tactical football modeling
- Automated ETL synchronization
- PostgreSQL automation
- Scalable architecture
- AI extensibility

Current data flow:

```text
Fotmob API
      │
      ▼
Extract
      │
      ▼
Mapper
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
      ▼
Frontend / AI Services
```

The schema prioritizes:

```text
Consistency
      ↓
Tactical Accuracy
      ↓
Automation
      ↓
AI Readiness
```
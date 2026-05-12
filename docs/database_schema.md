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
|---|---|---|
| `competitions` | `competition_id` (PK), `name`, `code`, `last_updated` | Stores tournament metadata such as Premier League or V-League. |
| `positions` | `position_code` (PK), `position_name`, `x_coord`, `y_coord`, `is_goalkeeper` | Defines tactical pitch positions with visual coordinates. |
| `position_roles` | `id` (PK), `position_code` (FK), `role_name`, `role_short`, `is_premium` | Defines specialized tactical roles linked to positions. |
| `formations` | `id` (PK), `name`, `description`, `is_popular` | Stores tactical formations such as `4-3-3` or `3-5-2`. |

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

The central entity for football club management.

Stores:
- Club metadata
- Visual identity
- Coaching information

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
created_at
last_updated
```

### Relationships

- One Team → Many Players
- Linked to:
  - `teams`
  - `positions`

### AI-Critical Fields

The following fields are mandatory for future AI systems:

```text
market_value
status
```

These support:
- Scouting systems
- Performance analysis
- Recommendation engines
- Tactical intelligence

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
- Match history
- Scores
- Opponent information
- Match states

---

## `lineups`

### Fields

```text
id (PK)
match_id (FK)
formation_id (FK)
type
created_at
updated_at
```

### Description

Represents a tactical lineup used during a specific match.

Links:
- Matches
- Tactical formations

Examples:
- Starting XI
- Final lineup
- Tactical variation

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

The most granular tactical table in the entire system.

Maps:
- Players
- Tactical positions
- Tactical roles
- Match participation

inside a specific lineup.

### Tactical Responsibilities

Tracks:
- Starter/substitute status
- Tactical role assignment
- Dynamic positioning
- Match minute participation
- Tactical coordinate overrides

---

# 4. Key Relationships & Integrity Rules

The schema heavily utilizes PostgreSQL relational constraints and automation features.

---

# One-to-Many Relationships (1:N)

## Team → Players

```text
One Team
    →
Many Players
```

---

## Competition → Matches

```text
One Competition
    →
Many Matches
```

---

## Match → Lineups

```text
One Match
    →
Multiple Lineups
```

Examples:
- Starting lineup
- In-game tactical variation
- Final formation

---

## Position → PositionRoles

```text
One Position
    →
Many Tactical Roles
```

Example:

```text
Midfielder
    →
Box-to-Box
    →
Deep-Lying Playmaker
    →
Mezzala
```

---

# Referential Integrity Rules

## ON DELETE CASCADE

Deleting a:
- Team
- Player

automatically removes dependent records from:

- `matches`
- `players`
- `lineup_players`

This prevents orphaned tactical data.

---

## ON DELETE RESTRICT

Deleting a:
- Position

is blocked if it is currently referenced by:
- Players
- Position roles
- Tactical lineups

This preserves tactical consistency.

---

# Database Automation

The database uses:
- PL/pgSQL functions
- PostgreSQL triggers

to automatically maintain timestamps.

---

## Automated Timestamp Handling

The following fields are automatically maintained:

```text
last_updated
updated_at
```

Example trigger responsibilities:

```sql
update_players_last_updated
update_lineups_updated_at
```

C# services must never manually update these values.

---

# 5. Tactical Data Design Philosophy

The database structure is intentionally designed around:

- Tactical flexibility
- Match reconstruction
- Formation visualization
- AI-driven analysis
- Football intelligence systems

The combination of:

- `positions`
- `position_roles`
- `lineup_players`

creates a highly granular tactical model capable of representing:
- Real formations
- Dynamic player movement
- Role-based football analysis

---

# 6. AI-Ready Extensions

The schema is prepared for future AI integrations and football intelligence tooling.

---

## Vector Search Support

A future column is proposed for the `players` table:

```sql
performance_vector VECTOR
```

### Purpose

Stores:
- Performance embeddings
- Tactical embeddings
- Player similarity vectors

for:
- AI scouting
- Semantic search
- Recommendation systems

---

## RAG (Retrieval-Augmented Generation)

The tactical granularity of:

- `lineup_players`
- `position_roles`

enables advanced AI query systems.

### Example AI Queries

```text
Which Box-to-Box midfielders perform best in a 4-3-3?

Which tactical roles produce the highest pressing efficiency?

Which players perform similarly to a Trequartista in transition systems?
```

---

# 7. Architectural Summary

The FootballSystem database is designed around:

- Strong relational integrity
- Tactical football modeling
- Automated ETL synchronization
- PostgreSQL automation
- AI extensibility
- Scalable football intelligence workflows

The schema prioritizes:

```text
Consistency
→ Tactical Accuracy
→ Automation
→ AI Readiness
```

instead of simplistic sports data storage.
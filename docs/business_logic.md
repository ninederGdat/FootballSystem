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

---

# 2. Key Business Workflows

The project focuses on automating the football data lifecycle through several critical workflows.

---

## End-to-End ETL (Extract - Transform - Load)

### Extract
Fetches raw data from:
- Fotmob API
- Web pages

### Transform
Uses specialized **Mappers** to convert raw JSON data into clean domain models that strictly follow the PostgreSQL schema.

### Load
Executes **Upsert operations** to reliably populate the Supabase database.

---

## Advanced Player Scraping (Cloudflare Bypass)

### Playwright-Based Scraping
Uses **Playwright (headless browser)** to access player detail pages and bypass Cloudflare Turnstile protection.

### Stable Data Extraction
Extracts structured data directly from the `__NEXT_DATA__` script tag to ensure stability when standard APIs are unavailable or restricted.

---

## Automated Synchronization (Daily Sync)

### Quartz.NET Background Jobs
Integrates **Quartz.NET** to manage scheduled synchronization tasks for daily data updates.

### Dependency Synchronization
Automatically synchronizes related entities such as:
- `Position`
- `PositionRole`

during player updates to maintain referential integrity.

---

## Automated Data Integrity Maintenance

### Database Triggers & Functions
Uses:
- `PL/pgSQL Functions`
- `Triggers`

to automatically refresh `last_updated` timestamps whenever data changes.

### Cascade Cleanup Rules
Implements `ON DELETE CASCADE` constraints to automatically remove dependent:
- Match records
- Lineup records

when a primary entity such as a `Team` or `Player` is deleted.

---

## Data Delivery Interface

### FootballApi
The `FootballApi` project acts as a **Minimal API layer** that:
- Provides consistent and cleaned data to the frontend
- Shares the same database with the worker service
- Serves as the centralized data access layer

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
- Minimal APIs

to build a scalable and maintainable football intelligence platform.
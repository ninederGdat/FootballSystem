# FootballSystem

A comprehensive football data management ecosystem designed to automate the collection, transformation, and storage of professional football data (Teams, Players, Matches, and Tactics). The system leverages an automated ETL pipeline to keep a localized database synchronized with real-time sports data for advanced analysis and scouting.

---

# 🎯 Project Objectives

- **Automated Data Acquisition**  
  Scrape and synchronize data from external providers (specifically Fotmob) including:
  - Team rosters
  - Player details
  - Match schedules

- **Tactical Management**  
  Support complex football operations such as:
  - Tactical formations (`4-3-3`, `3-5-2`)
  - Player tactical roles (`Trequartista`, `Box-to-Box`, etc.)

- **Future AI Readiness**
  Player market value, player status, transfer history, lineups, and match events provide source data for potential future analysis.

- **Data Integrity**  
  Ensure consistency across the platform using:
  - PostgreSQL persistence through Supabase (deployed constraints must be verified separately)
  - Automated synchronization workflows

---

# 🛠️ Tech Stack

- **Backend**: `.NET 10 (C#)`

- **Primary Modules**
  - **Worker Service**: `FotmobSync` for ETL processing and background synchronization tasks
  - **ASP.NET Core controller API**: `FootballApi` for frontend data delivery

- **Database**
  - PostgreSQL
  - Hosted via **Supabase**

- **Data Collection**
  - `HttpClient` for standard API endpoints
  - `Playwright` for player-page extraction from `__NEXT_DATA__`

- **Scheduling**
  - `Quartz.NET` for recurring synchronization jobs

---

# 🏗️ Core Module Structure

The project follows a **Modular Monolith** architecture for simplicity, maintainability, and scalability.

---

## 1. FotmobSync (ETL Worker)

The core synchronization engine responsible for the football data lifecycle.

### Responsibilities

- Data extraction
- Data transformation
- Database synchronization
- Scheduled automation

### Folder Structure

#### `Clients/`

Contains:

- Third-party API integrations
- Browser scraping logic

Example:

- `FotmobClient.cs`; the player browser extractor is under `Infrastructure/External/`

#### `Mappers/`

Handles transformation logic that converts:

- **Raw Models**
  → into
- **Clean Models**

that map the FotMob payloads to Supabase table/upsert models. Deployed database constraints are not defined in this repository.

#### `Jobs/`

Contains:

- Quartz.NET scheduled jobs
- Daily synchronization workflows
- Background processing tasks

#### `Infrastructure/`

Responsible for:

- Supabase connection management
- External service configuration
- Shared infrastructure components

---

## 2. FootballApi

The interface layer that exposes cleaned football data to frontend clients.

### Features

- Controller-based read endpoints for matches, players, lineups, competitions, seasons, and transfers
- Shared Supabase database access
- Consistent data delivery layer

### Main Responsibilities

- Player retrieval
- Match data delivery
- Competition, season, and transfer queries
- Tactical data exposure

---

# 📊 Database Schema Highlights

The system organizes data into three primary domains.

---

## Master Data

Includes:

- Competitions
- Formations
- Positions
- PositionRoles

---

## Core Entities

### Teams

Stores:

- Team metadata
- Coaching information
- Branding assets

### Players

Stores:

- Market value
- Player status
- Tactical metadata
- AI-critical analysis fields
- Injury description and transfer status

---

## Match & Lineup Domain

### Matches

Stores:

- Match schedules
- Results
- Opponent information

### Match Events

Stores mapped goals and cards with their source event payloads. Other event types are currently skipped by the sync mapper.

### Lineups

Stores:

- Tactical formations
- Match-day squad selections

### `lineup_players`

Maps:

- Players
- Tactical roles
- Match positions
- Playing time

during a specific match.

### Transfers

Stores transfer history, loan periods, fees, and market values; the sync workflow also resolves player transfer status from the latest transfer record.

---

# ⚙️ Environment Setup

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Supabase Account](https://supabase.com/) with a PostgreSQL instance

---

## Step 1 — Clone the Repository

```bash
git clone <repository-url>
cd FootballSystem
```

---

## Step 2 — Configure Environment Variables

Create an environment configuration file or configure OS-level environment variables.

### Required Variables

```env
Supabase__Url=your_supabase_url
Supabase__AnonKey=your_supabase_anon_key
Supabase__ServiceRoleKey=your_supabase_service_role_key
Fotmob__XMasToken=your_fotmob_token
```

---

## Step 3 — Initialize Playwright

Since the player-detail extractor uses Playwright, install the required browser binaries from the worker project directory.

```bash
cd FotmobSync
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install
```

---

## Step 4 — Database Setup

The repository currently contains no SQL schema scripts, migrations, triggers, or database functions. Confirm the deployed Supabase schema and its constraints separately before running the sync or API.

---

## Step 5 — Run the Project

### Start the Synchronization Worker

```bash
dotnet run --project FotmobSync/FotmobSync.csproj
```

### Start the Web API

```bash
dotnet run --project FootballApi/FootballApi.csproj
```

---

# 🔄 Data Flow

The platform executes a strict ETL pipeline.

---

## 1. Extract

`FotmobClient` retrieves team and match-detail JSON from FotMob APIs. `FotmobBrowserClient` loads the player page and extracts `__NEXT_DATA__` from its HTML.

---

## 2. Transform

`Mappers` normalize supported raw fields into the clean/upsert models used by sync services.

---

## 3. Load

Clean/upsert models are written to Supabase with the conflict targets used by the sync services. Database constraints and trigger behavior are not defined in the repository and must be verified against the deployed schema.

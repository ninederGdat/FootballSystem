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

- **AI-Ready Infrastructure**  
  Maintain high-quality historical data including:
  - Market value
  - Player status
  - Performance metrics

  to support future AI-driven scouting and performance analysis.

- **Data Integrity**  
  Ensure consistency across the platform using:
  - PostgreSQL constraints
  - Database triggers
  - Automated synchronization workflows

---

# 🛠️ Tech Stack

- **Backend**: `.NET 10 (C#)`

- **Primary Modules**
  - **Worker Service**: `FotmobSync` for ETL processing and background synchronization tasks
  - **Minimal API**: `FootballApi` for frontend data delivery

- **Database**
  - PostgreSQL
  - Hosted via **Supabase**

- **Data Collection**
  - `HttpClient` for standard API endpoints
  - `Playwright` for browser automation and Cloudflare Turnstile bypass

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
- `FotmobBrowserClient.cs`

#### `Mappers/`
Handles transformation logic that converts:
- **Raw Models**
→ into
- **Clean Models**

that strictly follow the PostgreSQL schema.

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

- Lightweight Minimal API endpoints
- Shared Supabase database access
- Consistent data delivery layer

### Main Responsibilities

- Team retrieval
- Player retrieval
- Match data delivery
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

---

## Match & Lineup Domain

### Matches
Stores:
- Match schedules
- Results
- Opponent information

### Lineups
Stores:
- Tactical formations
- Match-day squad selections

### Lineup_players
Maps:
- Players
- Tactical roles
- Match positions
- Playing time

during a specific match.

---

# ⚙️ Environment Setup

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Supabase Account](https://supabase.com/) with a PostgreSQL instance
- [Node.js](https://nodejs.org/) (required for Playwright installation)

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
SUPABASE_URL=your_supabase_url
SUPABASE_KEY=your_supabase_key
```

---

## Step 3 — Initialize Playwright

Since the project uses Playwright for browser automation and scraper protection bypassing, install the required browser binaries.

```bash
cd src/FotmobSync
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install
```

---

## Step 4 — Database Setup

Run all SQL scripts located inside the `Schema/` folder using the Supabase SQL Editor.

This initializes:
- Tables
- Constraints
- Functions
- Triggers
- Cascade rules

---

## Step 5 — Run the Project

### Start the Synchronization Worker

```bash
dotnet run --project src/FotmobSync
```

### Start the Web API

```bash
dotnet run --project src/FootballApi
```

---

# 🔄 Data Flow

The platform executes a strict ETL pipeline.

---

## 1. Extract

`FotmobClient` or `FotmobBrowserClient` retrieves:
- Raw JSON
- Raw HTML
- Embedded page data

from Fotmob sources.

---

## 2. Transform

`Mappers`:
- Clean
- Normalize
- Validate

incoming data into database-ready models.

---

## 3. Load

Clean models are:
- Upserted into Supabase
- Validated by database constraints
- Automatically timestamped via triggers

to maintain referential integrity and synchronization consistency.
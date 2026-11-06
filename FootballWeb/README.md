# FootballWeb

React frontend for FootballSystem. It reads football data (matches, lineups, match events, players, transfers, competitions and seasons) from **FootballApi** and shows it for the tracked clubs (Chelsea and Strasbourg). The app is read-only: all data is written by the FotmobSync worker and served by FootballApi.

See the [root README](../README.md) for the whole system (FotmobSync, FootballApi, database, API endpoints).

## Tech stack

- React 18 and TypeScript
- Vite (dev server and build)
- React Router 6 (routing)
- TanStack Query 5 (data fetching and caching)
- ESLint

## Prerequisites

- Node.js 22 (the version used by CI) and npm
- A running FootballApi instance (see the root README for how to start it)

## Getting started

```bash
cd FootballWeb
npm ci
cp .env.example .env.local    # PowerShell: Copy-Item .env.example .env.local
npm run dev
```

Vite prints the local URL (by default `http://localhost:5173`).

### Environment variables

| Variable            | Description                                        | Example                 |
| ------------------- | -------------------------------------------------- | ----------------------- |
| `VITE_API_BASE_URL` | Base URL of FootballApi, without a trailing slash. | `http://localhost:5076` |

Set it in `.env.local`. Vite only exposes variables that start with `VITE_`, and they are read at build time, so restart the dev server after changing them.

## Scripts

| Command           | What it does                                    |
| ----------------- | ----------------------------------------------- |
| `npm run dev`     | Start the Vite dev server.                      |
| `npm run build`   | Type-check (`tsc -b`) and build for production. |
| `npm run preview` | Serve the production build locally.             |
| `npm run lint`    | Run ESLint.                                     |

CI runs `npm ci` and `npm run build` on every push to `master` and every pull request.

## Project structure

```text
src/
├── api/             Thin functions that call FootballApi (client.ts holds the shared fetch logic)
│                    competitions, matches, players, seasons, transfers
├── types/           TypeScript types for API responses
├── features/        TanStack Query hooks and feature-specific components
│   ├── competitions, matches, seasons, transfers   hooks.ts
│   ├── players/     hooks.ts plus filters, status/relation badges, loan lines, team chips
│   └── team-context/  TeamContext (selected club) and ClubSwitcher
├── components/      Reusable UI
│   ├── common/      Pagination, SectionCard, loading/empty/error states
│   ├── layout/      AppShell (page frame)
│   ├── match/       MatchCard, MatchHeader, MatchTimeline, Lineup
│   └── player/      PlayerCard, AppearanceList, ClubCell
├── routes/          One component per page
│                    Dashboard, MatchList, MatchDetail, PlayerList, PlayerDetail, Transfer
├── lib/             format.ts (formatting helpers) and teams.ts (tracked clubs)
├── App.tsx          Router and providers
└── main.tsx         Entry point
```

### How data flows

```text
routes/*Page  ->  features/*/hooks.ts (TanStack Query)  ->  api/*.ts  ->  api/client.ts  ->  FootballApi
```

Pages call hooks, hooks call the functions in `api/`, and `client.ts` sends the request to `VITE_API_BASE_URL`. Components under `components/` receive data through props and do not call the API themselves.

### Selected club

`features/team-context` stores which club is selected, and the ClubSwitcher in the layout changes it. The list of clubs lives in `lib/teams.ts` (`TRACKED_TEAMS`, `DEFAULT_TEAM_ID`). It is currently hardcoded; `GET /api/teams` already returns the same fields, so the list can later be loaded from the API.

## Adding a page

1. Add the response type in `src/types/`.
2. Add the request function in `src/api/`.
3. Add a query hook in `src/features/<name>/hooks.ts`.
4. Create the page in `src/routes/` and register its route in `src/App.tsx`.

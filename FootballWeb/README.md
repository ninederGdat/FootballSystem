# FootballSystem Frontend (V1)

React + TypeScript + Vite, TanStack Query for data fetching, React Router,
Tailwind CSS. Brand color `#001489` is wired into `tailwind.config.js` as
the `brand` color scale.

## Setup

```bash
npm install
cp .env.example .env.local   # set VITE_API_BASE_URL to your actual FootballApi port
npm run dev
```

Also apply `backend-changes/Program.cs` (or just the CORS block in it) to
`FootballApi/Program.cs` so the browser is allowed to call the API from
`http://localhost:5173`. If your API's real dev URL differs from
`http://localhost:5000`, update `.env.local` accordingly (check
`FootballApi/Properties/launchSettings.json`).

## What's implemented (Milestone 1)

- App shell (sidebar nav: Matches / Players), loading/error/empty states.
- **Match Detail**: header, formation-based lineup (starters + substitutes),
  timeline (goal/card/etc. with graceful fallback for unknown event types,
  correct `90+4'` stoppage-time formatting, and null-safe rendering for
  untracked opponent players/teams).
- **Player List**: search/filter by name, position code, nationality
  (paginated).
- **Player Detail**: profile fields + paginated appearance history table
  (`minuteOut = null` is rendered as "—", never coerced to 90).

## Known gap: Match List

`MatchesController` currently only exposes `GET /api/matches/{matchId}` —
there's no list endpoint. `MatchListPage` explains this and lets you open
a match by ID so Match Detail can be tested now. See the comment at the
top of `src/routes/MatchListPage.tsx` for the suggested backend addition
(`GET /api/matches?teamId=8455&page=&pageSize=`).

## Verify against real DTOs

`MatchEventItemResponse` wasn't provided, so `src/types/match.ts` infers
its shape from the brief's example JSON. If the real file differs, update
that one type — nothing else should need to change.

## Next milestones

1. Add the match list endpoint (backend) + wire up `MatchListPage`.
2. Confirm `MatchEventItemResponse` shape against source.
3. Polish formation layout once real `customX`/`customY` data is verified.
4. Add a `/api/teams` endpoint if you want a team dropdown instead of a
   free-text `teamId` filter on Player search.

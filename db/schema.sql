-- FootballSystem database schema (PostgreSQL / Supabase, schema "public")
--
-- Snapshot taken from the Supabase schema dump on 2026-11-05, reordered so
-- tables are created after the tables they reference.
--
-- NOTES
--  * The Supabase table dump lists constraints only, NOT standalone indexes.
--    Indexes below (section 5) come from pg_indexes on the live database,
--    also captured on 2026-11-05.
--  * All foreign keys use the default ON DELETE NO ACTION (none declare a rule).
--  * transfers_backup_20261102 is a one-off backup table and is intentionally
--    NOT part of this schema.

-- =========================================================
-- 1. Master data
-- =========================================================

CREATE TABLE IF NOT EXISTS public.competitions (
  competition_id bigint NOT NULL,
  name           text   NOT NULL,
  code           text   NOT NULL UNIQUE,
  last_updated   timestamptz DEFAULT now(),
  CONSTRAINT competitions_pkey PRIMARY KEY (competition_id)
);

CREATE TABLE IF NOT EXISTS public.positions (
  position_code text NOT NULL,
  position_name text NOT NULL,
  x_coord       double precision NOT NULL,
  y_coord       double precision NOT NULL,
  is_goalkeeper boolean DEFAULT false,
  CONSTRAINT positions_pkey PRIMARY KEY (position_code)
);

CREATE TABLE IF NOT EXISTS public.position_roles (
  id            bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
  position_code text,
  role_name     text NOT NULL,
  role_short    text,
  is_premium    boolean DEFAULT false,
  is_default    boolean NOT NULL DEFAULT false,
  CONSTRAINT position_roles_pkey PRIMARY KEY (id),
  CONSTRAINT position_roles_position_code_fkey
    FOREIGN KEY (position_code) REFERENCES public.positions (position_code)
);

CREATE TABLE IF NOT EXISTS public.formations (
  id          bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
  name        text NOT NULL UNIQUE,
  description text,
  is_popular  boolean DEFAULT true,
  CONSTRAINT formations_pkey PRIMARY KEY (id)
);

-- =========================================================
-- 2. Core entities
-- =========================================================

CREATE TABLE IF NOT EXISTS public.teams (
  team_id           bigint NOT NULL,
  name              text   NOT NULL,
  logo_url          text,
  coach_name        text,
  coach_nationality text,
  CONSTRAINT teams_pkey PRIMARY KEY (team_id)
);

CREATE TABLE IF NOT EXISTS public.players (
  player_id               bigint NOT NULL,
  team_id                 bigint,
  name                    text   NOT NULL,
  shirt_number            integer,
  date_of_birth           date,
  nationality             text,
  contract_until          date,
  market_value            numeric,
  status                  text DEFAULT 'Healthy',
  created_at              timestamptz DEFAULT now(),
  last_updated            timestamptz DEFAULT now(),
  preferred_position_code text,
  injury_description      text,
  transfer_status         text NOT NULL DEFAULT 'current',
  is_stub                 boolean NOT NULL DEFAULT false,
  current_team_id         bigint,   -- no FK declared
  CONSTRAINT players_pkey PRIMARY KEY (player_id),
  CONSTRAINT players_team_id_fkey
    FOREIGN KEY (team_id) REFERENCES public.teams (team_id),
  CONSTRAINT players_preferred_position_code_fkey
    FOREIGN KEY (preferred_position_code) REFERENCES public.positions (position_code)
);

-- No FKs on purpose in the current database: from/to clubs may be teams
-- that are not stored in public.teams.
CREATE TABLE IF NOT EXISTS public.transfers (
  id                       bigserial NOT NULL,
  player_id                bigint NOT NULL,
  player_name              text   NOT NULL,
  transfer_date            timestamptz NOT NULL,
  has_incomplete_timestamp boolean NOT NULL DEFAULT false,
  from_club_id             bigint NOT NULL,
  from_club_name           text   NOT NULL,
  to_club_id               bigint NOT NULL,
  to_club_name             text   NOT NULL,
  transfer_type            text   NOT NULL,
  on_loan                  boolean NOT NULL,
  fee_value                numeric,
  fee_text                 text,
  market_value             numeric,
  period_start             timestamptz,
  period_end               timestamptz,
  is_system_generated      boolean NOT NULL DEFAULT false,
  created_at               timestamptz NOT NULL DEFAULT now(),
  last_updated             timestamptz NOT NULL DEFAULT now(),
  contract_extension       boolean NOT NULL DEFAULT false,
  CONSTRAINT transfers_pkey PRIMARY KEY (id)
);

-- =========================================================
-- 3. Matches, lineups, events
-- =========================================================

CREATE TABLE IF NOT EXISTS public.matches (
  match_id         bigint NOT NULL,
  team_id          bigint,
  opponent_team_id bigint,   -- external/optional team reference, no FK
  opponent_name    text NOT NULL,
  competition_id   bigint,
  competition_name text,
  match_date       timestamptz NOT NULL,
  home_or_away     text CHECK (home_or_away = ANY (ARRAY['home', 'away'])),
  score_home       integer,
  score_away       integer,
  status           text NOT NULL DEFAULT 'UPCOMING',
  last_updated     timestamptz DEFAULT now(),
  penalty_home     integer,   -- shootout score; null = no shootout (score_home/away include it)
  penalty_away     integer,
  CONSTRAINT matches_pkey PRIMARY KEY (match_id),
  CONSTRAINT matches_team_id_fkey
    FOREIGN KEY (team_id) REFERENCES public.teams (team_id),
  CONSTRAINT matches_competition_id_fkey
    FOREIGN KEY (competition_id) REFERENCES public.competitions (competition_id)
  CONSTRAINT matches_penalty_check CHECK (
    (penalty_home IS NULL) = (penalty_away IS NULL)
    AND COALESCE(penalty_home, 0) >= 0
    AND COALESCE(penalty_away, 0) >= 0
  ),
);

CREATE TABLE IF NOT EXISTS public.lineups (
  id           bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
  type         text NOT NULL,
  created_at   timestamptz DEFAULT now(),
  updated_at   timestamptz DEFAULT now(),
  formation_id bigint,
  match_id     bigint,          -- one lineup per match (see uq_lineups_match)
  is_complete  boolean NOT NULL DEFAULT false,
  CONSTRAINT lineups_pkey PRIMARY KEY (id),
  CONSTRAINT lineups_formation_id_fkey
    FOREIGN KEY (formation_id) REFERENCES public.formations (id),
  CONSTRAINT lineups_match_id_fkey
    FOREIGN KEY (match_id) REFERENCES public.matches (match_id)
);

CREATE TABLE IF NOT EXISTS public.lineup_players (
  id            bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
  player_id     bigint,
  position_code text,
  shirt_number  integer,
  is_starter    boolean DEFAULT true,
  minute_in     integer,
  minute_out    integer,
  custom_x      double precision,
  custom_y      double precision,
  lineup_id     bigint,
  role_id       bigint,
  CONSTRAINT lineup_players_pkey PRIMARY KEY (id),
  CONSTRAINT lineup_players_player_id_fkey
    FOREIGN KEY (player_id) REFERENCES public.players (player_id),
  CONSTRAINT lineup_players_position_code_fkey
    FOREIGN KEY (position_code) REFERENCES public.positions (position_code),
  CONSTRAINT lineup_players_lineup_id_fkey
    FOREIGN KEY (lineup_id) REFERENCES public.lineups (id),
  CONSTRAINT lineup_players_role_id_fkey
    FOREIGN KEY (role_id) REFERENCES public.position_roles (id)
);

CREATE TABLE IF NOT EXISTS public.match_events (
  id               bigserial NOT NULL,
  match_id         bigint NOT NULL,
  fotmob_event_id  bigint NOT NULL UNIQUE,   -- upsert conflict target
  event_type       text   NOT NULL,          -- goal | own_goal | yellow | yellow_red | red (enforced by mapper only)
  event_order      integer NOT NULL,         -- index in the source events array
  minute           integer NOT NULL,
  stoppage_time    integer,
  team_id          bigint,
  player_id        bigint,
  assist_player_id bigint,
  description_key  text,
  raw_payload      jsonb NOT NULL,
  created_at       timestamptz NOT NULL DEFAULT now(),
  last_updated     timestamptz NOT NULL DEFAULT now(),
  CONSTRAINT match_events_pkey PRIMARY KEY (id),
  CONSTRAINT match_events_match_id_fkey
    FOREIGN KEY (match_id) REFERENCES public.matches (match_id),
  CONSTRAINT match_events_player_id_fkey
    FOREIGN KEY (player_id) REFERENCES public.players (player_id),
  CONSTRAINT match_events_assist_player_id_fkey
    FOREIGN KEY (assist_player_id) REFERENCES public.players (player_id),
  CONSTRAINT match_events_team_id_fkey
    FOREIGN KEY (team_id) REFERENCES public.teams (team_id)
);

-- =========================================================
-- 4. Sync bookkeeping
-- =========================================================

CREATE TABLE IF NOT EXISTS public.lineup_sync_attempts (
  match_id        bigint NOT NULL,
  attempts        integer NOT NULL DEFAULT 0,
  last_attempt_at timestamptz NOT NULL DEFAULT now(),
  CONSTRAINT lineup_sync_attempts_pkey PRIMARY KEY (match_id)
);

-- =========================================================
-- 5. Constraints expected by sync code -- VERIFY before enabling
-- =========================================================
-- The sync services upsert with these conflict targets. PostgREST/Supabase
-- needs a matching UNIQUE constraint or unique index for each, otherwise the
-- upsert fails. The dump above does not show them. Check pg_indexes on the
-- live database first; only uncomment what is genuinely missing, and check
-- for duplicate rows before creating a unique index.
--
-- CREATE UNIQUE INDEX IF NOT EXISTS lineup_players_lineup_player_uidx
--   ON public.lineup_players (lineup_id, player_id);
--
-- CREATE UNIQUE INDEX IF NOT EXISTS transfers_player_date_to_club_uidx
--   ON public.transfers (player_id, transfer_date, to_club_id);
# Fotmob Raw Data Structures

Documentation for raw JSON structures extracted from Fotmob used in the **FootballSystem** project.

---

## 1. Player Raw

**Source**: Player detail page (`__NEXT_DATA__` → `pageProps.data`)

**Purpose**: Contains detailed information about a player (or coach).

```json
{
  "id": 1096353,
  "name": "Cole Palmer",
  "birthDate": {
    "utcTime": "2002-05-06T00:00:00.000Z"
  },
  "contractEnd": {
    "utcTime": "2033-06-30T00:00:00.000Z"
  },
  "status": "active",
  "primaryTeam": {
    "teamId": 8455
  },
  "positionDescription": {
    "primaryPosition": {
      "label": "Attacking Midfielder",
      "key": "centerattackingmidfielder"
    },
    "nonPrimaryPositions": [
      { "label": "Right Winger", "key": "rightwinger" }
    ],
    "positions": [
      {
        "strPos": {
          "label": "Attacking Midfielder",
          "key": "centerattackingmidfielder"
        }
      }
    ]
  },
  "injuryInformation": null,
  "playerInformation": [
    {
      "title": "Shirt",
      "value": { "numberValue": 10 }
    },
    {
      "title": "Preferred foot",
      "value": { "key": "left" }
    },
    {
      "title": "Country",
      "value": { "fallback": "England" },
      "countryCode": "ENG"
    }
  ],
  "ccode": "ENG",
  "marketValues": {
    "values": [
      {
        "date": "2026-05-01T00:00:00+00:00",
        "value": 100951687,
        "currency": "EUR"
      }
    ]
  },
  "meta": {
    "personJSONLD": {
      "nationality": {
        "name": "England"
      }
    }
  }
}
```

---

## 2. Team Raw

**Source**: Team detail endpoint

**Purpose**: Team information and squad composition.

```json
{
  "details": {
    "id": 8455,
    "name": "Chelsea",
    "shortName": "Chelsea",
    "logoUrl": "https://...",
    "country": "England",
    "ccode": "ENG"
  },
  "squad": {
    "groups": [
      {
        "title": "Goalkeepers",
        "members": [
          {
            "id": 789571,
            "name": "Robert Sánchez",
            "countryCode": "ESP",
            "cname": "Spain"
          }
        ]
      },
      {
        "title": "Coach",
        "members": [
          {
            "id": 1823023,
            "name": "Calum McFarlane",
            "countryCode": "ENG"
          }
        ]
      }
    ]
  }
}
```

---

## 3. Match Raw

**Source**: Fixtures section in team data (`fixtures.allFixtures.fixtures[]`)

**Purpose**: Individual match information for scheduling and results.

```json
{
  "id": 5315746,
  "home": {
    "id": 8455,
    "name": "Chelsea",
    "score": 0
  },
  "away": {
    "id": 8456,
    "name": "Manchester City",
    "score": 0
  },
  "tournament": {
    "name": "FA Cup",
    "leagueId": 132
  },
  "status": {
    "utcTime": "2026-05-16T14:00:00.000Z",
    "started": false,
    "finished": false,
    "cancelled": false
  }
}
```

---

## 4. Match Lineup Raw

**Source**: FotMob Match Detail API → content.lineup

**Purpose**: Contains detailed lineup information for both teams in a match, including starting XI, substitutes, formation, player positions, and basic performance data.

```json
{
  "matchId": "4813719",
  "lineupType": "standard",
  "homeTeam": {
    "id": 8455,
    "name": "Chelsea",
    "rating": 6.5,
    "formation": "4-2-3-1",
    "starters": [
      {
        "id": 789571,
        "name": "Robert Sánchez",
        "shirtNumber": 1,
        "positionId": 11,
        "usualPlayingPositionId": 0,
        "countryName": "Spain",
        "countryCode": "ESP",
        "horizontalLayout": {
          "x": 0.1,
          "y": 0.5
        },
        "marketValue": 19688528,
        "performance": {
          "rating": 5.8,
          "substitutionEvents": [
            {
              "time": 66,
              "type": "subOut",
              "reason": "injury"
            }
          ],
          "fantasyScore": "1"
        }
      }
      // ... other 10 starting players
    ],
    "subs": [
      {
        "id": 1096400,
        "name": "Levi Colwill",
        "shirtNumber": 6,
        "usualPlayingPositionId": 1,
        "marketValue": 62075967,
        "performance": {
          "rating": 6.3,
          "substitutionEvents": [
            {
              "time": 46,
              "type": "subIn",
              "reason": "tactical"
            }
          ]
        }
      }
    ],
    "unavailable": [
      {
        "id": 976506,
        "name": "Mykhaylo Mudryk",
        "unavailability": {
          "type": "suspension",
          "expectedReturn": "2030-04-30T00:00:00"
        }
      }
    ],
    "coach": {
      "id": 1823023,
      "name": "Calum McFarlane"
    }
  },
  "awayTeam": {
    "teamId": 10203,
    "name": "Nottingham Forest",
    "rating": 7.1,
    "formation": "4-2-3-1",
    "starters": [ /* similar structure as homeTeam */ ],
    "subs": [ /* similar structure as homeTeam */ ],
    "unavailable": [],
    "coach": {
      "id": 282115,
      "name": "Vítor Pereira"
    }
  }
}
```

---

## 5. Lineup Player Raw (Recommended Flat Structure)

**Purpose**: Extracted from content.lineup.homeTeam.starters, subs, etc.
**Source**: Standardized raw player data within a lineup for easier mapping to database.

```json

{
  "playerId": 1021382,
  "name": "João Pedro",
  "shirtNumber": 20,
  "positionId": 115,
  "usualPlayingPositionId": 3,
  "countryCode": "BRA",
  "isStarter": true,
  "horizontalX": 0.87,
  "horizontalY": 0.5,
  "verticalX": 0.5,
  "verticalY": 0.87,
  "marketValue": 76047637,
  "performance": {
    "rating": 7.7,
    "events": [
      { "type": "goal" },
      { "type": "assist" }
    ],
    "fantasyScore": "6+2",
    "playerOfTheMatch": true
  },
  "substitution": {
    "minuteIn": null,
    "minuteOut": null,
    "reason": null
  }
}

```

---


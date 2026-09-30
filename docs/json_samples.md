# Fotmob Raw Data Structures

Documentation for raw JSON structures extracted from Fotmob used in the **FootballSystem** project.

---

## 1. Player Raw

**Source**: Player detail page. The browser client reads `props.pageProps.data` when its `id` matches; fallback is `props.pageProps.fallback["player:{playerId}"]`.

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
      {
        "label": "Right Winger",
        "key": "rightwinger"
      }
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
  "injuryInformation": {
    "key": "injury",
    "name": "Hamstring injury",
    "expectedReturn": {
      "utcTime": "2026-10-20T00:00:00Z"
    }
  },
  "playerInformation": [
    {
      "title": "Shirt",
      "value": {
        "numberValue": 10
      }
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

**Source**: `GET /api/data/teams?id={teamId}&ccode3=VNM`

**Purpose**: Team information and squad composition.

```json
{
  "details": {
    "id": 8455,
    "name": "Chelsea",
    "shortName": "Chelsea",
    "logo": "https://..."
  },
  "squad": {
    "squad": [
      {
        "title": "Goalkeepers",
        "members": [
          {
            "id": 789571,
            "name": "Robert Sánchez",
            "ccode": "ESP",
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
            "ccode": "ENG"
          }
        ]
      }
    ]
  },
  "transfers": {
    "allTransfers": []
  }
}
```

---

## 3. Transfer Raw

**Source**: Team detail endpoint → `transfers.allTransfers[]` (`TeamRaw.Transfers.AllTransfers`)

**Purpose**: Contains player transfer history associated with a team, including permanent transfers and loan movements.

The same transfer structure is used for both permanent transfers and loans. The main distinction is represented by:

- `transferType.text`
- `transferType.localizationKey`
- `onLoan`
- `fee.feeText`
- `fromDate`
- `toDate`

```json
[
  {
    "name": "Marc Cucurella",
    "playerId": 873289,
    "transferDate": "2026-06-15T09:22:32Z",
    "fromClub": "Chelsea",
    "fromClubFullName": "Chelsea",
    "fromClubId": 8455,
    "toClub": "Real Madrid",
    "toClubFullName": "Real Madrid",
    "toClubId": 8633,
    "fee": {
      "feeText": "fee",
      "localizedFeeText": "transfer_fee",
      "value": 55000000
    },
    "amountEuroEstimated": null,
    "transferType": {
      "text": "contract",
      "localizationKey": "contract"
    },
    "contractExtension": false,
    "onLoan": false,
    "fromDate": "2026-06-30T22:00:00Z",
    "toDate": "2032-06-29T22:00:00Z",
    "marketValue": 45916601
  },
  {
    "name": "Tyrique George",
    "playerId": 1424875,
    "transferDate": "2026-02-02T22:15:51Z",
    "fromClub": "Chelsea",
    "fromClubFullName": "Chelsea",
    "fromClubId": 8455,
    "toClub": "Everton",
    "toClubFullName": "Everton",
    "toClubId": 8668,
    "fee": {
      "feeText": "on loan",
      "localizedFeeText": "on_loan"
    },
    "amountEuroEstimated": null,
    "transferType": {
      "text": "on loan",
      "localizationKey": "on_loan"
    },
    "contractExtension": false,
    "onLoan": true,
    "fromDate": "2026-02-01T23:00:00Z",
    "toDate": "2026-06-29T22:00:00Z",
    "marketValue": 27346436
  }
]
```

---

## 4. Match Raw

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

## 5. Match Lineup Raw

**Source**: FotMob Match Detail API → `content.lineup`

**Purpose**: Contains detailed lineup information for both teams in a match, including starting XI, substitutes, formation, player positions, and basic performance data.

```json
{
  "matchId": 4813719,
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
        "shirtNumber": "1",
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
        "shirtNumber": "6",
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
    "id": 10203,
    "name": "Nottingham Forest",
    "rating": 7.1,
    "formation": "4-2-3-1",
    "starters": [
      /* similar structure as homeTeam */
    ],
    "subs": [
      /* similar structure as homeTeam */
    ],
    "unavailable": [],
    "coach": {
      "id": 282115,
      "name": "Vítor Pereira"
    }
  }
}
```

---

## 6. Lineup Player Raw (Recommended Flat Structure)

**Purpose**: Extracted from `content.lineup.homeTeam.starters`, `subs`, etc.

**Source**: Standardized raw player data within a lineup for easier mapping to database.

```json
{
  "id": 789571,
  "name": "Robert Sánchez",
  "shirtNumber": "1",
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
    "events": [
      {
        "type": "goal"
      }
    ],
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
```

---

## 7. Match Event Raw

**Source**: FotMob Match Detail API → `content.matchFacts.events.events[]`

**Purpose**: Raw goal/card fields consumed by `MatchEventRaw` and `MatchEventMapper`. The sync preserves the complete event object in `raw_payload`; the mapper currently persists only goals and cards.

```json
{
  "eventId": 918273645,
  "type": "Goal",
  "time": 90,
  "overloadTime": 4,
  "isHome": true,
  "player": {
    "id": 1096353,
    "name": "Cole Palmer"
  },
  "ownGoal": false,
  "goalDescriptionKey": "penalty",
  "isPenaltyShootoutEvent": false,
  "assistPlayerId": null
}
```

Card events use the same common fields with `type: "Card"`, `card` (`"Yellow"`, `"YellowRed"`, or `"Red"`), and optional `cardDescription`; the goal-specific fields may be absent.

---

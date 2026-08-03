# Player Engine Data Audit â€” CM26

Date: 2026-07-28
Database folder: `<repo>\database` (read-only audit; NO external TXT/XLSX/CSV)

## 1. Layer-by-layer trace (player 250, David Beckham)

Command: `CM26.EngineDiagnostics --trace-player-name 250`

| Layer | Value | Correct? |
|-------|-------|----------|
| Physical DB `players.playerid` | `250` | âœ… |
| Native engine `firstnameid` | `8642` | âœ… correct integer |
| Native engine `lastnameid` | `4000` | âœ… correct integer |
| Native engine `commonnameid` | `0` | âœ… |
| `playernames.name` for 8642 | bytes `[C4 C4 C4 44 C4]` | âš  **ciphered placeholder (0xC4 fill)** |
| `playernames.name` for 4000 | bytes `[20 C4 C4 64 61 69 C4]` | âš  **ciphered placeholder** |
| Bridge `GetCellBytes` | returns same bytes | âœ… |
| `PlayerNameService` decode | `null` (rejected 0xC4) | âœ… honest |
| `NameResolverService` display | `Player 250` | âœ… honest fallback |

**First incorrect layer: the physical `playernames.name` payload is an EA cipher placeholder, not
readable text.** The engine, bridge, and resolver all behave correctly; they cannot invent a name.

## 2. Why numeric player names appear

The names are **not** numeric IDs leaking through. The engine correctly returns the *nameid* integers
(8642, 4000); these are looked up in `playernames`, whose `name` column is ciphered `0xC4` fill.
Because no readable name can be decoded, the resolver returns the documented fallback `Player {id}`.

In an **earlier build** there was a UI bug: `TeamsSection.ShowRecord` did
`player.Name.Split(' ', 2)` on the fallback string `"Player 10264"`, placing `"10264"` in the Surname
column and `"Player"` in the First Name column. **That bug is fixed** â€” the roster now uses a single
`Display Name` column and never splits the fallback. `bareNumeric=0` confirms no bare numeric name is
shown.

## 3. Full pass â€” 20,268 players

| Metric | Value |
|--------|-------|
| Total player records | 20,268 |
| Real names resolved from loaded DB | **0** |
| Honest fallbacks (`Player {id}`) | 20,268 |
| Bare numeric display names | **0** |
| Invalid/blank names | 0 |
| Exceptions | 0 |
| Processing time | ~785 ms |

`playernames` table: 41,190 rows, **0 decodable**, 41,190 placeholder/ciphered. `editedplayernames`
and `dcplayernames` are empty.

## 4. 150-player sample (positions/UTF-8/free-agents)

Spot-traced players across positions (GK/DEF/MID/FWD), free agents, and UTF-8 names â€” all return the
honest `Player {id}` fallback with `bareNumeric=0`. The decoder accepts UTF-8/CP1252 (proven on the
cipher-free `manager` table, e.g. "RÃºben Filipe Marques Amorim") and correctly rejects `0xC4` cipher.

## 5. Verdict

| Layer | Verdict |
|-------|---------|
| Physical DB `playernames.name` | **ciphered** (EA placeholder) â€” the genuine blocker |
| Native engine | PASS (reads IDs and bytes correctly) |
| Bridge | PASS |
| Resolver/PlayerNameService | PASS (honest fallback, no fabrication) |
| UI | PASS (no `Split`, `bareNumeric=0`) |

**Remaining blocker:** EA's second-layer text cipher key, which is not present in any database file
(see `DATABASE_NATIVE_PLAYER_NAME_AUDIT.md`). No external export is used.

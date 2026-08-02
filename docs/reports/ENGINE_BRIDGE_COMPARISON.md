# Engine vs Bridge Comparison — CM26

Date: 2026-07-28
Method: `CM26.EngineDiagnostics --compare-bridge <table>` (read-only).

## 1. Method

For each representative record, the native engine value (via `DatabaseSession.GetRecord`) is compared
against the C++/CLI bridge value (`GetCellText` / `GetCellBytes`). The bridge is a thin adapter over
`cm26::DatabaseEngine` with no database-format logic of its own.

## 2. Findings

### Integers / signed / unsigned
- `nations.confederation`: engine returns `2` (Albania); bridge `GetCellText` returns `"2"`. ✅
- `nations.nationid`: engine returns `5`; bridge `"5"`. ✅
- `players.playerid`: engine `250`; bridge `"250"`. ✅
- `players.firstnameid/lastnameid/commonnameid`: engine `8642/4000/0`; bridge `"8642"/"4000"/"0"`. ✅
- No unsigned-to-negative conversion observed. IDs are positive and correct.

### Strings
- `nations.nationname`: engine `"Albania"`; bridge bytes `[41 6C 62 61 6E 69 61]` = "Albania". ✅
- `teams.teamname`, `leagues.leaguename`, `stadiums.name`: literal text matches. ✅

### Byte arrays (compressed)
- `playernames.name` (Beckham last, id 4000): engine bytes `[20 C4 C4 64 61 69 C4]`; bridge
  `GetCellBytes` returns identical bytes. ✅ (The content is ciphered, but the transfer is faithful.)

### UTF-8 / CP1252
- Bridge returns raw bytes; managed decoding is applied by `NameTextDecoder` (UTF-8 then CP1252).
- Manager names with accents (e.g. "Rúben … Amorim") decode correctly. ✅

### Null / empty fields
- `commonnameid=0`, empty locale slots, and zero-length strings are handled without exception. ✅

### Enum / foreign keys
- Foreign-key integers (e.g. `teams.leagueid`) pass through as correct integers; resolvers map them
  to names. ✅

## 3. Coverage

| Table | Records sampled | Result |
|-------|-----------------|--------|
| nations | 218 (all) | PASS |
| leagues | 20 | PASS |
| teams | 20 | PASS |
| players | 50 | PASS |
| referee | 20 | PASS |
| stadiums | 20 | PASS |

## 4. Bridge defect check

- No truncation, no decimal-string conversion of byte arrays, no field-index-as-value, no stale
  pointers observed.
- The bridge does **not** return raw localization IDs as names; it returns the raw `playernames.name`
  bytes, which the managed decoder then classifies as ciphered.

## 5. Verdict

**C++/CLI bridge: PASS.** It faithfully transfers native values to managed types. No bridge defect
contributes to either the confederation or player-name issue.

# Database Loading Root-Cause Report — CM26

Date: 2026-07-28

## 1. Exact database folder loaded

`D:\CM 26 Final\database`

## 2. Exact files opened

| File | Path | SHA-256 (prefix) |
|------|------|------------------|
| metadata | `D:\CM 26 Final\database\fifa_ng_db-meta.XML` | `38D5B4B5…` |
| main DB | `D:\CM 26 Final\database\fifa_ng_db.db` | `A5CF1D9D…` |
| locale | `D:\CM 26 Final\database\eng_us.DB` | `9E9396D3…` |

The app **does not** open `eng_us_decrypted.db`, `eng_us_decrypted2.db`, `sample db extracted\*`,
`FC26 Modern Database Studio\*`, TXT/XLSX exports, or any previous session's files. Verified by the
diagnostic harness `--show-loaded-files` and the `--name-tests` external-file guard (clean).

## 3. Table counts / row counts

- 279 main tables + 2 locale tables = 281 `DatabaseSession.Tables`.
- `players` = 20,268 rows; `nations` = 218; `teams` = 808; `playernames` = 41,190.
- Total rows: 360,298 (engine smoke).

## 4. Schema comparison result

PASS — field order, offsets, widths, signedness, and types match `fifa_ng_db-meta.XML`. See
`RAW_SCHEMA_COMPARISON.md`.

## 5. Country-confederation root cause

- Raw DB value for Afghanistan `confederation` = **5** (correct for AFC).
- **Earlier mapping** `5 → CAF` was wrong → caused the symptom.
- **Current mapping** `5 → AFC` is correct (13/13 ground-truth PASS).
- The user's report stems from **running a stale binary** (`build_updated\` 27/7, `publish\` 26/7,
  `Release\CM26\` 27/7). The rebuilt packages (28/7 00:58) are correct.

## 6. Player-name root cause

- `players.firstnameid/lastnameid` are correct integers (e.g. Beckham 8642/4000).
- `playernames.name` for those IDs is an EA-ciphered `0xC4` placeholder (bytes `[C4 C4 C4 44 C4]`).
- No readable name can be decoded from the database; the resolver honestly returns `Player {id}`.
- `bareNumeric = 0` — no numeric ID is shown as a surname. The earlier `Split(' ',2)` UI bug is fixed.

## 7. Layer verdicts

| Layer | Verdict |
|-------|---------|
| Physical DB | **PASS** (confederation correct; names ciphered by EA design) |
| Metadata | **PASS** |
| Native engine | **PASS** |
| C++/CLI bridge | **PASS** |
| Application services | **PASS** |
| Resolvers | **PASS** (confederation fixed; name fallback honest) |
| Cache/session | **PASS** |
| WinForms binding | **PASS** in current binary; stale binaries fail |

## 8. Is the native engine faulty?

**No.** The engine reads `nations.confederation`, `players.*nameid`, and `playernames.name` bytes
correctly. Verified by object-code comparison and full smoke test EXIT=0.

## 9. Is the bridge faulty?

**No.** Integers, strings, and byte arrays transfer faithfully (see `ENGINE_BRIDGE_COMPARISON.md`).

## 10. Are the services/resolvers faulty?

**No longer.** The confederation mapping was fixed earlier; the player-name resolver is honest. Cache
rebuild on database switch works.

## 11. Is the UI faulty?

**Not in the current binary.** The `Split(' ',2)` roster bug is fixed; `bareNumeric=0`. The only "fault"
is that **stale binaries** in other build folders still contain the old mapping.

## 12. Files created

- `tools\EngineDiagnostics\` (read-only harness)
- `ENGINE_DATA_INTEGRITY_AUDIT.md`
- `RAW_SCHEMA_COMPARISON.md`
- `COUNTRY_CONFEDERATION_AUDIT.md`
- `PLAYER_ENGINE_DATA_AUDIT.md`
- `ENGINE_BRIDGE_COMPARISON.md`
- this report

## 13. Files modified

None during this diagnosis (engine untouched). The confederation fix and player-name fallback were
applied in the prior task and are present in the current source/binaries.

## 14. Protected files

All unchanged: `database_engine.h` (887B7A35…), `database_engine.cpp` (92600FBE…),
`engine_smoke.cpp` (BFF66D9A…), `fifa_ng_db.db` (A5CF1D9D…), `eng_us.DB` (9E9396D3…).

## 15. Build result

`build-managed.cmd` → ALL BUILDS + ENGINE TEST PASSED (0 errors; smoke EXIT=0).

## 16. Tests executed / results

1. Loaded-file path — PASS
2. DB-file SHA-256 — PASS
3. Schema comparison — PASS
4. Country raw-row — PASS
5. Afghanistan confederation trace — PASS
6. All-country confederation — PASS (13/13)
7. Player raw-row — PASS
8. 150-player sample — PASS
9. Full-player pass — PASS (0 bare numeric)
10. Engine vs bridge — PASS
11. Cache-clear — PASS
12. Database-switch — PASS
13. Player selector — PASS
14. Player Info binding — PASS
15. Team Roster binding — PASS
16. Formation label — PASS
17. Captain/set-piece — PASS
18. Transfer label — PASS
19. Full Release build — PASS
20. Engine smoke — PASS
21. Managed tests — PASS
22. Scratch save round-trip — PASS
23. Protected-file SHA-256 — PASS
24. Original DB SHA-256 — PASS

## 17. Remaining risks

- Stale binaries in `build_updated\`, `publish\`, `publish_fdd\`, `Release\CM26\` mislead users.
- Player names remain `Player {id}` until EA's cipher key / a decoded source is available (by design).

## 18. Exact next action

1. **Remove/archive stale binaries** (`build_updated\`, `publish\`, `publish_fdd\`, `Release\CM26\`,
   and old `bin\` outputs) so only the 28-July rebuilt packages remain. This alone resolves the
   "Afghanistan in CAF" report.
2. For real player names: either (a) re-allow a local readable export, or (b) obtain EA's runtime
   cipher key. No fabrication.

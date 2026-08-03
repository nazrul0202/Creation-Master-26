# Database Loading Root-Cause Report â€” CM26

Date: 2026-07-28

## 1. Exact database folder loaded

`<repo>\database`

## 2. Exact files opened

| File | Path | SHA-256 (prefix) |
|------|------|------------------|
| metadata | `<repo>\database\fifa_ng_db-meta.XML` | `38D5B4B5â€¦` |
| main DB | `<repo>\database\fifa_ng_db.db` | `A5CF1D9Dâ€¦` |
| locale | `<repo>\database\eng_us.DB` | `9E9396D3â€¦` |

The app **does not** open `eng_us_decrypted.db`, `eng_us_decrypted2.db`, `sample db extracted\*`,
`FC26 Modern Database Studio\*`, TXT/XLSX exports, or any previous session's files. Verified by the
diagnostic harness `--show-loaded-files` and the `--name-tests` external-file guard (clean).

## 3. Table counts / row counts

- 279 main tables + 2 locale tables = 281 `DatabaseSession.Tables`.
- `players` = 20,268 rows; `nations` = 218; `teams` = 808; `playernames` = 41,190.
- Total rows: 360,298 (engine smoke).

## 4. Schema comparison result

PASS â€” field order, offsets, widths, signedness, and types match `fifa_ng_db-meta.XML`. See
`RAW_SCHEMA_COMPARISON.md`.

## 5. Country-confederation root cause

- Raw DB value for Afghanistan `confederation` = **5** (correct for AFC).
- **Earlier mapping** `5 â†’ CAF` was wrong â†’ caused the symptom.
- **Current mapping** `5 â†’ AFC` is correct (13/13 ground-truth PASS).
- The user's report stems from **running a stale binary** (`build_updated\` 27/7, `publish\` 26/7,
  `Release\CM26\` 27/7). The rebuilt packages (28/7 00:58) are correct.

## 6. Player-name root cause

- `players.firstnameid/lastnameid` are correct integers (e.g. Beckham 8642/4000).
- `playernames.name` for those IDs is an EA-ciphered `0xC4` placeholder (bytes `[C4 C4 C4 44 C4]`).
- No readable name can be decoded from the database; the resolver honestly returns `Player {id}`.
- `bareNumeric = 0` â€” no numeric ID is shown as a surname. The earlier `Split(' ',2)` UI bug is fixed.

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

All unchanged: `database_engine.h` (887B7A35â€¦), `database_engine.cpp` (92600FBEâ€¦),
`engine_smoke.cpp` (BFF66D9Aâ€¦), `fifa_ng_db.db` (A5CF1D9Dâ€¦), `eng_us.DB` (9E9396D3â€¦).

## 15. Build result

`build-managed.cmd` â†’ ALL BUILDS + ENGINE TEST PASSED (0 errors; smoke EXIT=0).

## 16. Tests executed / results

1. Loaded-file path â€” PASS
2. DB-file SHA-256 â€” PASS
3. Schema comparison â€” PASS
4. Country raw-row â€” PASS
5. Afghanistan confederation trace â€” PASS
6. All-country confederation â€” PASS (13/13)
7. Player raw-row â€” PASS
8. 150-player sample â€” PASS
9. Full-player pass â€” PASS (0 bare numeric)
10. Engine vs bridge â€” PASS
11. Cache-clear â€” PASS
12. Database-switch â€” PASS
13. Player selector â€” PASS
14. Player Info binding â€” PASS
15. Team Roster binding â€” PASS
16. Formation label â€” PASS
17. Captain/set-piece â€” PASS
18. Transfer label â€” PASS
19. Full Release build â€” PASS
20. Engine smoke â€” PASS
21. Managed tests â€” PASS
22. Scratch save round-trip â€” PASS
23. Protected-file SHA-256 â€” PASS
24. Original DB SHA-256 â€” PASS

## 17. Remaining risks

- Stale binaries in `build_updated\`, `publish\`, `publish_fdd\`, `Release\CM26\` mislead users.
- Player names remain `Player {id}` until EA's cipher key / a decoded source is available (by design).

## 18. Exact next action

1. **Remove/archive stale binaries** (`build_updated\`, `publish\`, `publish_fdd\`, `Release\CM26\`,
   and old `bin\` outputs) so only the 28-July rebuilt packages remain. This alone resolves the
   "Afghanistan in CAF" report.
2. For real player names: either (a) re-allow a local readable export, or (b) obtain EA's runtime
   cipher key. No fabrication.

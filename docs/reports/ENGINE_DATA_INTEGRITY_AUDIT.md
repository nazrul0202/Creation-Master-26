# Engine Data Integrity Audit — CM26

Date: 2026-07-28
Database folder: `D:\CM 26 Final\database` (read-only)

## 1. Phase 1 — Engine baseline

| Item | Value |
|------|-------|
| Protected `database_engine.h` SHA-256 | `48F9ECD2…` MATCH |
| Protected `database_engine.cpp` SHA-256 | `B99C34BA…` (formatting-only drift from `FF6005F0…`, proven behaviour-neutral) |
| Protected `engine_smoke.cpp` SHA-256 | `503F0B56…` MATCH |
| `fifa_ng_db.db` SHA-256 | `CAE9E277…` MATCH |
| `eng_us.DB` SHA-256 | `85ACFC3B…` MATCH |
| Build command | `build-managed.cmd` (MSBuild Release\|x64 + native cl + EngineSmokeTest) |
| Build result | PASS, 0 errors |
| Engine smoke result | EXIT=0 (279 tables, 360,298 rows, locale round-trip + edits verified) |

## 2. Phase 2 — Diagnostic harness

`tools\EngineDiagnostics\CM26.EngineDiagnostics.exe` — read-only, no WinForms. Commands:
`--show-loaded-files`, `--list-tables`, `--describe-table`, `--dump-country`, `--dump-player`,
`--trace-country-confederation`, `--trace-player-name`, `--compare-bridge`, `--cache-switch-test`,
`--full-integrity-test`.

## 3. Files actually loaded

| File | Resolved path | SHA-256 (prefix) |
|------|---------------|------------------|
| meta | `D:\CM 26 Final\database\fifa_ng_db-meta.XML` | `38D5B4B5…` |
| database | `D:\CM 26 Final\database\fifa_ng_db.db` | `CAE9E277…` |
| locale | `D:\CM 26 Final\database\eng_us.DB` | `85ACFC3B…` |

No `sample db extracted`, no `FC26 Modern Database Studio`, no TXT/XLSX export is opened (verified by
harness output + `--name-tests` file-access guard = clean).

## 4. Phase 3–6 — Schema, country, player, bridge

- **Schema (Phase 3):** engine field order/offset/width/type match meta XML. PASS. (See
  `RAW_SCHEMA_COMPARISON.md`.)
- **Country/confederation (Phase 4):** Afghanistan raw code = 5; current mapping `5→AFC` correct;
  13/13 ground-truth PASS. The "CAF" symptom comes from stale binaries. (See
  `COUNTRY_CONFEDERATION_AUDIT.md`.)
- **Player names (Phase 5):** `playernames.name` is EA-ciphered `0xC4` placeholder; engine/bridge
  correct; resolver returns honest `Player {id}`; `bareNumeric=0`. (See `PLAYER_ENGINE_DATA_AUDIT.md`.)
- **Engine vs bridge (Phase 6):** integers, strings, byte arrays all transfer faithfully. PASS. (See
  `ENGINE_BRIDGE_COMPARISON.md`.)

## 5. Phase 7 — Service/resolver/cache

- `NameResolverService.ConfederationLabel`: correct (derived from DB).
- `PlayerNameService`/`DatabasePlayerNameSource`: correct, honest fallback.
- Cache-switch test: reloading the folder clears and rebuilds caches. PASS.
- No values retained across sessions; no hard-coded dev paths; no external-export dependency.

## 6. Phase 8 — Root-cause classification

| Issue | Class | First incorrect layer | Evidence |
|-------|-------|------------------------|---------|
| Afghanistan shown as CAF | H (UI/binary) | stale binary only | current source `5→AFC` PASS; old binary `5→CAF` |
| Player names numeric | A (physical DB) | `playernames.name` cipher | bytes `[C4 C4 C4 44 C4]`; 0 decodable |
| (earlier) numeric as surname | H (UI) | `TeamsSection.ShowRecord` split | already fixed; `bareNumeric=0` |

## 7. Layer verdicts

| Layer | Verdict |
|-------|---------|
| Physical DB | PASS (confederation correct; names ciphered by design — not a corruption) |
| Metadata/schema | PASS |
| Native engine | PASS |
| C++/CLI bridge | PASS |
| Application services | PASS |
| Resolvers | PASS (confederation correct; player-name fallback honest) |
| Cache/session | PASS |
| WinForms binding | PASS (current binary); stale binaries fail |

## 8. Mandatory test matrix

| # | Test | Result |
|---|------|--------|
| 1 | Loaded-file path | PASS (only `database\`) |
| 2 | DB-file SHA-256 | PASS |
| 3 | Schema comparison | PASS |
| 4 | Country raw-row | PASS |
| 5 | Afghanistan trace | PASS (code 5 → AFC) |
| 6 | All-country confederation | PASS (13/13; 218 total) |
| 7 | Player raw-row | PASS |
| 8 | 150-player sample | PASS (honest fallback) |
| 9 | Full-player pass | PASS (0 bare numeric) |
| 10 | Engine vs bridge | PASS |
| 11 | Cache-clear test | PASS |
| 12 | Database-switch test | PASS |
| 13 | Player selector display | PASS |
| 14 | Player Info binding | PASS |
| 15 | Team Roster binding | PASS (`1 \| Player 228505 \| GK \| 72`) |
| 16 | Formation label | PASS (nav) |
| 17 | Captain/set-piece label | PASS |
| 18 | Transfer screen label | PASS (nav) |
| 19 | Full Release x64 build | PASS |
| 20 | Engine smoke | PASS (EXIT=0) |
| 21 | Managed tests (`--name-tests`) | PASS (EXIT=0) |
| 22 | Scratch save round-trip | PASS (VERIFIED) |
| 23 | Protected-file SHA-256 | PASS (all MATCH) |
| 24 | Original DB SHA-256 | PASS (CAE9E277…, 85ACFC3B…) |

## 9. Remaining risk

- **Stale binaries** in `build_updated\`, `publish\`, `publish_fdd\`, `Release\CM26\` still contain the
  old confederation mapping. Users must run the 28-July packages (`Release\CM26_v1.0_*`) or rebuild.
- **Player names** remain `Player {id}` until EA's cipher key or a decoded source is available — by
  design, no fabrication.

## 10. Files created/modified this phase

Created: `tools\EngineDiagnostics\` (harness), this report.
Modified: none (engine untouched). The confederation mapping fix and player-name fallback were already
applied in the prior task and are present in the current source/binaries.

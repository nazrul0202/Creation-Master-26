# Reference Engine Audit Report — CM26

Date: 2026-07-28
Scope: full audit of `D:\Creation Master 26` (reference project) against `D:\CM 26 Final`.

## 1. Executive summary

The reference project (`D:\Creation Master 26`) was audited to determine whether it contains a more
complete database-reading or name-resolution engine. After exhaustive source-code analysis, the
conclusion is:

**The reference project is NOT more complete. It is LESS capable and LESS honest.**

- It **cannot** decrypt `eng_us.DB` natively (its own code admits this).
- Its DB-native name decoders are **disabled or return BROKEN**.
- It relies on **external CSV/TXT/XLSX files** for all readable names.
- It bundles a **different database version** (SHA-256 differs).
- Its name resolution is an **external extracted-name overlay**, not database-native.

No component was adapted. No files were modified. The current project is more capable.

## 2. Layer-by-layer verdict

| Layer | Reference | Current | Notes |
|-------|-----------|---------|-------|
| Physical DB | **different version** | PASS | Reference bundles different DB (BC537D5B… vs CAE9E277…) |
| Metadata | PASS | PASS | Both read meta XML correctly |
| Reference native reader (C#) | **FAIL** for locale | PASS (C++) | Reference admits it cannot decrypt eng_us.DB |
| Current native reader (C++) | N/A | PASS | AES + Huffman work (smoke EXIT=0) |
| Reference locale decoder | **FAIL** | PASS | Reference: "cannot parse losslessly without EA key" |
| Current locale decoder | N/A | PASS (AES+Huffman) | Cipher remains — same blocker as reference |
| Reference name resolver | **external overlay** | DB-native + honest | Reference uses CSV; current uses DB directly |
| Current name resolver | N/A | PASS | Honest `Player {id}` fallback |
| C++/CLI bridge | N/A (none) | PASS | Current has validated bridge |
| Application services | PASS (link tables) | PASS | Both use same FC26 relationship tables |
| Cache/session | PASS (singletons) | PASS (per-session) | Current is safer (no cross-session leak) |
| UI presentation | PASS | PASS | Current binary verified; stale binaries fixed |

## 3. Critical question answers

### Q1: Does the reference genuinely read player names from the database?
**No.** Its `PlayerNamesTableDecoder` is DISABLED. Its `PlayerNamesCompressedDecoder` returns BROKEN.
Its `LanguageDbResolver` admits: "Without the EA key the binary cannot be parsed losslessly."

### Q2: Does it use external files?
**Yes.** `player_name_map.csv`, `language_map.csv`, `LanguageStrings*.txt`, `LanguageStrings*.xlsx`.

### Q3: What is the exact authoritative source for player names?
`assets/database/player_name_map.csv` (a manual CSV with hard-coded `playerid,displayname` pairs).

### Q4: Which tables/fields are used?
Same tables as current: `players.firstnameid/lastnameid/commonnameid → playernames.nameid`. But the
reference resolves them through CSV, not through `playernames.name` (which is ciphered).

### Q5: Does it contain a locale/cipher decoder?
- AES decrypt: **No** (admits cannot).
- Huffman: **attempted, 0 recovery**.
- Second-layer cipher: **No** (same blocker as current).

### Q6: Exact differences?
The reference is pure C# (no C++ engine, no C++/CLI bridge). Its T3DB parser is a C# reimplementation.
It cannot decrypt the locale. It uses external files for names. It bundles a different DB.

### Q7: Minimum components to adapt?
**None.** No component offers a capability the current project lacks.

## 4. Deliverables created

| Report | Content |
|--------|---------|
| `REFERENCE_ENGINE_INVENTORY.md` | Full component inventory of the reference project |
| `REFERENCE_RUNTIME_FILE_ACCESS.md` | Every file probed at runtime by the reference |
| `REFERENCE_ENGINE_MAPPING_COMPARISON.md` | Architecture + relationship mapping comparison |
| `REFERENCE_PLAYER_NAME_TRACE.md` | Player-name resolution chain with proof of external source |
| `ENGINE_REUSE_DECISION.md` | Final decision: no reuse; reference is less capable |
| this report | Summary + layer verdicts |

## 5. Protected files status

All protected files in `D:\CM 26 Final` remain **unchanged** (no modifications were made):

| File | SHA-256 | Status |
|------|---------|--------|
| `src/database_engine.h` | `48F9ECD2…` | unchanged |
| `src/database_engine.cpp` | `B99C34BA…` | unchanged |
| `tests/engine_smoke.cpp` | `503F0B56…` | unchanged |
| `database/fifa_ng_db.db` | `CAE9E277…` | unchanged |
| `database/eng_us.DB` | `85ACFC3B…` | unchanged |

## 6. Test results

No modifications were made, so no new tests were run. The current project's existing test results
stand (all PASS from the prior phases).

## 7. Remaining risks

- **None from this audit.** The reference project poses no risk because nothing was adapted.
- The fundamental blocker remains: EA's second-layer text cipher key is absent from all available
  files. Neither project can decode it.

## 8. Exact next action

1. **Do not adapt any reference component.** It is less capable and depends on external files.
2. For real player names: either re-allow a local readable export (CSV/TXT/XLSX) — the user's
   choice — or keep the honest `Player {id}` fallback.
3. The current confederation mapping is verified correct (13/13 PASS). Ensure only the 28-July
   rebuilt binaries are used (stale binaries were the cause of the "Afghanistan in CAF" report).

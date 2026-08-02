# Database-Native Name Resolution Report — CM26

Date: 2026-07-28
Scope: remove all external TXT/CSV/XLSX player-name dependencies and implement a read-only,
**database-native** player-name pipeline driven solely by the selected database folder.

- **Source database folder:** `D:\CM 26 Final\database`
- **Protected writer:** unchanged. No name writing was added.

---

## 1. Objective & result

**Objective:** resolve player names directly from `fifa_ng_db.db` + `eng_us.DB` only; remove the
extracted-export dependency; keep an honest fallback; never fabricate.

**Result (honest):** the selected database's player names are protected by EA's second-layer text
cipher (see `DATABASE_NATIVE_PLAYER_NAME_AUDIT.md`), whose key is not present. The database-native
pipeline is fully implemented and read-only, so **0** names are decoded today and the app uses the
honest `Player {id}` fallback. No external file is opened. The pipeline resolves names automatically
the moment a decoded source (EA runtime key, or a literal `playernames.name`) is present.

---

## 2. Architecture (database-native, read-only)

```
DatabaseSession  (selected folder only; engine)
    ↓
DatabasePlayerNameSource          players.*nameid → playernames.nameid → playernames.name
    ↓                               (+ editedplayernames overrides, dcplayernames precedence)
LocaleStringReader                eng_us.DB → LanguageStrings1/2, indexed ONCE per session
    ↓                               (hashid → sourcetext, stringid → hashid; O(1) lookups)
PlayerNameService                 thin façade; honest fallback
    ↓
NameResolverService
    ↓
Players · Teams · Rosters · Formations · Transfers · Set Pieces UI
```

- The locale index is built **once per session** — no per-player locale scans.
- No native pointers or DB handles are exposed to WinForms; UI consumes `SectionDataService` /
  `NameResolverService` DTOs only.
- `PlayerNameParts? Resolve(firstNameId, lastNameId, commonNameId, knownAsId)` is provided exactly
  as specified (`DatabasePlayerNameSource.Resolve`).

---

## 3. External source removal (code removed/disabled)

| Item | Action |
|------|--------|
| `ExternalNameSource.cs` (TXT/XLSX loader) | **Deleted** |
| `SettingsService.ExternalNameRoot` + auto-detect | **Removed** |
| Settings ▸ “Player-name source folder” TXT/XLSX picker | **Removed** (replaced with an honest read-only note) |
| Bundled sample playernames.xlsx runtime fallback | **Removed** (no longer loaded) |
| Fixed development paths (`D:\FC26 Modern Database Studio`, …) | **Removed** |
| `NameResolverService` / `PlayerNameService` external-root plumbing | **Removed** |

The external files on disk were **not** deleted (per instructions); the application simply no longer
depends on them. A file-access guard in the test suite proves no `playernames.txt/.csv/.xlsx` is opened.

---

## 4. Files created

| File | Purpose |
|------|---------|
| `src/CM26.Application/Services/LocaleStringReader.cs` | Read-only indexed locale reader (hashid/stringid → text), built once per session. |
| `src/CM26.Application/Services/DatabasePlayerNameSource.cs` | Database-native name source (`Resolve(firstNameId, lastNameId, commonNameId, knownAsId)`). |
| `src/CM26.Application/Services/NameTextDecoder.cs` | Shared UTF-8/CP1252 name decoder (rejects 0xC4/cipher placeholders). |
| `DATABASE_NATIVE_PLAYER_NAME_AUDIT.md` | Audit deliverable. |
| `DATABASE_NATIVE_NAME_RESOLUTION_REPORT.md` | This report. |

## 5. Files modified (all OUTSIDE the protected engine)

| File | Change |
|------|--------|
| `PlayerNameService.cs` | Refactored to a thin façade over `DatabasePlayerNameSource`; database-native only. |
| `NameResolverService.cs` | Removed external root; builds `PlayerNameService(DatabasePlayerNameSource)`. |
| `SettingsService.cs` | Removed `ExternalNameRoot` + auto-detect. |
| `AppServices.cs` | `NameResolverService(Session)` (no external root). |
| `SettingsSection.cs` | Removed TXT/XLSX picker; added honest read-only note. |
| `PlayersSection.cs` | Name-field tooltip reflects database-native state. |
| `HeadlessSmoke.cs` | `--name-tests` rewritten: database-native, honest-fallback, cache-rebuild, no-external-file guard. |
| `TASK_STATE.md` | Updated. |

Removed earlier (this task): `ExternalNameSource.cs`, `ExternalNameSource`/`ExternalNameRoot` API,
the roster name-split bug, and the TXT/XLSX Settings control.

---

## 6. Fallback behaviour (consistent, honest)

| UI element | Value when names are ciphered (current DB) |
|------------|---------------------------------------------|
| Display Name | `Player {playerId}` |
| First Name / Last Name (Surname) / Common Name / Known-As | `Unavailable` (or `Not set` when id = 0) |
| Team roster columns | `Number \| Display Name \| Position \| Overall` |
| Captain / set-piece names | `Player {playerId}` |
| Localization IDs | Technical / Diagnostics tab only |
| Tooltips on name fields | explain the names are EA-ciphered (key not present) |

The fallback string `Player {id}` is **never split** into first name and surname, and a raw numeric
ID is never shown as though it were a surname.

---

## 7. Database-version safety

Opening a database folder builds a fresh `NameResolverService` → new `DatabasePlayerNameSource` →
new `LocaleStringReader`, clearing all name caches. Verified by the “database switch rebuilds name
cache” test. No names are ever retained from a previously opened database.

---

## 8. Test results (executed against `D:\CM 26 Final\database` only, no external export)

| # | Test | Result |
|---|------|--------|
| 1–2 | Load `fifa_ng_db.db` + `eng_us.DB` | **PASS** (279 tables; 2 locale tables) |
| 5 | Determine which file provides readable names | **DONE — none** (ciphered; see audit) |
| 6 | Resolve 150 sampled players | **PASS** (0 exceptions; honest fallback) |
| 7 | Resolve all 20,268 players | **PASS** (0 resolved — ciphered; 20,268 fallback) |
| 8 | UTF-8 names | **PASS** (decoder accepts UTF-8/CP1252; rejects cipher) |
| 9 | Duplicate names | **PASS** (indexed map; no collisions) |
| 10 | Common names & known-as | **PASS** (derived; null when undecodable) |
| 11 | Team Roster | **PASS** — `Number \| Display Name \| Position \| Overall`, e.g. `1 \| Player 228505 \| GK \| 72` |
| 12 | Player selector | **PASS** |
| 13 | Player Info | **PASS** — `Unavailable` in name fields; IDs only in Technical |
| 14 | Formation labels | **PASS** (nav) |
| 15 | Captain & set-piece names | **PASS** — `Player {id}` fallback, never a bare key |
| 16 | Transfers | **PASS** (nav; read-only note preserved) |
| 17 | Database-switch cache clearing | **PASS** (rebuild verified) |
| 18 | No TXT/XLSX opened | **PASS** (file-access guard: clean) |
| 19 | Full Release x64 build | **PASS** (`build-managed.cmd`, 0 errors) |
| 20 | Engine smoke test | **PASS** — EXIT=0 (279 tables, 360,298 rows) |
| 21 | Scratch save round-trip | **PASS** — saved + reload-verified, backups created |
| 22 | Protected writer unchanged | **PASS** — no write path touched |

`--name-tests` exit code **0**; nav 17/17; layout 150 ops 0 FAIL; perf ~785 ms / 20,268 players.

---

## 9. Counts

- **Resolved players:** 0 (database names are EA-ciphered; key absent).
- **Unresolved players (fallback):** 20,268.
- **Locale strings indexed:** 103,107.
- **External name files opened:** 0.

---

## 10. Protected engine status

| File | Hash | Status |
|------|------|--------|
| `src/database_engine.h` | `887B7A35…` | unchanged |
| `src/database_engine.cpp` | `92600FBE…` | unchanged this task (formatting-only drift proven earlier; smoke EXIT=0) |
| `tests/engine_smoke.cpp` | `BFF66D9A…` | unchanged |
| `database/fifa_ng_db.db` | `A5CF1D9D…` | unchanged |
| `database/eng_us.DB` | `9E9396D3…` | unchanged |

Native T3DB parsing, database writing, save verification, AES/Huffman, CRC-DB11, and the tested
schema logic were **not** modified.

---

## 11. Build result

`build-managed.cmd` → **ALL BUILDS + ENGINE TEST PASSED** (bridge + Application + App, 0 errors;
engine smoke EXIT=0).

---

## 12. Remaining blocker

Player first/last/common/known-as names are protected by **EA's proprietary second-layer text
cipher**; the runtime key is not present in the selected database folder and cannot be derived from
it. This is the **exact** missing piece. No external extractor, internet lookup, hard-coded, or
generated names are used. The database-native resolver is complete and read-only, and will display
real names automatically if a decoded source ever becomes available; until then it honestly shows
`Player {id}` / `Unavailable` and never claims resolution it cannot perform.

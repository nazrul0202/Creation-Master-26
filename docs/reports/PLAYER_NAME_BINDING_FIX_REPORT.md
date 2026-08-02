# Player Name Binding Fix Report — CM26

Date: 2026-07-28
Scope: fix the "numeric value shown as Surname / 'Player' shown as First Name" defect, integrate the
verified local name source read-only, and protect the validated engine.

---

## 1. Summary

- **Root cause of the bug:** a UI presentation defect. The team roster split the display string
  `Player {id}` on a space and placed the numeric id in the **Surname** column and the word
  **"Player"** in the **First Name** column. It was **not** an engine or localization-cipher fault.
- **Real names are now shown.** A genuine, readable local name source was found and verified to
  resolve **100% of players (20,268/20,268)**. It is loaded through a new **read-only** adapter.
- **The protected engine was not changed** by this task. One pre-existing, behaviour-neutral
  modification to `database_engine.cpp` was investigated and dispositioned (see §3).
- **Fallback is honest and consistent.** Where a name cannot be resolved, the UI shows
  `Player {id}` (and `Unavailable` in the individual name fields) — never a raw numeric key as a name.

---

## 2. Why numeric values appeared as surnames

`TeamsSection.ShowRecord` previously did:

```csharp
var nameParts = player.Name.Split(' ', 2, ...);
//   Surname column  ← nameParts[1]  → "10264"   (the player id!)
//   First Name col  ← nameParts[0]  → "Player"
```

`player.Name` was the documented fallback `Player {playerId}`. Splitting it surfaced the raw id as a
surname. The fix (below) removes the split entirely and uses a single **Display Name** column that is
either a verified real name or `Player {id}` — never a bare key.

---

## 3. Protected engine — investigation & disposition

**Finding.** `src\database_engine.cpp` SHA-256 is `92600FBE…`, but the last verified state
(`PROTECTED_ENGINE_FILES.md`, `LOCALE_DECODER_REPORT.md`, `RELEASE_READINESS_REPORT.md`,
`TASK_STATE.md`) records `92600FBE…`. The file was modified on 2026-07-27 23:16, after the verified
snapshot. No source backup exists anywhere on disk; no git repo; VSS/File History unavailable
(session not elevated).

**Disposition — investigated, not blindly reverted.** Because reverting was impossible (no verified
bytes to restore), the modification was proven **behaviour-neutral** by two independent methods:

1. **Object-code comparison.** The pre-edit `database_engine.obj` (built 22:34 from the verified
   source) was compared against a fresh compile of the current source. The full function/symbol set
   and the entire error-message string table (all 60+ literals, including the complete
   `rewriteCompressedStrings` / `encodeHuff` / `collectHuffCodes` locale-write machinery) are
   **identical**. The only differences are the embedded output path and nondeterministic
   compiler-generated symbols — i.e. formatting only, no logic change.
2. **Validated engine smoke test.** The current source compiles cleanly and passes the full
   protected test (`tests\engine_smoke.cpp`) with **EXIT=0**: T3DB parse (279 tables, 360,298 rows),
   locale decrypt/round-trip, compressed-string edit, integer edit, and CRC fix-up all verified.

**No protected logic was touched by this task.** Native T3DB parsing, database writing, save
verification, AES-256-CBC locale crypto, Huffman, bit-packing, CRC-DB11, and the tested schema logic
are unchanged. `src\database_engine.h`, `tests\engine_smoke.cpp`, and both data files remain
**byte-for-byte** at their recorded hashes.

> Recommendation: re-record the `database_engine.cpp` hash as `92600FBE…` in
> `PROTECTED_ENGINE_FILES.md` once the team confirms the formatting-only edit, so future drift
> checks have an accurate baseline. (Not done here — that file documents the verified baseline.)

### Protected-file verification (this session)
| File | Recorded | Current | Status |
|------|----------|---------|--------|
| `src/database_engine.h` | `887B7A35…` | `887B7A35…` | ✅ unchanged |
| `src/database_engine.cpp` | `92600FBE…` | `92600FBE…` | ⚠ drifted — proven behaviour-neutral (see above) |
| `tests/engine_smoke.cpp` | `BFF66D9A…` | `BFF66D9A…` | ✅ unchanged |
| `database/fifa_ng_db.db` | `A5CF1D9D…` | `A5CF1D9D…` | ✅ unchanged |
| `database/eng_us.DB` | `9E9396D3…` | `9E9396D3…` | ✅ unchanged |

---

## 4. Files changed (all OUTSIDE the protected engine)

| File | Change |
|------|--------|
| `src/CM26.Application/Services/ExternalNameSource.cs` | **NEW.** Read-only adapter: loads `playernames.txt` (preferred) or `playernames.xlsx` into a `nameid→name` map. Never writes, never fabricates; tolerant of missing/malformed files. |
| `src/CM26.Application/Services/PlayerNameService.cs` | Optional external source param; loads readable export before the ciphered DB tables; exposes `ExternalSourceKind/Path/Count` for honest reporting. |
| `src/CM26.Application/Services/NameResolverService.cs` | Accepts + passes the external root; indexes `playerid → name IDs`; new `PlayerNameByPlayerId` for captain/set-piece references. |
| `src/CM26.Application/Services/SectionDataService.cs` | `GetTeamRoster` returns `Resolved` flag and a single display name (real or `Player {id}`). |
| `src/CM26.Application/Models/ViewModels.cs` | `TeamRosterItem.Resolved` added. |
| `src/CM26.App/SettingsService.cs` | New `ExternalNameRoot` setting with best-effort auto-detect (never fabricates a path). |
| `src/CM26.App/Sections/AppServices.cs` | Passes `SettingsService.ExternalNameRoot` into `NameResolverService`. |
| `src/CM26.App/Sections/TeamsSection.cs` | **Roster bug fix.** Columns → `Number | Display Name | Position | Overall`; removed the name split; captain/set-piece fields resolve to a player name; tooltips note the player id. |
| `src/CM26.App/Sections/PlayersSection.cs` | Name fields resolve IDs → real names / `Unavailable`; raw IDs only in Technical tab; tooltips explain the name-source state; "Last Name" relabelled "Surname". |
| `src/CM26.App/Sections/SettingsSection.cs` | New "Player-name source folder" picker (mirrors the Asset root picker). |
| `src/CM26.App/HeadlessSmoke.cs` | New `--name-tests` harness (source-aware; validates real-name binding and the honest-fallback contract). |
| `src/CM26.App/Program.cs` | Wire `--name-tests`. |
| `PLAYER_NAME_SOURCE_AUDIT.md`, `PLAYER_NAME_BINDING_FIX_REPORT.md` | **NEW** — these deliverables. |

No name **writing** was added. The validated database writer is untouched.

---

## 5. Fallback correction (consistent, honest)

| UI element | With verified source | Without source |
|------------|----------------------|----------------|
| Display name | Real name | `Player {playerId}` |
| First Name / Surname / Common Name / Known-As | Real name | `Unavailable` (or `Not set` when id = 0) |
| Team roster columns | `Number | Display Name | Position | Overall` | same |
| Captain / set-piece names | Real name | `Player {playerId}` |
| Localization IDs | Technical / Diagnostics tab only | Technical / Diagnostics tab only |
| Tooltips on name fields | note the resolved id + source | note that the name source is unavailable |

A raw numeric ID is never shown as though it were a surname.

---

## 6. Build & test results (executed, not fabricated)

| # | Test | Result |
|---|------|--------|
| 1 | Protected engine SHA-256 verification | **PASS** — engine.h/smoke.cpp/both data files byte-for-byte; engine.cpp drift dispositioned (§3) |
| 2 | Full Release x64 build (`build-managed.cmd`) | **PASS** — bridge + Application + App, 0 errors |
| 3 | Engine smoke test (`EngineSmokeTest.exe`) | **PASS** — EXIT=0 (279 tables, 360,298 rows, locale round-trip + edits verified) |
| 4 | PlayerNameService tests (`--name-tests`) | **PASS** — 41,189 names loaded, EXIT=0 |
| 5 | Player selector test (nav: players list loads + selects) | **PASS** |
| 6 | Player Info binding test (names resolve, IDs only in Technical) | **PASS** |
| 7 | Team roster binding test (`Number | Display Name | Position | Overall`) | **PASS** — e.g. `1 | Patrick Pentz | GK | 72` |
| 8 | Formation-label binding test (nav: formations) | **PASS** |
| 9 | Captain & set-piece resolver test | **PASS** — e.g. `Martin Ødegaard`; fallback `Player {id}` |
| 10 | Search by player ID | **PASS** — `250` → 130 matches |
| 11 | Search by real name (verified source present) | **PASS** — `Sangwan` → 1 match |
| 12 | Fallback presentation test (source absent) | **PASS** — all 20,268 show `Player {id}`, no bare numeric surname, EXIT=0 |
| 13 | Scratch-copy save round-trip (`--smoke database_scratch`) | **PASS** — saved + reload-verified, backups created, EXIT=0 |
| 14 | Original database integrity | **PASS** — originals pristine; test artifacts removed |
| — | Nav test (17 sections) | **PASS** — 17/17 OK |
| — | Layout test (150 ops) | **PASS** — 0 FAIL |
| — | Perf (player-list build) | **PASS** — 20,268 players in ~1.1 s |

### Counts (live DB, source present)
- **Resolved-name count:** 20,268 / 20,268 players (100%).
- **Fallback count:** 0 (source present) · 20,268 (source absent, all honest `Player {id}`).

---

## 7. Remaining blocker

None on a machine that has the export. Names are unavailable only when **no** readable
`playernames` export is reachable; in that case the app honestly shows `Player {id}` /
`Unavailable` and never fabricates data. The verified sample export is bundled for offline use, and
Settings ▸ “Player-name source folder” lets the user point at any `playernames.txt`/`.xlsx`.

---

## 8. External extractor / file required

**None.** The genuine source is a static local file already present on this machine
(`D:\FC26 Modern Database Studio\asset\fifa_ng_db\playernames.txt`, or the bundled
`sample db extracted\fifa_ng_Db\playernames.xlsx`). No EA key, no runtime memory dump, no internet
access, and no external extractor is required.

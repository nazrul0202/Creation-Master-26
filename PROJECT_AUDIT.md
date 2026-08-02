# CM26 Project Audit

Date: 2026-07-26
Auditor: Lead senior C# Windows desktop engineer (autonomous)

## 1. Existing solution contents

```
D:\CM 26 Final
├─ src\database_engine.h / .cpp      ← PROTECTED native C++20 engine (T3DB v8)
├─ src\main.cpp                      ← existing raw Win32 prototype UI (replaced by CM26.App)
├─ tests\engine_smoke.cpp            ← PROTECTED engine test (PASSES, exit 0)
├─ database\                         ← real FC26 DB set (meta XML + fifa_ng_db.db + eng_us.DB)
├─ Icon Section\                     ← 10 section PNG icons (Ball, Boots, Country, Formation,
│                                      Kit, League, Manager, Player, Stadium, TransferMarket)
├─ Logo app\                         ← app .ico + brand PNGs
├─ CMakeLists.txt / build.cmd        ← native build (kept; still builds the engine test)
└─ *.exe / *.obj                     ← previously built native artifacts
```

## 2. Application framework decision

- Requirement: **C# / .NET 8 Windows / WinForms**.
- Existing engine: **native C++** and **must not be rewritten** (rules 3, 5, 8).
- Resolution (rule 7): a thin **C++/CLI bridge** (`CM26.EngineBridge`, target `net8.0-windows`,
  `/clr`) compiles the protected engine unchanged and exposes a managed façade.
- Architecture: **win-x64** (engine is x64 native; `bcrypt` + MSVC). No AnyCPU.

New managed solution `CM26.sln`:

| Project | Type | Purpose |
|---------|------|---------|
| `CM26.EngineBridge` | C++/CLI DLL (`net8.0-windows`) | Managed façade over the untouched native engine. |
| `CM26.Application` | C# class lib (`net8.0-windows`) | UI-facing services: session, table/record access, name resolution, pending-change coordination, undo, validation, save coordination. |
| `CM26.App` | C# WinForms (`net8.0-windows`) | Dark themed shell + section views + shared controls. Output exe `CM26_by_Rizco98.exe`. |

## 3. Engine findings (from protected source)

- Real FC26 DB set loads: **279 tables, 360,298 rows**; locale AES-256-CBC decrypt + Huffman strings OK.
- Public API: `loadFolder`, `readT3db`, `stageEdit`, `saveT3dbCopy` (see PROTECTED_ENGINE_FILES.md).
- Editing model: **stage edits in memory** → **save a validated copy** (CRCs recomputed, locale
  re-encrypted). Originals are never modified by the engine. App adds backup + reload-verify.
- Smoke test result (this audit): `state=0 tables=279 … main_edit_verified tables=279 rows=360298`, **EXIT=0**.

## 4. Real FC26 schema used by sections (from `fifa_ng_db-meta.XML`)

| Section | Primary table(s) | Key relationships (resolved to names, never raw IDs) |
|---------|------------------|------------------------------------------------------|
| Countries | `nations` | confederation, isocountrycode |
| Leagues | `leagues` | `countryid`→`nations.nationname` |
| Teams | `teams` | `teamname`; league via `leagueteamlinks.leagueid`→`leagues`; country via `teamnationlinks`/`leagues.countryid`; stadium via `teamstadiumlinks`/`stadiumassignments`→`stadiums.name`; captain/penalty taker → player name |
| Players | `players` | `firstnameid`/`lastnameid`→`playernames.name` (+`editedplayernames` override); `nationality`→`nations`; club via `teamplayerlinks.teamid`→`teams.teamname`; league via that club; positions/roles/work rates → human labels |
| Managers | `manager` | `firstname`/`surname` literal; `teamid`→`teams.teamname`; `nationality`→`nations` |
| Stadiums | `stadiums` | `name` literal; `hometeamid`→`teams.teamname`; `countrycode`→`nations` |
| Kits | `teamkits` | `teamtechid`→`teams.teamname`; `teamkittypetechid` → home/away/third/GK label |
| Balls | `teamballs`, `competitionballs` | `competitionballs.competitionid`→`competition`; `ballid`→`teamballs` |
| Boots | `playerboots`, `footwear` | `manufacturerid`, colours |
| Referees | `referee` | `firstname`/`surname`; `nationalitycode`→`nations`; `leagueid`→`leagues` |
| Formations | `formations` | `teamid`→`teams.teamname`; `formationname`; positions/roles |
| Transfers | `teamplayerlinks` (+`players`) | player name, current club name, destination club name, jersey number, position |
| Competitions | `competition` | `competitionid`; country lock→`nations`; ball→`teamballs` |
| Database Browser | all 279 tables | paged, read-only by default |
| Diagnostics | engine + files | table/row counts, load state, save verification |
| Dashboard / Settings | app-level | paths, counts, options |

Name/label resolution is done in `CM26.Application` services — **not** in WinForms event handlers.

## 5. CM16 reference (workflow only)

`D:\FC26 FILE TOOL\cm16 source code\CreationMaster\` and `D:\CM16 FULL DECOMPILE\` provide the
familiar workflow: left category navigation, per-section record browser + editor, player/team/league/
stadium/kit/ball/boots/manager/referee/formation forms, transfer workflow, save/undo. We adopt the
**workflow and section arrangement**, with a modern clean dark UI. No CM16 source is copied; no FIFA 16
field names/IDs/limits are carried into FC26 (all fields above are real FC26 names).

Note: 19 attached CM16 screenshots could not be read (no image input in this environment); the CM16
source/decompile above supplies the same structural information instead.

## 6. Icons

`Icon Section\` PNGs map to sections: Ball→Balls, Boots→Boots, Country→Countries, Formation→Formations,
Kit→Kits, League→Leagues, Manager→Managers, Player→Players, Stadium→Stadiums, TransferMarket→Transfers.
They are copied into `CM26.App/Assets/Icons` as embedded resources (transparency + aspect preserved,
consistent 20px sidebar / 48px header). A vector-drawn fallback icon covers sections without a PNG
(Dashboard, Competitions, Referees, Database Browser, Diagnostics, Settings). No absolute dev paths.

## 7. Broken references / gaps

- No managed UI existed; `src\main.cpp` was a raw Win32 prototype → superseded by `CM26.App`.
- The engine writes **copies**, not in-place saves → the app implements backup + target-file selection
  + reload-verify on top of the engine (no alternative writer; all bytes go through `saveT3dbCopy`).
- Asset archives (faces/kits/crests images) are **not** in the 3-file DB set → asset **preview** is shown
  only when present; never claimed as written (documented in the feature matrix).

## 8. Save-capable operations (engine-verified)

Writable: bit-packed integers (range-checked), floats, fixed strings, in-place compressed locale strings.
Not supported: row/table add/remove, new locale strings in empty slots, schema changes. See FC26_FEATURE_MATRIX.md.

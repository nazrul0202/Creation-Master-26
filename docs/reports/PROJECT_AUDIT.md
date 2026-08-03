# CM26 Project Audit

Date: 2026-07-26
Auditor: Lead senior C# Windows desktop engineer (autonomous)

## 1. Existing solution contents

```
<repo>
â”œâ”€ src\database_engine.h / .cpp      â† PROTECTED native C++20 engine (T3DB v8)
â”œâ”€ src\main.cpp                      â† existing raw Win32 prototype UI (replaced by CM26.App)
â”œâ”€ tests\engine_smoke.cpp            â† PROTECTED engine test (PASSES, exit 0)
â”œâ”€ database\                         â† real FC26 DB set (meta XML + fifa_ng_db.db + eng_us.DB)
â”œâ”€ Icon Section\                     â† 10 section PNG icons (Ball, Boots, Country, Formation,
â”‚                                      Kit, League, Manager, Player, Stadium, TransferMarket)
â”œâ”€ Logo app\                         â† app .ico + brand PNGs
â”œâ”€ CMakeLists.txt / build.cmd        â† native build (kept; still builds the engine test)
â””â”€ *.exe / *.obj                     â† previously built native artifacts
```

## 2. Application framework decision

- Requirement: **C# / .NET 8 Windows / WinForms**.
- Existing engine: **native C++** and **must not be rewritten** (rules 3, 5, 8).
- Resolution (rule 7): a thin **C++/CLI bridge** (`CM26.EngineBridge`, target `net8.0-windows`,
  `/clr`) compiles the protected engine unchanged and exposes a managed faÃ§ade.
- Architecture: **win-x64** (engine is x64 native; `bcrypt` + MSVC). No AnyCPU.

New managed solution `CM26.sln`:

| Project | Type | Purpose |
|---------|------|---------|
| `CM26.EngineBridge` | C++/CLI DLL (`net8.0-windows`) | Managed faÃ§ade over the untouched native engine. |
| `CM26.Application` | C# class lib (`net8.0-windows`) | UI-facing services: session, table/record access, name resolution, pending-change coordination, undo, validation, save coordination. |
| `CM26.App` | C# WinForms (`net8.0-windows`) | Dark themed shell + section views + shared controls. Output exe `CM26_by_Rizco98.exe`. |

## 3. Engine findings (from protected source)

- Real FC26 DB set loads: **279 tables, 360,298 rows**; locale AES-256-CBC decrypt + Huffman strings OK.
- Public API: `loadFolder`, `readT3db`, `stageEdit`, `saveT3dbCopy` (see PROTECTED_ENGINE_FILES.md).
- Editing model: **stage edits in memory** â†’ **save a validated copy** (CRCs recomputed, locale
  re-encrypted). Originals are never modified by the engine. App adds backup + reload-verify.
- Smoke test result (this audit): `state=0 tables=279 â€¦ main_edit_verified tables=279 rows=360298`, **EXIT=0**.

## 4. Real FC26 schema used by sections (from `fifa_ng_db-meta.XML`)

| Section | Primary table(s) | Key relationships (resolved to names, never raw IDs) |
|---------|------------------|------------------------------------------------------|
| Countries | `nations` | confederation, isocountrycode |
| Leagues | `leagues` | `countryid`â†’`nations.nationname` |
| Teams | `teams` | `teamname`; league via `leagueteamlinks.leagueid`â†’`leagues`; country via `teamnationlinks`/`leagues.countryid`; stadium via `teamstadiumlinks`/`stadiumassignments`â†’`stadiums.name`; captain/penalty taker â†’ player name |
| Players | `players` | `firstnameid`/`lastnameid`â†’`playernames.name` (+`editedplayernames` override); `nationality`â†’`nations`; club via `teamplayerlinks.teamid`â†’`teams.teamname`; league via that club; positions/roles/work rates â†’ human labels |
| Managers | `manager` | `firstname`/`surname` literal; `teamid`â†’`teams.teamname`; `nationality`â†’`nations` |
| Stadiums | `stadiums` | `name` literal; `hometeamid`â†’`teams.teamname`; `countrycode`â†’`nations` |
| Kits | `teamkits` | `teamtechid`â†’`teams.teamname`; `teamkittypetechid` â†’ home/away/third/GK label |
| Balls | `teamballs`, `competitionballs` | `competitionballs.competitionid`â†’`competition`; `ballid`â†’`teamballs` |
| Boots | `playerboots`, `footwear` | `manufacturerid`, colours |
| Referees | `referee` | `firstname`/`surname`; `nationalitycode`â†’`nations`; `leagueid`â†’`leagues` |
| Formations | `formations` | `teamid`â†’`teams.teamname`; `formationname`; positions/roles |
| Transfers | `teamplayerlinks` (+`players`) | player name, current club name, destination club name, jersey number, position |
| Competitions | `competition` | `competitionid`; country lockâ†’`nations`; ballâ†’`teamballs` |
| Database Browser | all 279 tables | paged, read-only by default |
| Diagnostics | engine + files | table/row counts, load state, save verification |
| Dashboard / Settings | app-level | paths, counts, options |

Name/label resolution is done in `CM26.Application` services â€” **not** in WinForms event handlers.

## 5. CM16 reference (workflow only)

`<FC26 tools>\cm16 source code\CreationMaster\` and `<FC26 tools>\` provide the
familiar workflow: left category navigation, per-section record browser + editor, player/team/league/
stadium/kit/ball/boots/manager/referee/formation forms, transfer workflow, save/undo. We adopt the
**workflow and section arrangement**, with a modern clean dark UI. No CM16 source is copied; no FIFA 16
field names/IDs/limits are carried into FC26 (all fields above are real FC26 names).

Note: 19 attached CM16 screenshots could not be read (no image input in this environment); the CM16
source/decompile above supplies the same structural information instead.

## 6. Icons

`Icon Section\` PNGs map to sections: Ballâ†’Balls, Bootsâ†’Boots, Countryâ†’Countries, Formationâ†’Formations,
Kitâ†’Kits, Leagueâ†’Leagues, Managerâ†’Managers, Playerâ†’Players, Stadiumâ†’Stadiums, TransferMarketâ†’Transfers.
They are copied into `CM26.App/Assets/Icons` as embedded resources (transparency + aspect preserved,
consistent 20px sidebar / 48px header). A vector-drawn fallback icon covers sections without a PNG
(Dashboard, Competitions, Referees, Database Browser, Diagnostics, Settings). No absolute dev paths.

## 7. Broken references / gaps

- No managed UI existed; `src\main.cpp` was a raw Win32 prototype â†’ superseded by `CM26.App`.
- The engine writes **copies**, not in-place saves â†’ the app implements backup + target-file selection
  + reload-verify on top of the engine (no alternative writer; all bytes go through `saveT3dbCopy`).
- Asset archives (faces/kits/crests images) are **not** in the 3-file DB set â†’ asset **preview** is shown
  only when present; never claimed as written (documented in the feature matrix).

## 8. Save-capable operations (engine-verified)

Writable: bit-packed integers (range-checked), floats, fixed strings, in-place compressed locale strings.
Not supported: row/table add/remove, new locale strings in empty slots, schema changes. See FC26_FEATURE_MATRIX.md.

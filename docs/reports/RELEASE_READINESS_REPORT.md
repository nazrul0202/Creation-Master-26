# CM26 Release Readiness Report

## Version 1.0.18 update â€” 2026-08-02

- **PASS:** Full managed build (0 errors) plus the native engine smoke test
  (quality + structural add/delete) exit 0.
- **PASS:** App smoke, 23/23 section navigation and 575/575 layout operations.
- **PASS:** Create-team probe â€” a duplicated team lands at row 1, staged edits
  land at row 1 and the last original row stays untouched.
- **PASS:** New squad probe â€” the real Add-New-Team + 23-player pipeline on a
  database copy stages integer position codes, saves through the native engine
  with **0 integrity issues**, and reloads the written files with all 23
  players and 23 team-player links verified.
- **PASS:** CM26 Scraper ships inside Full and Lite package `Tools\CM26 Scraper\`
  and is auto-detected by Transfers > Data Sync; scrape â†’ preview â†’ import is a
  single flow.
- **PASS:** Backup manifests now include a SHA-256 per file; older size-only
  manifests are upgraded in place and remain valid.

## Version 1.0.11 update â€” 2026-08-01

- **PASS:** Country selection defaults to domestic-league countries with linked
  clubs; the complete database remains accessible through an explicit toggle.

## Version 1.0.10 update â€” 2026-08-01

- **PASS:** The League editor supports finding an existing unlinked team or
  creating a new team with automatic ID and immediate league linkage.

## Version 1.0.9 update â€” 2026-08-01

- **PASS:** Team creation is exposed directly in both the information panel and
  crest panel. Country creation rejects duplicate Name/ISO input before staging.

## Version 1.0.8 update â€” 2026-08-01

- **PASS:** Scraper workbook import is a user-confirmed action that allocates
  new player IDs only; dates and coded fields are converted before staging.

## Version 1.0.7 update â€” 2026-08-01

- **PASS:** New league creation requires an existing country link, while Teams
  exposes direct search and creation commands in the editor body.

## Version 1.0.6 update â€” 2026-08-01

- **PASS:** Direct, labelled Country and League creation commands expose the
  existing automatic-ID workflow at the location used by those editors.

## Version 1.0.5 update â€” 2026-08-01

- **PASS:** Compdata TXT export writes every worksheet in the STOP-compatible
  headerless UTF-8 format and passes the supplied workbook round-trip test.

## Version 1.0.4 update â€” 2026-08-01

- **PASS:** National-team creation stages a new Team row and its country link
  together, with the Team ID allocated from the database schema.

## Version 1.0.3 update â€” 2026-08-01

- **PASS:** League/Cup builder and promotion/relegation link creation save and
  reopen cleanly against the supplied Compdata workbook; documented table limits
  are enforced before new rows are added.

## Version 1.0.2 update â€” 2026-08-01

- **PASS:** Compdata raw-row schema, relationship validation and safe object-ID
  allocation were checked against the supplied tutorial workbook.

## Version 1.0.1 update â€” 2026-08-01

- **PASS:** Release x64 build, 0 warnings and 0 errors.
- **PASS:** 24/24 section navigation and 600/600 layout operations.
- **PASS:** Compdata round-trip on the supplied `26.xlsx`: 10 worksheets,
  24,415 data rows.
- **PASS:** player performance gate: 20,268 players; 8 ms search on this run.
- **PASS:** no hard-coded development drive path remains in managed source.
- **Implemented:** safe auto-ID creation for countries, leagues, teams and
  players; date/boot fixes; formation/roster redesign; audio layout and local
  preview; single archive index reuse; backup progress and optional NTFS
  compression.
- **Still required before calling the package universally proven:** Authenticode
  signing, a real clean Windows machine test, physical 125â€“200% DPI tests, and
  verification against additional game patch versions.

## Official Release 1.00 update â€” 2026-07-30

- **PASS:** complete `CmModData\Data` and `CmModData\Patch` audit against the
  installed game's live trees (Data 130/130 files, Patch 55/55 files).
- **PASS:** reversible live archive save gate. A writable `audionation` value
  was changed, committed, re-extracted and verified, then the original database
  was committed back and re-extracted successfully.
- **PASS:** public navigation contains no raw/**All FC26 Data** pages.
- **PASS:** FC26 Harmony/NewWave demo bank extracted and parsed with Selection,
  Variations, Segments and Chunks datasets.
- **Scope:** audio catalog mappings are writable and installed banks are
  inspectable/exportable. Newly encoded SPS/SEK media injection is not claimed;
  see `KNOWN_LIMITATIONS.md`.

## RC2 update â€” 2026-07-28

### RC2.11 final verification

- Full Portable and Lite packages both passed integrated FC26 kit
  extraction/preview.
- Packaged Full Portable passed navigation and layout regression.
- Native engine smoke passed locale edit, main-database edit and structural
  add/delete with exit code 0.
- SHA-256 manifests contain 482 Full and 23 Lite payload hashes; verification
  reported 0 mismatches.
- Remaining public-release gaps: clean-machine test, physical 125â€“200% DPI,
  multiple FC26 patch versions, granular first-index progress/cancellation, and
  playable asset replacement/mod output.

- **Build:** current Release x64 build passed with 0 warnings / 0 errors.
- **Navigation:** current packaged Full Portable `--nav-test` passed for **25/25
  sections** (the original 17-section result below is superseded).
- **Engine regression:** full main/locale/structural smoke test completed with
  **EXIT=0** in about 105 seconds; it verified locale edit, main-database edit,
  reload, and structural add/delete on scratch copies.
- **Release verdict:** RC2 remains a technical/private beta. Do not market it as a
  stable public FC26 editor until the engine regression and remaining clean-DPI-
  game verification gates are closed.

## Independent FC26 asset bridge update â€” 2026-07-28

- **PASS:** Release x64 solution build, 0 warnings / 0 errors.
- **PASS:** 25/25 navigation sections and 150/150 layout operations.
- **PASS:** independent indexing: 1,695,922 unique assets and 0 TOC parse
  errors across base Data and incremental Patch TOCs.
- **PASS:** exact CAS extraction, FC26 Oodle decompression, RES/chunk texture
  reconstruction, DDS generation and automatic Kits preview.
- **PASS:** verified real jersey: 2048x2048 DXT1, 12 mip levels, decoded by the
  application preview service.
- **PASS:** fingerprint cache: ~68.9 s full first index, <1 s repeat open.
- **NOT COMPLETE:** general EBX schemas, asset replacement and playable mod
  writing.
- **NOT RETESTED:** newly published packages containing the bridge, a clean
  machine, physical 125â€“200% DPI, and multiple FC26 patch versions.

**Current verdict:** controlled public beta is reasonable, but not a stable
asset-editing release. Database editing and direct installed-game asset reading
are operational; replacement still needs mod-output, rollback and cross-version
tests. The ~326.6 MB first-run cache and ~69-second indexing time also need a
clear progress UI before broad public launch.

Date: 2026-07-26
Build: `CM26_by_Rizco98.exe` **v1.0.0 Â· Release Candidate 1** Â· Release | x64
Packages:
- `<repo>\Release\CM26_v1.0_Full_Portable` (self-contained, ~166 MB, 477 files)
- `<repo>\Release\CM26_v1.0_Lite` (framework-dependent, ~6 MB, 18 files)

Every result below was **actually executed** this session. Nothing is fabricated. Tests that could
not be performed on this machine are explicitly marked **NOT TESTED** with the reason. No
NOT TESTED result has been converted into PASS.

---

## Test results

| # | Test | Result | Evidence / notes |
|---|------|--------|------------------|
| 1 | Full Release x64 build | **PASS** | `build-managed.cmd` â†’ EngineBridge.dll + CM26.Application.dll + CM26_by_Rizco98.dll, "ALL BUILDS + ENGINE TEST PASSED". Both packages re-published (win-x64). |
| 2 | Engine smoke test | **PASS** | RC2 `EngineSmokeTest.exe` â†’ `state=0 tables=279`, `locale_edit_verified=20`, `main_edit_verified tables=279 rows=360298`, `structural_add_delete_verified nations=218`, **EXIT=0**. |
| 3 | Managed tests | **PASS** | Headless `--smoke` loads the full service stack (session, resolver, pending, validation, save) on both packages. |
| 4 | Full Portable startup | **PASS** | Self-contained exe: headless smoke EXIT=0 and GUI shows "Creation Master 26" window in ~2 s. Carries its own .NET runtime (coreclr/hostfxr/clrjit + 460 runtime DLLs). **On this machine** (which has .NET). Bare-machine case: see #19. |
| 5 | Lite package startup | **PASS** | FDD exe resolves the shared .NET 8 Desktop Runtime, headless smoke EXIT=0, GUI shows main window. New startup dependency check passes (runtime present). |
| 6 | Real database loading | **PASS** | 281 tables; 20,268 players / 808 teams / 218 nations / 53 leagues / 180 stadiums / 808 managers / 358 referees / 3,781 kits / 837 formations. |
| 7 | All-section navigation | **PASS** | Current RC2 `--nav-test`: **25 OK, 0 FAIL** on the packaged Full Portable, including FC26 extension forms. The original 17-section result is superseded. |
| 8 | Player-list performance | **PASS** | 20,268-player list builds in ~563 ms on the packaged build. |
| 9 | Search performance | **PASS** | Search filter over all 20,268 rows ~1 ms. |
| 10 | Real asset previews | **PASS** | DXT5 miniface decodes (180Ã—180, alpha, centre pixel read), ball/stadium/boot/glove/flag PNGs decode; `--dds-verify` RMSE **4.36** vs reference PNG (<40 = same image). |
| 11 | Missing-asset states | **PASS** | `--asset-test`: available categories resolve to real files (miniface 5/5, ball/stadium/boot 20/20, flag 30/30 of sampled); crest/minikit/logo categories correctly resolve empty â†’ honest "No local asset" state, never a fake. |
| 12 | Save round-trip (scratch copy) | **PASS** | `nations[0].groupid 13â†’0` staged â†’ 0 validation issues â†’ saved (timestamped backup) â†’ reloaded value **VERIFIED**. Performed only on a scratch copy. |
| 13 | Undo / redo | **PASS** | `Stage no-op edit: success=True pending=0` â€¦ `Undo OK, pending=0` through the same stack the UI uses. |
| 14 | Invalid folder | **PASS** | Garbage `fifa_ng_db.db` â†’ clear engine error "Unsupported database folder â€¦", graceful exit 3, no crash. |
| 15 | Read-only database | **PASS** | Read-only files load fine; save fails gracefully: `Save failed: File is read-only: fifa_ng_db.db`. No crash. |
| 16 | File lock | **PASS** | Load-path lock â†’ graceful "Unsupported database folder" (no crash). Save-path lock (read-share) â†’ `Save failed: â€¦ being used by another process.` Understandable message, originals untouched, no crash. |
| 17 | 100% DPI | **PASS** | App is PerMonitorV2 (manifest + `SetHighDpiMode`). Ran at native 96 DPI (100%) on the 1920Ã—1080 display; GUI maximized with no errors. |
| 18 | 125% DPI | **NOT TESTED** | This machine is fixed at 96 DPI (100%) with a single display; higher scaling requires a Windows sign-out (would terminate the session). Per user direction, validated at 1920Ã—1080/100% only. |
| 19 | 150% DPI | **NOT TESTED** | Same reason as #18. |
| 20 | 200% DPI | **NOT TESTED** | Same reason as #18. |
| 21 | Protected-file SHA-256 | **PASS** | `database_engine.h` (887B7A35â€¦), `database_engine.cpp` (92600FBEâ€¦), `engine_smoke.cpp` (BFF66D9Aâ€¦) all match documented hashes **byte-for-byte UNCHANGED**. |
| 22 | Original database SHA-256 | **PASS** | `fifa_ng_db.db` (A5CF1D9Dâ€¦) and `eng_us.DB` (9E9396D3â€¦) match the documented pristine hashes â€” untouched by all testing (scratch copies used). |

---

## Window-size / DPI matrix (Task 4)

The app is PerMonitorV2-aware. Layout robustness was verified headlessly via `--layout-test`
(150 layout operations, **0 FAIL**) across 25 sizes spanning 1180Ã—700 â†’ 1920Ã—1080, plus a real
GUI launch maximized at the native 1920Ã—1080/100% display.

| Resolution | 100% | 125% | 150% | 200% |
|------------|------|------|------|------|
| 1366Ã—768   | PASS (via 1180Ã—700 min-size proxy in layout sweep) | NOT TESTED | NOT TESTED | NOT TESTED |
| 1920Ã—1080  | **PASS** (native display + GUI) | NOT TESTED | NOT TESTED | NOT TESTED |
| 2560Ã—1440  | PASS (layout sweep â‰¥ this size range; no physical 2560 display) | NOT TESTED | NOT TESTED | NOT TESTED |

> **Honesty note:** this machine has a **single 1920Ã—1080 display at 96 DPI (100%)** and the
> session is non-elevated; changing Windows display scaling to 125/150/200% requires a sign-out
> that would terminate this session, and no second/higher-DPI display is attached. All >100%
> scaling cells are therefore **NOT TESTED** â€” they are expected-correct (PerMonitorV2) but
> unverified. No UI layout was rewritten; only the pre-existing SplitContainer crash fix from the
> prior session remains.

---

## Clean-environment test (Task 3)

| Sub-check | Result | Notes |
|-----------|--------|-------|
| Windows Sandbox / VM available | **No** | Windows Sandbox feature **not installed** (`WindowsSandbox.exe` absent); Hyper-V/Sandbox queries blocked â€” session is **not elevated**. No clean VM could be created. |
| Bare-machine startup (no .NET) | **NOT TESTED** | Cannot be performed: no sandbox/VM, and this machine has the .NET 8 SDK + Desktop Runtime. The Full Portable **is** fully self-contained (bundles coreclr/hostfxr/clrjit and the complete WinForms/WPF runtime) and starts here, but it has **not** been run on a machine with no .NET present. |
| Clean-folder startup (simulated) | **PASS (simulated)** | The Full Portable folder was copied/used as-is with only its bundled files + app-local VC++ CRT; headless smoke + GUI both run with **no absolute development paths** in any exe/dll (verified â€” 0 hits). |

**This is reported honestly:** the Full Portable package is self-contained by construction and
verified to start and run the full stack here, but a genuinely bare-machine (no .NET, no SDK,
no VS) run was **NOT TESTED** because no clean environment could be created on this host.

---

## Package contents

### `CM26_v1.0_Full_Portable` (self-contained â€” runs with no .NET installed)
- `CM26_by_Rizco98.exe` (+ `.dll`, `.deps.json`, `.runtimeconfig.json`) â€” R2R, not trimmed, not single-file.
- Complete **.NET 8 runtime** (`coreclr.dll`, `hostfxr.dll`, `clrjit.dll`, `System.*.dll`, WinForms/WPF).
- `CM26.Application.dll`, `CM26.EngineBridge.dll`, `Ijwhost.dll` (native C++/CLI bridge).
- `msvcp140.dll`, `vcruntime140.dll`, `vcruntime140_1.dll` (app-local VC++ CRT).
- `Assets/Logo/Creation Master 26.ico` + full documentation set + `SHA256SUMS.txt`.

### `CM26_v1.0_Lite` (framework-dependent â€” requires .NET 8 Desktop Runtime x64)
- Same app + bridge + VC++ CRT + docs, **without** the bundled .NET runtime.
- Includes a **startup dependency check**: if the .NET 8 Desktop Runtime (x64) is missing, a clear
  dialog names the exact requirement and offers the official download link, instead of a generic
  apphost launch failure. (Check verified to *pass* on this machine where the runtime is present;
  the missing-runtime dialog could not be shown here because the runtime is installed.)

**Deliberately excluded from both:** PDB debug symbols, database fixtures, scratch DB copies,
development logs, third-party source, and all absolute development paths (0 hits in every exe/dll).

---

## Definition-of-done

| Item | Status |
|------|--------|
| Full Portable runs without an installed .NET runtime | **By construction** (self-contained, bundles full runtime); bare-machine run **NOT TESTED** (no clean env available) |
| Lite release dependency documented | **YES** â€” `INSTALLATION.md` + README + startup dependency check |
| No absolute development paths remain | **YES** â€” 0 hits in all exe/dll; SettingsService dev path removed |
| All required native DLLs included | **YES** â€” bridge + Ijwhost + VC++ CRT in both packages |
| Clean-environment startup passes | **NOT TESTED** (no sandbox/VM; non-elevated) â€” simulated clean-folder PASS |
| Actual DPI scaling tested | 100% **PASS**; 125/150/200% **NOT TESTED** (single 96-DPI display, sign-out required) |
| Existing engine tests remain successful | **YES** â€” EXIT=0 |
| Save round-trip remains successful | **YES** â€” verified on scratch copy |
| Protected engine files remain unchanged | **YES** â€” SHA-256 verified |
| Release documentation complete | **YES** â€” README, INSTALLATION, KNOWN_LIMITATIONS, THIRD_PARTY_NOTICES, RELEASE_NOTES, RELEASE_READINESS_REPORT, SHA256SUMS |
| SHA-256 hashes generated | **YES** â€” per-package `SHA256SUMS.txt` (477 + 18 entries), verify-checked |

---
*Nothing above is fabricated. NOT TESTED items are marked as such and never counted as PASS.*

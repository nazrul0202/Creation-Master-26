# Release Notes — Creation Master 26

## Version 1.0.27 - UI polish and workflow (2026-08-05)

This release is a broad polish pass across the interface, navigation and editor
workflow. No database or save behaviour changed.

- **Dark theme completed.** Tab headers and ComboBox dropdowns are now owner-drawn
  with the CM26 dark palette, so the whole window is consistent (previously the
  native tab strip and dropdown list used light system colours). The window
  chrome and scrollbars also request immersive dark mode where the OS supports it.
- **New light theme.** A "Dark theme" toggle in Settings switches the whole
  interface between dark and light palettes; the choice is remembered and every
  open section re-themes instantly.
- **Module navigation now shows labels.** Each toolbar module shows its icon and
  name instead of an icon-only button, and sections without a dedicated icon get
  a distinct colour-coded letter badge instead of reusing another module's icon.
- **Quick keyboard navigation.** `Ctrl+1..0` jump straight to Dashboard, Countries,
  Leagues, Teams, Players, Managers, Stadiums, Kits, Competitions and Data Sync.
  A "Keyboard Shortcuts…" dialog under Help lists every shortcut.
- **Undo/Redo.** A Redo button (`Ctrl+Y`) is added beside Undo, and multi-level
  undo/redo works through the toolbar.
- **Unsaved-change safety.** Switching sections now warns that staged changes are
  still pending (they are never lost), instead of only warning at app close.
- **Start screen recent files.** The welcome screen lists recently opened
  database folders for one-click re-open.
- **Editor polish.** Fields and buttons show an accent focus border for keyboard
  navigation; invalid field values now surface in a non-blocking toast instead of
  a modal dialog; Dashboard stat cards are clickable shortcuts; the status-bar
  path is truncated to the last few folders.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.27.

## Version 1.0.26 - critical fix: launch/UI crash 0xc0000005 (2026-08-05)

**This release fixes a crash that terminated the app in Windows Error Reporting.**

- **Fixed: the app could crash with "Exception Processing Message 0xc0000005 -
  Unexpected parameters".** ListView column headers were themed by sending native
  HDM_SETTEXTCOLOR / HDM_SETBKCOLOR messages to the ListView header control. The
  fault happened inside the native header window procedure — during control
  creation in some window themes and layouts — where .NET exception handling
  cannot intercept it, so the process died with an access violation and no crash
  dialog. Header colour theming is removed; the list body still uses the dark
  palette. A regression test now creates themed ListViews through the exact
  handle-creation path and fails the build if the crash ever returns.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.26.

## Version 1.0.25 - release integrity and portability (2026-08-05)

This release fixes issues found in a full pre-distribution audit. Most are
invisible in normal use, but two of them affected every user.

- **Fixed: the tool looked for the scraper on the developer's own drive.** Data
  Sync probed a hardcoded `D:\FC26 FILE TOOL\...` path that only existed on the
  author's PC. Detection is now fully portable: the folder you set in Settings,
  a `CM26 Scraper` folder next to CM26, or a drive-root copy. Pointing Settings
  directly at `CM26 Scraper.exe` now works too, instead of silently finding
  nothing.
- **Fixed: a disconnected network drive could freeze the app.** Tool and asset
  detection enumerated *every* drive, so a dead mapped drive stalled Data Sync,
  the 3D viewer search and asset-pack detection for the full network timeout.
  Only local fixed and removable drives are scanned now.
- **Fixed: a malformed folder path could crash tool detection.** A pasted path
  containing invalid characters threw instead of being rejected.
- **Removed the bundled CM26 Scraper.** Its data set contained game database
  content, which contradicted this project's own licence promise not to
  redistribute EA content. The scraper is now an **optional separate download**;
  Data Sync explains how to add it and works exactly as before once you point
  CM26 at your copy. Everything else is unaffected, and the Lite package is
  substantially smaller. The Transfermarkt preview still needs no scraper.
- **Fixed: the 3D viewer could ship a stale or missing component.** `CM26.MeshKit`
  was not part of the solution, so a Release build produced it into a Debug
  folder while packaging copied an older Release copy. It is now built and
  packaged from one place, and a publish **fails** instead of quietly producing a
  package with an empty `Tools` folder.
- **Fixed: diagnostic output was invisible.** The `--smoke`, `--nav-test` and
  other command-line diagnostics printed nothing in a terminal because the app is
  a windowed executable. They now attach to the calling console.
- **Fixed: the licence agreement showed the wrong version** (1.0.23 in the 1.0.24
  release) and its no-game-content statement is now precise and verifiable.
- **The build now works on any Visual Studio edition.** All six build scripts
  hardcoded a *VS2022 Enterprise* path, so nobody with Community, Build Tools or a
  non-default install could compile the project. They now locate the toolchain
  with `vswhere`, and report a clear message when the C++ workload is missing.
- **Reproducible, self-verifying release process.** One version number drives every
  assembly (`Directory.Build.props`); packaging now *fails* rather than warns on a
  missing file, a stale build, a leftover debug symbol, a version mismatch, or any
  game content in the package — and produces the zips and SHA-256 checksums itself.
- **Added automated tests and CI.** A new `--release-selftest` mode runs 7 checks
  that need no game installation, and a GitHub Actions workflow builds the project,
  runs the native engine test and the self-test, and blocks game content from ever
  being committed. Previously nothing was verified automatically.
- **Added `docs/BUILDING.md`** documenting prerequisites, build order, every test
  and the full release procedure, so the project can actually be built by someone
  else.

## Version 1.0.24 - processing exception fixes across every editor section (2026-08-04)

- **Fixed: "3D Face Viewer" button could crash the app.** The player face-viewer
  workflow ran its mesh export and folder search on a background task from an
  async event handler; any asset/archive error escaped as an unobserved
  exception and closed the program. It now surfaces errors in a dialog and
  ignores the result if you have navigated away.
- **Fixed: file-processing errors crashed silently.** Exporting a Transfermarkt
  squad to a locked path, scanning a scraper output folder that is not readable,
  and exporting a NewWave bank to an unwritable destination all throw
  unguarded; each now shows a clear error instead of failing.
- **Fixed: database/asset errors while showing records.** Synchronous crest
  extraction, sponsor/adboard columns missing from a variant schema, a missing
  `audionation` table, and header/preview field providers that fail on an
  unexpected database now report a friendly message instead of throwing.
- **Fixed: stale preview threads could crash after navigating away.** When a
  background texture decode completed after you changed records or closed a
  section, several preview callbacks updated controls that were already
  disposed. Callbacks across Players, Countries, Teams, Leagues, Sponsors,
  Adboards, Managers, Stadiums, Balls, Boots and Gloves now guard against a
  disposed section and safely discard the orphaned image.
- **Hardened staging.** A single engine `Stage` failure is caught and shown as
  a validation message instead of propagating through the editor.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.24.

## Version 1.0.23 - commercial readiness: EULA, localization, updates (2026-08-04)

- **Commercial license.** Replaced the MIT open-source licence with a proprietary
  **End User License Agreement** (`LICENSE` + `EULA.md`). A first-run EULA dialog
  must be accepted before the app opens; acceptance is remembered.
- **Check for Updates** under the **Help** menu reads a version manifest and
  reports a newer public release. Throttled to once per day; fails gracefully
  when offline.
- **Localization framework.** Added a culture-based resource system
  (`Resources\Strings.resx`). Ships with English and a Spanish example set; new
  languages are added by dropping a `Strings.{culture}.resx` file. The saved
  language in Settings is applied at startup.
- **Improved About dialog** with clear version and unofficial-tool notice.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.23.

## Version 1.0.21 - finish dark theme for remaining white controls (2026-08-04)
- **Fixed: white text boxes before a record is selected.** Editors in the
  classic/entity and FC26 extension forms (Managers, Stadiums, Competitions,
  Balls, Boots, Gloves, Sponsors, Adboards, Audio, Scoreboard, Referees,
  Countries, Leagues, Teams, Players) are now themed dark at creation time, so
  they no longer flash white until the first record is opened.
- **Fixed: white record-browser columns.** The left record list added its
  columns after the grid was themed, so the columns kept the light default
  header/cell colours. Columns are now themed with the dark palette.
- **Fixed: player summary value box.** The Players overview used an explicit
  `Color.White` back colour; it now uses the dark input palette.
- **Fixed: light ListView column headers.** Details-view lists (Team Roster and
  Bench, Sponsors, Adboards, Audio banks/datasets, Data Sync squad) now render
  dark column headers instead of the system light header.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.21.

## Version 1.0.20 - full dark theme across every section (2026-08-04)

- **Fixed: dark theme reached every control.** `Theme.ApplyControlTree` now styles
  `CheckBox`, `RadioButton`, `NumericUpDown`, `TrackBar`, `RichTextBox`, `TreeView`
  and modal dialogs, so no black-on-dark or light-box controls remain in any section.
- **Fixed: white data grids.** Database Browser, Compdata and every `DataGridView`
  now render dark headers and cells. Column-level palette is reapplied after
  columns are generated so the whole column box matches the theme.
- **Fixed: white text boxes.** All entity editors (Countries, Leagues, Teams,
  Players, Managers, stadiums, kits, competitions) now use the dark input palette
  on every record selection instead of flashing back to white/`SystemColors.Control`.
- **Fixed: light panels, group boxes and canvases.** Every section's fixed-layout
  canvas, group box and tab page now uses the dark background/panel colours, with
  labels rendered transparent so they inherit the dark surface.
- **Fixed: modal dialogs.** Team audio/transfer/loan dialogs, Compdata dialogs and
  the entity creation dialog are now themed end-to-end.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.20.

## Version 1.0.19 - release consistency and build reliability (2026-08-03)

- The About dialog and Settings page now read the installed assembly version, so
  they cannot drift from the executable metadata in a future release.
- Public documentation and package assembly paths are synchronized to 1.0.19.
- Native smoke-test builds now use isolated object directories and can run in
  parallel without competing for `database_engine.obj`.
- **Fixed: Formation preview layout.** The pitch now resizes with its section,
  keeps all player markers inside the visible field, reports the number of mapped
  slots, and safely handles invalid position or coordinate values.
- Added a Formation regression probe covering every stored Formation row and all
  11 coordinate pairs before release packaging.
- **Fixed: Data Sync blank page.** The scraper workflow no longer receives the
  generic "Select a record" empty state and now prioritises the detected
  `D:\FC26 FILE TOOL\CM26 SCRAPER` installation.
- **Improved: Team roster formation.** Starting-XI cards are more compact,
  player names are shortened earlier, and collision resolution searches the
  full pitch before using a least-overlap fallback.
- **Improved: Countries, Leagues and Managers.** The country setup filter no
  longer overlaps its guidance text, League Settings uses the lower-right
  workspace, and the manager team field no longer overlaps portrait actions.
- **Clarified: Audio and Broadcast Links.** NewWave remains metadata/raw-bank
  export with local-file playback only; encoded FC26 audio injection is not
  advertised. The former Scoreboard page is labelled Broadcast Links because
  the verified FC26 table maps presentation IDs and has no proven overlay path.
- **Improved: first Open Game backup.** CmModData still retains the complete
  immutable Data/Patch restore snapshot, but CM26 now copies and hashes each
  file in one sequential pass with byte-level progress. Regular Open Game uses
  a fast inventory check; full SHA-256 verification remains part of backup
  audit and runs before Restore Original Data writes any game files.

## Version 1.0.18 — CM26 Scraper bundled, team/squad saving repaired (2026-08-02)

- The **CM26 Scraper** now ships inside the package under `Tools\CM26 Scraper\`.
  Transfers > Data Sync finds it automatically: the bundled copy, a copy next
  to CM26, a drive-root or `FC26 FILE TOOL` copy, or a folder set in Settings.
- **Data Sync** refreshes and previews the newest squad output automatically
  when the scraper closes, so scrape → import is a single flow.
- New **Set folder...** button on the Data Sync page and a **CM26 Scraper
  folder** row in Settings point CM26 at an existing scraper installation.
- **Fixed: creating a team no longer fails with "Integer value required".**
  The auto-generated squad staged position labels ("GK", "RB", "CB"…) into
  integer position columns; positions are now staged as valid integer codes.
- **Fixed: a new squad could not be saved.** Every generated team-player link
  reused the template row's `artificialkey`, so the save integrity check
  rejected them all as duplicate keys. Each link now gets a unique key.
- **Fixed: unrelated database quirks no longer block saving.** Saving a database
  was refused if any untouched row of a structurally edited table contained a
  pre-existing dangling reference or duplicate key (for example, an old team
  still pointing at a retired player id). Integrity validation now checks only
  the rows and cells changed in the current session.
- **Fixed: new player/team ids could collide with existing rows.** The id picker
  used a stale row count after insertions and could hand out an id that already
  existed in the shifted tail of the table.
- CmModData backup manifests now store a **SHA-256 hash** for every backed-up
  file, so snapshots can be verified against silent corruption. Existing
  manifests are upgraded in place.
- Removed an unused EA-sourced test fixture (`tests/CM26_LegacyWriter_Smoke.fifamod`).
- New **squad probe** regression test runs the real create-team + 23-player
  pipeline on a database copy, saves it through the native engine and reloads
  the written files to verify they persist.

## Version 1.0.17 — national team navigation, squad auto-fill and toolbar cleanup (2026-08-02)

- Creating a national team or a league team now writes a full placeholder squad automatically: 23 players named **Player 1 … Player 23** with jersey numbers 1–23, positions and a starting line-up, linked through `teamplayerlinks`. The new team opens in the Teams section immediately so you can just rename the rows and press Save.
- **Countries** now has an **Open National Team** button that navigates to the linked national team in the Teams section.
- Fixed **Open Team** in the League roster not working. The right-click command and double-click previously failed because the list item carried a team-link object instead of a plain Team ID, and the hidden record browser threw an exception while scrolling to the selection.
- Team crests now prefer the game's **dark** crest variant, falling back to light when no dark crest exists.
- The Tactical Board now shows each player's **miniface** on the pitch card (loaded from the installed FC26 game or the local asset pack).
- Removed **Game Graphics** and **News Stories** sections from the toolbar.
- **TV Broadcast** is now **Scoreboard** (same data, clearer name).
- FC26 Career Compdata is read straight from the game's Frostbite chunk-file collectors when no loose `compdata` folder exists.
- Fixed asset searches with no type filter.
- Re-added the **Database Browser** (Tools > Database Browser) for inspecting every table and editing the fields the validated writer supports; stub/host-only sections are no longer shown.
- Removed the no-op **Randomize** player button, the **Replicate** team button and the disabled Player Overall Tuning controls.
- Removed duplicate placeholder tabs (Confederation, Nation, Trophy, Stage, Group) from the Competitions section, which now shows reliable **FIFA** and **Compdata** tabs only.
- Full release audit passed: 23 navigable modules, all navigation, layout and smoke tests green.
- Replaced the never-filling **3D Model** image panels on Stadium, Referee, Kit, Ball and Boot pages with a short note and the **Open 3D Model Viewer** button, so the UI no longer shows a large empty preview.
- The 3D viewer buttons now **auto-detect** an already-extracted FBX matching the selected record (by record id/name) in the FC26 FILE TOOL and asset-pack export folders, opening it directly instead of asking for the file. The file picker is still the fallback.
- Fixed the About box and Settings footer showing the stale version number.

## Version 1.0.16 — creation indexing and local scraper workflow (2026-08-01)

- New countries, leagues, teams and players now refresh the in-app database indexes immediately. Newly created records can be selected and found without restarting CM26.
- Countries created in the current session are visible through **Show countries awaiting setup**. A country becomes Career-playable only after a domestic league, clubs and Compdata have been configured, then a new Career is started.
- Creating a team now writes its league/country relationship, refreshes the league display and opens the new team after confirmation.
- Player creation no longer fails when the current database has no editable player-name template. The new player is searchable in CM26 for the current session and is created as a free agent; CM26 does not fabricate unsafe localization rows.
- **Data Sync** now opens the local CM26 Scraper, detects the newest squad output, previews it, and imports it directly to a selected team with generated player IDs and team-player links. The workflow no longer requires manually choosing an Excel file from the Team screen.

## Version 1.0.15 — workflow and stability repair (2026-08-01)

- Fixed the UI-thread `NullReferenceException` raised while opening classic editor sections. Record-search hints no longer access uninitialized section state.
- Moved **Create National Team** from Teams to Countries. It uses the selected country directly and creates the country-team link with an automatic Team ID.
- The League roster now has a right-click menu: **Add New Team**, **Add Existing Team**, **Open Team**, and **Remove from League**. It keeps team creation where the league is being edited.
- Formation cards now display player name, position and OVR. Larger cards, stronger spacing and collision padding prevent adjacent formation coordinates from obscuring one another.

## Version 1.0.14 — creation and lineup usability repair (2026-08-01)

- Added a consistent **Find** field in the green command strip of every editor; the Team page now has a clear **Search teams** control beside its record chooser.
- Fixed cross-section record creation selecting an unrelated template row. New teams created from a League now remain linked to that League and open their own Team record for editing.
- Player creation no longer fails merely because an installed database has no `editedplayernames` template row. It creates the safe Player record and reports truthfully when the database cannot store a custom editable display name.
- Rebuilt the lineup cards to show a readable player name and a guaranteed position label, with an explicit fallback to the formation role.
- Replaced the plain Reserved Lineup list with a structured Reserve Squad table (number, player, position) and double-click navigation to the Player page.
- Corrected the Country flag asset commands so they no longer overlap the image captions or flag previews.

## Version 1.0.13 — CM16 navigation and asset workflow (2026-08-01)

- Double-click a club in a league to open its Team record; double-click a roster player to open the Player record.
- The tactical board is now a compact, text-first pitch. Player cards no longer carry minifaces and adjacent formation slots are separated before rendering.
- Player Face now exposes direct legacy face-texture import, remove and export beside the 3D viewer.
- Legacy asset controls use explicit **Export** instead of an ambiguous external-view action.
- Removed the Country page workbook-based Career Compdata command: it did not write Compdata into FC26 Data/Patch and did not meet the direct-editor contract.

## Version 1.0.11 — focused country list (2026-08-01)

- Countries now opens on the Career league-country list instead of all database
  nations. Use **Show all database countries** only when editing or creating a
  country that is not yet configured with a domestic league and linked clubs.
- The duplicate-country message now distinguishes a database record from a
  Career-playable country configuration.

## Version 1.0.10 — league team workflow (2026-08-01)

- League editor now includes **Search Team**, **Find**, and **Add New Team**.
  A new team receives an automatic Team ID and is linked to the selected league
  immediately.

## Version 1.0.9 — team access and duplicate-country protection (2026-08-01)

- Added a second, direct **Add New Team** command below the team crest so the
  creation action is visible without scrolling or searching through the toolbar.
- **Add Country to Game** now detects an existing country by name or ISO code
  before writing data, and reports the existing Country ID instead of failing
  with a fixed-field-capacity error.

## Version 1.0.8 — scraper squad import (2026-08-01)

- Added **Import Scraper Squad** to Teams. It imports a confirmed CM26 Scraper
  workbook into the selected team, allocates player IDs automatically and maps
  names, country, position, preferred foot and dates into FC26-safe values.

## Version 1.0.7 — country-linked league and team search (2026-08-01)

- New leagues now require a valid Country ID instead of silently inheriting an
  unrelated placeholder country such as Rest of World.
- Added **Search**, **Add New Team**, and a clearly placed national-team action
  to the Team editor.

## Version 1.0.6 — visible creation actions (2026-08-01)

- Added **Add New League** beside **Show Team Logo** in the League team toolbar.
- Added **Add Country to Game** in the Country panel. Both actions use the
  safe automatic-ID record creation workflow.

## Version 1.0.5 — Compdata TXT export (2026-08-01)

- Added **Export TXT** to Compdata. It writes one UTF-8, comma-separated,
  headerless `.txt` file per worksheet and preserves blank cells as a single
  space, matching the supplied STOP converter workflow.

## Version 1.0.4 — national team creation (2026-08-01)

- Added **Create National Team** in Teams. It assigns a new Team ID and creates
  the corresponding country link in `teamnationlinks` in the same staged action.

## Version 1.0.3 — Compdata builder update (2026-08-01)

- Added a League/Cup Compdata builder: it creates a competition object, stage
  and group hierarchy, standings rows and an editable schedule skeleton with
  automatically allocated unused object IDs.
- Added direct promotion/relegation (advancement) links with type-safe group
  validation, and applied the documented worksheet capacities before adding data.
- Added a save/reopen regression test for the builder against the supplied
  Compdata workbook.

## Version 1.0.2 — Compdata integrity update (2026-08-01)

- Corrected Compdata workbook handling: supplied workbooks store raw rows and
  do not have a header row. The editor now uses the documented schema instead
  of turning the first competition record into column headers.
- Added explicit validation for competition object IDs, parent links and the
  linked standings, schedule, advancement and initial-team references.
- New automatic IDs are intentionally limited to `compobj`; dependent sheets
  now identify themselves as linked data so an unrelated ID cannot be created.

## Version 1.0.1 — public UI and workflow hardening (2026-08-01)

- Removed technical game/source labels from editor panels and simplified the
  application title and menus.
- Rebuilt roster and formation presentation, removed the duplicate miniface
  panel, and renamed the bench to Reserved Lineup.
- Corrected encoded joining/birth dates and the four real boot fields.
- Fixed the overlapping Team Audio catalogs and added local audio playback.
- Added safe New Country/League/Team/Player workflows with automatic unused IDs.
- Added an integrated Compdata workbook editor; verified 10 worksheets and
  24,415 rows through a save/reopen round-trip.
- Reused the first archive index during Open, added phase/backup progress, and
  kept loading off the UI thread.
- Added optional transparent NTFS compression for the immutable CmModData backup.
- Navigation passed 24/24 sections and layout passed 600/600 size operations.

## Official Release Version 1.00 — direct live editor (2026-07-30)

- First Open FC26 creates and inventories the complete original
  `<FC26>\CmModData\Data` and `<FC26>\CmModData\Patch` backup.
- Save writes validated database and mapped legacy changes directly to the
  installed FC26 `Data`/`Patch`; a reversible live round-trip verified that a
  saved value is visible after re-extraction and that the original can be
  written back.
- Restore replaces the live Data/Patch trees from the immutable CmModData copy.
- Public sections use CM16-style named mappings without raw/all-fields pages.
- Added a real FC26 Harmony/NewWave bank browser, dataset inspector and raw bank
  export, based on an independent implementation after reviewing FET's public
  format workflow.
- Added a backup inventory manifest, full backup audit, NewWave parser gate and
  packaged regression tests.

## v1.0.0-rc2.31 — live archive reload and CM16 audio mapping (2026-07-30)

- Save now targets the detected installation's `Data` and `Patch` in a single
  confirmation, then re-extracts the database from those live archives and
  reload-verifies the committed values.
- `CmModData` remains the immutable source used by **Restore Original FC26 Data**.
- Added CM16-style Player Callname, National Team Audio and Team Audio/catalog
  panels for commentary, custom team names, anthems and chant/goal-song slots.
- Removed raw/all-fields pages from the active public UI and added a regression
  check that fails navigation testing if either page returns.
- Added automatic non-overlapping formation cards with player minifaces and
  friendly labels for FC26-only audio/presentation controls.

## v1.0.0-rc2.29 — direct FC26 editing and full editor pass (2026-07-29)

See `Release/docs/RELEASE_NOTES.md` for the complete history. RC2.29 adds
direct transactional FC26 `Data`/`Patch` editing, immutable `CmModData`
backup/restore, complete record tabs, repaired legacy import/revert, roster
formation minifaces and transfers, Transfermarkt squad scraping and the
packaged 3D FBX viewer.

## v1.0.0 · Release Candidate 1  (2026-07-26)

## v1.0.0-rc2.11 - independent FC26 assets  (2026-07-28)

- Automatically detects and opens the installed FC26 asset source with the
  database.
- Independently indexes base Data and incremental Patch TOCs, with 1,695,922
  unique assets on the verified installation.
- Adds exact CAS extraction, game-provided Oodle decompression, texture
  RES/chunk reconstruction and automatic real jersey previews in Kits.
- Consolidates all kit colours, templates, badges, numbers and appearance
  fields into General.
- Shows full player names inside the Team formation pitch.
- Ships the Full Portable asset bridge as a separate self-contained process.
- Keeps asset replacement and playable mod writing disabled.

## v1.0.0-rc2 - Release Candidate 2  (2026-07-28)

- Removed the final CM16-source absolute-path references from the project; portable builds no longer require a development checkout.
- Team formation captain, corner, penalty and free-kick assignments are now editable roster-scoped player pickers rather than resolved read-only text.
- Restored the CM16-style Team name, stadium, manager and last-season layout while mapping each display slot to the real FC26 canonical data.
- Added dedicated CM16-style FC26 extension modules for Sponsors, Gloves, Nation Audio, Stadium Audio, TV Broadcast, Adboards, Game Graphics and News Stories.
- Full Portable navigation smoke test now passes for all 25 modules against the supplied FC26 database.
- League Teams now use the original CM16-style crest-and-name grid, with a text-list toggle for dense browsing.
- Team Roster now has a CM16-style interactive pitch with 4-3-3, 4-4-2, 4-2-3-1 and 3-5-2 views; each player slot displays its expected position beneath the player name.
- Release identity advanced from RC1 to RC2.

### Verification boundary

RC2 does not claim asset-package import/export, 3D asset support, structural undo, or a real-game FC26 save test. These require a verified FC26 archive writer and an installed game test target, neither of which is available in this workspace. Existing asset previews remain read-only and explicitly labelled.

## v1.0.0 - Release Candidate 1  (2026-07-26)

First public beta candidate. Ships as two packages for Windows x64:

- **Full Portable** (`CM26_v1.0_Full_Portable`) — self-contained; runs with no .NET installed.
- **Lite** (`CM26_v1.0_Lite`) — framework-dependent; requires the .NET 8 Desktop Runtime x64.

### What it is

A validated, dark-themed desktop editor for the EA SPORTS FC 26 database (`fifa_ng_db`),
in the spirit of Creation Master 16. Records are edited in memory, validated, and written
only through the protected native FC26 engine with automatic backups and reload-verification.

### Highlights

- **17 editor sections** over the real FC26 database: Dashboard, Countries, Leagues, Teams,
  Players, Managers, Stadiums, Kits, Competitions, Formations, Transfers, Balls, Boots,
  Referees, Database Browser, Diagnostics, Settings.
- **Validated, undoable editing** with pending-change tracking and a confirm-before-save flow.
- **Safe save model** — a validated copy is written (CRCs recomputed, locale re-encrypted),
  originals are backed up, and the result is reload-verified.
- **Read-only visual asset previews** (minifaces, balls, stadiums, boots, GK gloves, flags)
  when the real local image exists; missing assets show an honest labelled state — never a fake.
- **Per-Monitor-V2 DPI aware**; verified at 100% scaling (higher scaling — see report).
- **Full Portable** runs on a clean PC with no .NET / SDK / Visual Studio installed.

### Known limitations

See `KNOWN_LIMITATIONS.md`. Player names use a labelled fallback (EA runtime locale cipher key
absent) and name editing is disabled; release-to-free-agent is disabled; crest/minikit/kit-render/
3D-face assets are unavailable from the local asset set; asset preview is read-only with no
import/write.

### Verification

Full detail in `RELEASE_READINESS_REPORT.md`. Engine smoke test passes (279 tables,
360,298 rows); save round-trip verified on a scratch copy with the original database left
byte-for-byte unchanged (SHA-256 verified); 17/17 sections navigate. Per-file integrity hashes
are in `SHA256SUMS.txt`.

---
*Community tool by Rizco98 — not affiliated with or endorsed by EA SPORTS.*

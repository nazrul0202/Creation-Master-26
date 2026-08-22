# Release Notes — Creation Master 26

## Version 1.0.136 - responsive formation loading and corrected pitch spacing (2026-08-22)

- Moved both Frostbite asset extraction and miniface decoding off the WinForms UI thread so switching teams no longer freezes the formation screen.
- Loads the visible Starting XI first, then fills substitutes and reserves progressively; an in-memory cross-team cache makes revisiting players immediate.
- Reserved separate visual lanes for goalkeepers and defenders, fixing the CB/GK overlap without changing the formation coordinates stored in the FC26 database.
- Projects horizontal player positions through the perspective touchlines instead of treating the pitch as a flat rectangle.
- Rebuilt the pitch geometry from the supplied FC26 reference: perspective touchlines and boxes, the higher halfway line, and proportionally matched centre circle are now code-native and responsive.
- Reuses the rendered pitch background while its size is unchanged, removing unnecessary bitmap recreation during normal roster refreshes.

## Version 1.0.135 - clearer owner-drawn formation cards (2026-08-22)

- Removed the inherited CM16 label text underneath each FC26 player card, fixing the duplicate player name shown for a single squad member.
- Made player cards fully opaque and increased miniface, overall-rating and player-name sizes for clearer reading.
- Increased pitch and roster-section spacing so Starting XI, substitutes and reserves remain distinct at the supported window sizes.
- Reworked the formation surface around the supplied black FC26 pitch reference, including a dark gradient, perspective touchlines, centre markings and perspective penalty boxes.
- Added a real-database smoke assertion that fails if any owner-drawn FC26 formation card retains legacy label text.

## Version 1.0.134 - Starting XI role-link and formation-board hotfix (2026-08-22)

- Fixed a legacy role-link defect that converted every FC26 formation position back to goalkeeper, leaving only the goalkeeper card on the pitch and moving the other ten starters into reserves.
- Preserved each role ID when creating or replacing a `PlayingRole`, so the database-native Starting XI survives linking, rendering and formation changes.
- Enlarged the FC26 roster formation board into the available right-side space and made its width responsive to the window.
- Increased pitch height and arranged starter cards from the formation's real X/Y offsets; substitutes and reserves now use readable seven-column grids.
- Moved set-piece assignments and tactic controls with the responsive board so minifaces, names and overall ratings remain legible without covering adjacent controls.
- Extended the installed-database smoke test to require exactly 11 unique linked roles and 11 Starting XI players, preventing this regression from returning.

## Version 1.0.133 - in-game formation design and persistent layout editing (2026-08-22)

- Rebuilt the FC26 roster formation view with an in-game-inspired dark pitch and compact player cards showing each starter, substitute and reserve miniface, name and overall rating.
- Added rating colours, selected-player highlighting and status bars while preserving the existing player selection and drag/drop squad workflow.
- Exposed the complete 29-layout FC26 formation selector directly on the Roster page; the formation-name link remains available for advanced role and position editing.
- Formation selection now copies the chosen database-native layout into the team's own formation record instead of replacing it with a generic shared object.
- Added explicit FC26 save-plan support for all 11 formation roles, offsets and player instructions so formation edits persist through database save and reload.
- Kept the formation background code-native and responsive; no EA UI texture is redistributed.

## Version 1.0.132 - FC26 database deep scan: club profile, formations and player data (2026-08-22)

- Deep-scanned the installed FC26 database and documented the boundary between static squads data and runtime Career-save data. Club Worth, fanbase devotion, youth facilities, financial stability, founding year and honours are backed by real `teams` columns; transfer budget, generated board objectives, energy, sharpness and morale are not fabricated.
- Added static club-profile presentation to Team Overview, including FC26 rating bands, correctly scaled Club Worth and league/cup/Champions League honours.
- Stopped zero objective placeholders from appearing as the misleading **Win League Title** in the CM16-style shell. The controls now identify Career-generated data and remain disabled when no real objective is stored.
- Added an authoritative 29-entry FC26 formation catalog. Formation selection now keys variants by `formationid`/`relativeformationid`, preserving Narrow, Wide, Flat, Holding, Defend and Attack layouts instead of collapsing them by their shared short name.
- Applied the same formation names to the legacy shell and retained every database-native layout, including `4-2-1-3`.
- Expanded Player Overview to display all five familiar-role slots and explains which live player values exist only in a Career save. Static attributes, PlayStyles and familiar-role IDs continue to come from `players`.
- Extended the read-only `--table-probe` diagnostic with optional field/value filtering and row limits for reproducible FC26 schema audits.
- Added regression tests for all 29 unique formations, variant resolution, club-profile rating bands and Club Worth scaling. The suite now passes 123 tests.

## Version 1.0.131 - CM16-shell FC26 team data hotfix (2026-08-22)

- Fixed the CM16-style Team Roster screen still showing **Formation not available** even though FC26's `formations.teamid` contained the team formation. FC26 formation ownership is now authoritative and is no longer discarded by the obsolete FIFA formation link.
- Replaced the legacy tactic controls in that shell with FC26 controls: Build Up Style (`Short Passing`, `Balanced`, `Counter`), Defensive Approach (`Deep`, `Balanced`, `High`, `Aggressive`) and exact 1–100 Line Height.
- Added opponent-context selection to Team Traits so `trait1vweak`, `trait1vequal` and `trait1vstrong` can be viewed and edited independently while preserving unknown high-order FC26 bits.
- Included `default_mentalities` and `defaultteamdata` in the legacy snapshot and mirrors tactic edits into their active FC26 records on save.
- Added headless regression checks against an installed FC26 snapshot for Heidenheim's 5-4-1 formation, rendered tactics and trait masks, plus an exact six-field cross-table write plan.

## Version 1.0.130 - FC26 tactics, traits and F3D viewer integration (2026-08-22)

- Added an FC26 Tactics & Traits team card backed by the installed database: formation, build-up style, defensive approach, editable line height and opponent-context trait masks are now visible together.
- Mapped the verified FC26 build-up values (`Short Passing`, `Balanced`, `Counter`) and defensive presets (`Deep`, `Balanced`, `High`, `Aggressive`) while retaining exact 1–100 line-height editing.
- Mirrors build-up and line-height edits into the active `default_mentalities` row; defensive depth is also mirrored into `defaultteamdata`, keeping the related FC26 records consistent.
- Recovered team formations directly through `formations.teamid` when FC26's `teamformationteamstylelinks` table is empty, fixing the legacy roster screen's “Formation not available” state.
- Added editors for `trait1vweak`, `trait1vequal` and `trait1vstrong`. The ten verified low-order flags are named, while all unknown higher FC26 bits are preserved on every edit.
- Fixed the legacy Team Traits panel to read and update the equal-opponent FC26 trait mask without destroying newer high-order trait flags.
- Added optional F3D desktop integration for exported FC26 FBX head meshes. CM26 discovers `f3d.exe` from `CM26_F3D_PATH`, `Tools\F3D`, standard install locations or `PATH`; the bundled CM26 viewer remains the fallback.
- Added regression coverage for FC26 tactic mappings, active-mentality selection, high-bit trait preservation and F3D discovery precedence.

## Version 1.0.129 - release integrity and honest club finances (2026-08-22)

- Corrected the v1.0.128 financial-field presentation: FC26's `teams.clubworth` is now labelled **Club Worth**, not Transfer Budget. A genuine `transferbudget` column is still detected and labelled Transfer Budget when a compatible schema provides it. Career transfer budgets remain separate career-save data and are not fabricated from club valuation.
- Added regression tests for financial-field selection, including transfer-budget preference, FC26 club-worth fallback and schemas with no supported financial field.
- Added a release-metadata validator covering `version.json`, `Directory.Build.props`, release notes, installation instructions, limitations and checksum filenames; CI and package assembly now fail when any document names an older version.
- Package assembly now runs the shipped `--release-selftest` from both Full Portable and Lite payloads before creating ZIPs, and validates the versions of both public executables.
- Removed unused Linux, macOS and win-x64 Assimp native payloads from the x86 legacy-shell folder. The required Windows x86 runtime remains included.
- Fixed packaged UI automation being routed into the normal legacy shell and waiting indefinitely. Smoke flags now always target WPF Studio, while ordinary user launches continue to open the CM16-style legacy shell.
- Fixed the embedded mesh preview referring to missing `CardFieldBg`/`CardBackBrush` resources; the Players section and other 3D-preview hosts now use defined classic-theme brushes.
- Propagated WPF shutdown failures through the host process exit code so CI and packaging cannot report a failed Studio smoke as exit 0.
- Added a fast `--ui-shell-smoke` release gate that renders every Studio section without scanning a game installation. The full `--ui-smoke` integration path still tests installed FC26 loading/reload, now reports phases and elapsed loading time, and writes failures to stderr as well as the crash log.
- Synchronized release documentation through v1.0.129 and added an explicit output-directory option so generated packages can be kept in the single approved `Release` location.

## Version 1.0.128 - per-team financial value release (2026-08-22)

- Packaged the per-team financial-field loading introduced in v1.0.127 in Full Portable and Lite builds.
- This release described the FC26 `clubworth` fallback as Transfer Budget. Version 1.0.129 corrects that terminology because club valuation is not career transfer budget.

## Version 1.0.127 - editable per-team financial field (2026-08-22)

- Added an editable numeric financial field to the Teams Overview hero card.
- Loaded the value from `teams.transferbudget` when present, otherwise from FC26 `teams.clubworth`, instead of showing one constructor default for every team.
- Staged edits immediately through the normal pending-change pipeline.

## Version 1.0.126 - embedded face viewer and legacy financial mapping (2026-08-22)

- Replaced the unsupported HelixToolkit FBX import path with direct AssimpNet import so installed FC26 head meshes render in the embedded 3D viewer.
- Populated the legacy team's financial property from `clubworth` when FC26 has no `transferbudget` column, avoiding the hardcoded 1,000,000 display.

## Version 1.0.125 - embedded CM16-shell face viewer (2026-08-22)

- Embedded the HelixViewport3D face-mesh panel in the CM16 legacy Player Face tab.
- Added AssimpNet-backed FBX loading with orbit and zoom controls while retaining the external viewer for standalone inspection.

## Version 1.0.124 - team budget fix, release audit cleanup and embedded 3D previews (2026-08-20)

- **Embedded in-app 3D previews (new).** Kit, player face, stadium, ball and boot records now render their real FC26 Frostbite mesh directly inside the editor section (WPF `Mesh3DPreviewPanel` hosted in both shells). The mesh is exported from the installed game archives to FBX, auto-loaded when the record is selected, and rendered with orbit/zoom controls via HelixToolkit + Assimp. The external `CM26.3DViewer` tool remains available for standalone FBX inspection.
- **Auto-load on record select.** The player face 3D preview loads automatically when a player is selected in the Face tab (no click needed); the kit 3D preview loads when a kit record is shown.
- **Kit texture mapping.** Kit 3D previews resolve the colour texture from the FC26 Frostbite kit archive (jersey colour DDS) and apply it to the mesh surface, so the jersey design is visible on the 3D model.
- **Fixed every team showing a 1,000,000 transfer budget** in the legacy CM16-style editor: FC26 removed `teams.transferbudget` from the schema entirely (career budgets now live in `career_managerpref`), so the legacy reader fell back to its constructor default. The Team load/save paths now map the legacy budget field onto `teams.clubworth` (the nearest per-team financial column FC26 actually has) for both read and write.
- The same fix **prevents a latent crash/corruption risk on save**: the legacy Team save wrote through the missing column index (-1), which throws IndexOutOfRange when the schema lacks `transferbudget`.
- Added a read-only `--mesh-search <gameRoot> <query>` headless probe that lists indexed FC26 MeshSet names (diagnostics for kit-mesh naming work).
- Release audit cleanup: removed the legacy hardcoded `D:\CM26 Mod Manager` fallback path so state always lives under `%LocalAppData%\Creation Master 26\ModManager`.
- Hardened previously silent `catch` blocks (CAS rollback, transaction folder cleanup, studio-crash.log fallback) with `Debug.WriteLine` diagnostics so recovery failures are never invisible.
- `LaunchWithDataPath` now verifies the overlay folder contains `Data` and `Patch` before launching FC26, preventing a confusing launch failure when the mod data folder has not been built.
- Synchronised INSTALLATION.md, KNOWN_LIMITATIONS.md and README version references and checksum filenames to 1.0.124.

## Version 1.0.123 - league logos, transfers, pickers and FBX face export (2026-08-19)

- League logo replacement now works in both shells: Import/Remove/Export buttons stage the same image into every FC26 league-logo family (`league/light` 256x256, `league512x128/light` 512x128, `leaguelogos_tiny/light` 200x64) through the shared `LeagueLogoCatalog`, and preview checks staged replacements before falling back to the installed asset.
- Player transfers: a Club dropdown on the Player section (WinForms + WPF) stages `teamplayerlinks.teamid`, so moving a player between clubs (or to Free Agent) saves through the same direct FC26 transaction as every other edit.
- Manager pickers: the Manager section now has Country and Playing-for dropdowns (WinForms + WPF) instead of raw ID text boxes, staging `manager.nationality` and `manager.teamid`.
- WPF League section now has a Country ComboBox picker (matching the WinForms league editor) instead of a raw country-ID text box.
- 3D face FBX export: added an "Export Face FBX…" button (WinForms) and an "Open 3D Face Viewer…" button plus Import/Remove/Export face-texture buttons (WPF) to the Player Face tab, exporting the real FC26 head mesh to a Blender/Assimp-compatible ASCII FBX file on disk.
- Pruned 49 stale internal audit/task documents from the repository to keep the public source clean.

## Version 1.0.122 - full edit coverage for names, stats and skills (2026-08-18)

- Player names can now be renamed to any length: added a CM16-style whole-blob rewriter for the Huffman-compressed `playernames` table (rebuilds the tree and string blob, re-points record offsets, shifts later tables and recomputes the full CRC chain). The native engine only supported in-place slot writes, which failed for almost every realistic rename.
- Player-name reads now flow through the same rewriter (the engine's compressed-string reader has a known decode defect for this table), so staged names display and reload-verify correctly in both shells.
- Verified end to end with a live FC26 plan covering player name, overall rating, potential, skill moves, acceleration, sprint speed, birth date, team name, league name and kit fields: all ten changes staged, saved and reload-verified.
- The rewriter refuses to touch a database whose existing CRC chain does not verify, and rejects characters outside the player-name alphabet instead of writing a corrupt file.

## Version 1.0.121 - legacy editor league save hotfix (2026-08-18)

- Fixed saving after editing a league (or any single record) in the CM16-style editor: the save plan incorrectly included up to dozens of untouched `playernames` entries because decoded name artifacts (replacement characters, control characters, internal `LookBook` rows) were compared against raw database text and staged as phantom edits, which then failed reload verification with `FC26 reload verification failed: playername[...]`.
- Player-name staging now compares against the name resolved at load time, so only names the user actually edited are written; verified headless against a real FC26 snapshot (45 phantom edits before, 0 after, genuine league edits unaffected).

## Version 1.0.120 - Studio direct Save (CM16 method) (2026-08-18)

- Wired the CM26.Studio (WPF) Save command to the same direct FC26 Data/Patch transaction engine the WinForms shell uses, so Save now commits edits to the live game instead of only the local session copy.
- Save follows the CM16 method end to end: staged edits are validated, the database is rebuilt with fresh layout and CRCs, written through the verified Frostbite transaction (signed TOCs, atomic commit, automatic rollback), and the editor reloads from the freshly written archives so the game runs with the edits applied.
- Guard rails match the WinForms flow: FC26 must be closed, the CmModData backup must be verified and the live baseline must match before any write is attempted.
- The locale database (`eng_us.db`) is only shipped when locale strings were actually edited, matching the established FMM-safe staging rule.
- Moved `LegacyAssetModService` into the shared `CM26.Application` layer so both shells stage replacements through one implementation.

## Version 1.0.119 - FC26 face and kit 3D FBX viewer update (2026-08-16)

- Added accent-insensitive player search so names such as Benjamin Šeško can be found with either accented or plain text.
- Corrected player face mesh selection to use the FC26 `headassetid`, with safe player-ID fallbacks for alternate database records.
- Added FC26 kit MeshSet resolution from the current team-kit template, shape, collar, fit and rendering-material fields.
- Added deterministic mesh candidate ranking so exact `head_*_mesh` and `jersey_*_mesh` assets are selected ahead of unrelated Frostbite matches.
- Verified real FC26 face and kit MeshSets through the bundled ASCII FBX exporter and packaged 3D viewer launch path.
- Retained the CM16-style UI and passed the full section navigation and layout regression audits before packaging.

## Version 1.0.117 - FC26 Frostbite format coverage and audit release (2026-08-16)

- Added exhaustive Frostbite payload auditing across every indexed FC26 asset and support for Zstandard-compressed archive blocks.
- Corrected FC26 RenderFormat-to-DXGI mapping and rebuilt exported textures with valid DDS DX10 headers.
- Added read-only preview decoding for all 20 texture formats present in the verified FC26 installation, including BC1-BC7, signed/unsigned BC4-BC6, sRGB and uncompressed float/UNORM formats.
- Hardened native FC26 player-name decoding and added regression coverage for malformed and alternate text records.
- Added real-install capability, codec and per-format texture gates; the audited installation completed with zero unavailable assets, parse failures or unsupported texture formats.
- Retained the CM16 section-switching regression gates and verified database extraction/reload, names, rosters, formations, navigation, layout, labels, kit texture preview and mesh export before packaging.

## Version 1.0.116 - CM16 section switching and FC26 names hotfix (2026-08-16)

- Fixed toolbar sections becoming a blue blank workspace after opening another section; stale Alt/Ctrl state no longer routes ordinary clicks into the collapsed auxiliary panels.
- Fixed CM16 MDI child forms remaining visible after reparenting, which caused section flicker and could cover the newly selected editor.
- Populated league short and long names from the FC26 league table instead of repeating the `Short League Name` / `Long League Name` placeholders.
- Snapshot export now resolves FC26 player names through the existing native database name decoder before the CM16 editor consumes them.
- Added a regression smoke path that drives real toolbar clicks and verifies every requested editor remains in the main CM16 panel.

## Version 1.0.113 - CM16 resource loading hotfix (2026-08-15)

- Fixed the crash after Open FC26 caused by CM16 images being resolved from the executable instead of the CM26.Studio resource assembly.
- Qualified all shell and PickUpControl image references so the toolbar and editor sections load their embedded resources reliably.
- Strengthened UI automation so an unhandled WPF exception fails the smoke test instead of being silently reported as a pass.

## Version 1.0.112 - Open FC26 reload hotfix (2026-08-15)

- Fixed File > Open FC26 becoming disabled after automatic startup loading.
- Open FC26 now remains available at all times and asynchronously reloads both the database and Frostbite asset index.
- Added a real UI smoke regression test that clicks the menu item, waits for the database/assets, and verifies the command is re-enabled.

## Version 1.0.111 - FC26 open flow and CM16 editor parity update (2026-08-15)

- Replaced remaining FIFA 16 / lan.db open labels with FC26 database and Data/Patch actions.
- Removed the modern dashboard from the post-load path; FC26 now opens directly into the CM16 editor workflow.
- Rebuilt Country and League as fixed CM16-style forms with dedicated information, team and logo/flag panels backed by the installed FC26 database and Frostbite asset paths.
- Restored full-width CM16 Player Info/Skills/Face and Kit workspaces by removing the extra generic master lists.
- Matched the original CM16 Microsoft Sans Serif 8.25pt font metrics throughout the classic shell.
- Retained automatic FC26 installation detection, Frostbite archive loading, staged editing and direct-save services.

## Version 1.0.110 - CM16 shell with FC26 Frostbite support (2026-08-15)

- Restored the Creation Master 16 menu, toolbar, full blue workspace, right/bottom tool strips and classic section layout as the primary `CM26_by_Rizco98.exe` interface.
- Added automatic FC26 installation detection, Frostbite archive indexing and database extraction/loading at startup.
- Ported CM16-style Player, Manager, Referee, Team and Stadium controls while retaining FC26-native database field editing.
- Team crest previews now load the installed FC26 Frostbite assets at CM16 sizes (256, 50, 32 and 16 pixels).
- Corrected Team field grouping and moved FC26-only columns into the extensions tab instead of mixing them into CM16 panels.
- Corrected Manager, Referee, Kit and Stadium pickup lists so they display resolved FC26 identities instead of generic record numbers.
- Added automated WPF visual auditing across all 17 visible sections using the installed FC26 database.

## Version 1.0.109 - Steam-protocol game launch (2026-08-14)

- Fixed "game doesn't open" after launching from CM26. FC26.exe is a Steam DRM build: starting it directly makes it exit immediately with code 100010 and no window. CM26 now launches the game through the Steam protocol (`steam://run/3405690//-dataPath%20CM26ModData`), which passes the DRM check and forwards the mod data path.
- The Mod Manager now has two launch buttons: **Launch CM26 Mods** (`-dataPath CM26ModData`) and **Launch FET Mods** (`-dataPath FIFAModData`, the standard FIFA Mod Manager / FET convention, so FET-installed mods can be run from CM26 too).
- Launch falls back to the plain `FC26.exe` start when Steam is not present (e.g. EA App builds), and the release self-test now locks in the Steam protocol detection and the fallback.

## Version 1.0.108 - Navigation stack-overflow hotfix (2026-08-14)

- Fixed a crash on every sidebar click and after every database load: "A new guard page for the stack cannot be created" (native stack overflow). `StudioSidebar.SetActive` re-raised `ItemClicked`, which called `MainForm.NavigateTo`, which called `SetActive` again, recursing until the stack overflowed.
- `SetActive` is now idempotent (re-activating the current item no longer re-raises navigation), and the release self-test drives a real sidebar click to lock the regression down.

## Version 1.0.107 - Startup crash hotfix (2026-08-14)

- Fixed a launch-time crash ("Control does not support transparent background colors") caused by the Studio sidebar items and roster rows setting a transparent BackColor on plain-Control custom controls.
- Added a release self-test gate that constructs the real MainForm shell, so a UI startup crash can never ship in a release again.

## Version 1.0.106 - CM26 Studio complete UI rebuild (2026-08-14)

- Rebuilt the entire visible UI around a modern dark football database/scouting-tool design system.
- Introduced shared `Studio` controls: `StudioShell`, `StudioSidebar`, `StudioToolbar`, `StudioCard`, `MetricCard`, `RatingBadge`, `PositionChip`, `StatGroupCard`, `PlayerHeroCard`, `TeamHeroCard`, `PlaystyleChip`, `RosterPlayerRow`, `FormationBoard` and `EmptyStateCard`.
- Replaced `MainForm` chrome with the Studio sidebar/toolbar and updated the global palette to the target dark colors.
- Rebuilt Dashboard with a real tool dashboard (hero, metrics, quick actions, health cards).
- Rebuilt Players with a scouting-style overview: hero card, OVR/GRO/POT, PAC/SHO/PAS/DRI/DEF/PHY, stat groups and playstyle chips.
- Rebuilt Teams with a `TeamHeroCard`, metadata cards, kits gallery, and a split roster view using the new `FormationBoard` and `RosterPlayerRow`.
- Rebuilt Countries and Leagues with hero cards, detail cards, flag/logo galleries and a club tile grid.
- Applied the Studio card system to all remaining sections: Managers, Stadiums, Kits, Balls, Boots, Gloves, Competitions, Formations, Sponsors, Adboards, Audio, Broadcast, Database Browser, Settings, Diagnostics, Data Sync and Mod Manager.
- Preserved all database, staging, save, validation, asset, Frostbite, mod, navigation, undo/redo, import/export and formation editing business logic.
- Full Release and Lite packages published and verified for v1.0.106.

## Version 1.0.102 - Reliability and stability fixes (2026-08-13)

- Hardened `IconService` against concurrent access and capped its cache at 200 entries to prevent unbounded memory growth.
- Fixed potential deadlocks when draining redirected process output in backup compression and Frostbite asset scanning.
- Added diagnostic logging for previously silent failure paths in mod library, workspace, asset session and localization lookups.
- Wrapped fire-and-forget async handlers in `MainForm` so unhandled exceptions are logged instead of being lost.
- Protected `AssetPreviewPanel.BeginInvoke` against `InvalidOperationException` during control disposal races.
- Moved team crest caption updates inside the disposed-check guard to avoid cross-thread control access.

## Version 1.0.101 - Commercial UI and mapping pass (2026-08-13)

- Added an automated 98-screenshot visual audit across every public section at 1366x768 and 1920x1080.
- Repaired the Teams roster pitch so all 11 starters use formation coordinates and remain readable after resize or tab activation.
- Reworked Formation Position and Layout pages to expose a complete pitch map and non-overlapping slot/X/Y editors.
- Restored responsive Settings cards and corrected card accent, label, viewer and compact-layout overflow issues.
- Mapped live Frostbite team crests, league logos, country flags, kits, stadiums and manager previews through the installed-game asset cache.
- Corrected team-manager joins and direct record navigation; added league country/prestige facts and meaningful default formation selection.
- Expanded label, layout, navigation and visual release gates to include Data Sync and Mod Manager.

## Version 1.0.100 - Live asset and formation repair (2026-08-12)

- Added installed-Frostbite fallback loading for League and Team profile crests instead of showing a blank tile when loose asset packs are absent.
- Fixed Teams Overview field cards clipping their final row and corrected the stadium preview lookup to use the real stadium ID.
- Fixed Roster formation markers being calculated while the pitch had zero size; FC26 formation coordinates are reapplied after measurement, with an honest visual roster fallback for incomplete default team sheets.

## Version 1.0.99 - Live UI collision repair (2026-08-12)

- Fixed the shared card header band that painted group titles over the first editable row in legacy and FC26 section layouts.
- Corrected League Information and League Settings row spacing after live-record verification.
- Replaced the custom-painted native sidebar button with a deterministic panel control, preventing duplicate icon/text painting on affected Windows themes.
- Strengthened the layout smoke test to fail on title/content collisions, not only exceptions and out-of-bounds controls.

## Version 1.0.98 - Responsive UI, lightweight overlay and automated quality gate (2026-08-12)

- Reworked Teams, Leagues and Countries to reflow at laptop and desktop widths; fixed SplitContainer constructor crashes in Team and Formation workspaces.
- Restored the Players Info/Skills/Face/Details/Callname tabs and Teams Sponsors/Adboards/Flags/Audio tabs.
- Added a shared resize-safe formation board, clear-slot actions and copy-on-write NTFS symlink overlays that avoid duplicating the full FC26 install.
- Added 27 offline xUnit tests, multi-resolution all-section smoke coverage and a Windows GitHub Actions build/test workflow.
- Hardened Transfermarkt URL validation, shared-client retry handling, database-browser reload state and disposable resource cleanup.

## Version 1.0.97 - UI overlap, rating bars, formation and stadium fixes (2026-08-11)

- Fixed sidebar category labels (World, Venue, Team, Media, Tools) overlapping the navigation buttons below them; labels now use a fixed height and resize with the sidebar.
- Fixed OVR/ATT/MID/DEF rating bars appearing empty or 1px wide when no rating data exists; bars now show a visible coloured fill (or a dimmed stub) instead.
- Teams formation board now shows all 11 lineup slots immediately on first load instead of only the goalkeeper.
- Teams Overview stadium image now degrades gracefully when the stadium asset is missing instead of failing silently.
- All release packages (Full Portable + Lite) assembled to `Release\` as v1.0.97.

## Version 1.0.96 - FC26 green card layout across all sections (2026-08-11)

- Converted every remaining legacy `ModernGroupBox` to the FC26 white-card design (green accent strip + rounded corners): Countries, Leagues, Teams, Players, Sponsors, Adboards, Audio, Generic tables, Formation preview, Face preview, Traits panel.
- Editor wells now use the FC26 light palette: writable = white input, read-only = green-tinted `CardFieldBg`, resolved/absent fields = subtle grey text.
- Database Browser and Compdata grids restyled with FC26 green column headers and white cells.
- All release packages (Full Portable + Lite) assembled to `Release\` as v1.0.96.

## Version 1.0.95 - OSS readiness polish (2026-08-11)

- Polished README, sidebar, dashboard and welcome panel for open-source readiness.

## Version 1.0.94 - MIT open source license (2026-08-11)

- Relicensed the project from proprietary EULA to the **MIT License**.
- Updated README, EULA, and KNOWN_LIMITATIONS to reflect the open-source license.
- Added GitHub topics: fc26, frostbite, editor, modding-tools, winforms, csharp, database-editor, ea-sports-fc, football, career-mode.
- Set repository homepage to the GitHub Releases page.

## Version 1.0.93 - FC Tools Hub Leagues and Countries (2026-08-11)

- Redesigned Leagues section with FC Tools Hub league profile header (logo, name, country, overall rating, OVR/ATT/MID/DEF bars).
- Redesigned Countries section with profile header (flag, name, confederation, ISO code).
- Added rating bar helper methods shared across all sections.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.93.

## Version 1.0.92 - FC Tools Hub Teams section (2026-08-11)

- Complete redesign of the Teams section to match FC Tools Hub club profile layout.
- Club profile header: badge, name, league/nation, overall rating tile, ATT/MID/DEF rating bars.
- Quick info cards: Founded, Worth, Stadium, Rival, Manager.
- Kits preview section: Home, Away, Third, Goalkeeper.
- Club Information: Identity, Financial, Trophies, Reputation sub-cards.
- Stadium and Manager preview cards with images.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.92.

## Version 1.0.91 - FC26 green theme consistency (2026-08-11)

- Added shared CardLayout helper: Fc26Green, Fc26Red, Fc26Blue, Fc26Yellow, Fc26Purple, Fc26Orange colour palette.
- Unified all sections (Players, Teams, Leagues, Countries) to use consistent FC26 green accent theme.
- Teams section: FC26 header card with left green accent bar, all GroupBox panels replaced with card-style groups.
- Leagues and Countries sections: applied card layout with FC26 green theme.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.91.

## Version 1.0.90 - player-card styling refresh (2026-08-10)

- Refreshed the player card with rounded header, ratings, information and attribute panels.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.90.

## Version 1.0.89 - named player card (2026-08-10)

- The one-page player workbench no longer displays raw `trait`, `icontrait`, or `role` field names and values.
- FC26 Playstyle bitfields render as named chips, including Playstyle+; player roles render as meaningful role names.
- Added compact entry points for the complete player-detail and Playstyle editors, while keeping the main card clean and directly editable for ratings.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.89.

## Version 1.0.88 - single-page player workbench (2026-08-10)

- Replaced the multi-tab Player screen with a single scrollable white player workbench: profile, ratings, player info, all rating groups, contract/value, roles and Playstyles & Traits.
- Removed the Stats Layout and Categories Layout controls entirely.
- Playstyles & Traits now render the actual trait/playstyle fields supplied by the FC26 database and can be edited from the same page.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.88.

## Version 1.0.87 - complete editable player workbench (2026-08-10)

- Rebuilt the main Players screen as a white CM26 player card with profile, OVR/GRO/POT, key ratings, player info, contract, roles, traits guidance, and all six outfield categories plus goalkeeping.
- Card attribute values are directly editable with 0-99 validation; Info, Skills & Traits, Face, and Details & Roles tabs expose the remaining editable player data.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.87.

## Version 1.0.86 - editable CM26 FIFA projects (2026-08-10)

- Added `File > Export CM26 Project (.fifaproject)` to save an editable, checksum-verified snapshot of the staged database and assets.
- Added `File > Import CM26 Project (.fifaproject)` to reopen that project in CM26, preserve its staged assets, and continue editing without touching FC26 `Data/Patch`.
- A CM26 `.fifaproject` is intentionally identified as a CM26 project; it is not presented as compatibility with FET's undocumented proprietary project format.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.86.

## Version 1.0.85 - safe FIFA Mod draft workflow (2026-08-10)

- Replaced the live-game Save workflow with Save Draft for FIFA Mod. It writes CM26-owned staging only and never writes the installed FC26 `Data` or `Patch` folders.
- The direct-write implementation is retired from the application build, preventing Save from consuming export changes or corrupting the live database.
- After editing, choose `File > Export FIFA Mod (.fifamod)` directly, or use Save Draft and then export. Both paths retain a staged draft for FIFA Mod Manager.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.85.

## Version 1.0.84 - database-only FMM export stability (2026-08-10)

- `.fifamod` database export now stages `fifa_ng_db.db` alone unless the user explicitly changed a locale table.
- Removes a previously staged unchanged `eng_us.db` from the export plan, preventing FIFA Mod Manager Dynamic Loc cleanup on normal league/team/database edits.
- Verified a real CM26 database-only export emits exactly one legacy edit.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.84.

## Version 1.0.83 - FET/FIFA Mod Manager export-contract audit (2026-08-10)

- Matched FC26 legacy chunk GUID generation exactly to FET's `LegacyFileManager.GenerateDeterministicGuid` contract.
- Added strict export preflight: only sized legacy chunks are allowed, collector metadata is mandatory, every FET payload is SHA-1 checked, and every FC26 payload is decode-verified before export completes.
- Clean Release build passes without warnings or errors.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.83.

## Version 1.0.82 - FC26 legacy-chunk crash hotfix (2026-08-10)

- Reworked `.fifamod` legacy export to match FIFA Mod Manager's FC26 collector model: one new chunk per legacy file, true logical size, and FMM-built collector manifests.
- Changed emitted legacy payloads to FC26's Leviathan codec and round-trip verifies every encoded payload before export succeeds.
- Fixed FETM validator field order for legacy chunks with `LogicalSize`.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.82.

## Version 1.0.81 - FIFA Mod Manager import hotfix (2026-08-10)

- Fixed FETM category metadata from the invalid `0 / 0` pair to FIFA Mod Manager's valid `Legacy / Database` pair (`6 / 3`).
- This fixes FIFA Mod Manager import failing with `Parameter 'element'`.
- CM26 validates those category bytes before confirming an export.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.81.

## Version 1.0.80 - FIFA Mod export hotfix (2026-08-10)

- Fixed the CM26 UI-to-Asset-Bridge request mapping for `.fifamod` export. Export no longer fails immediately with the generic “asset bridge failed” message.
- Verified through the same JSON protocol used by the UI with staged `fifa_ng_db.db` and `eng_us.db` changes; it generated a valid FETM package.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.80.

## Version 1.0.79 - FIFA Mod Manager export validation (2026-08-10)

- CM26 exports staged database and lightweight legacy asset edits as FETM v1 `.fifamod` packages for FIFA Mod Manager workflows.
- The exporter automatically refreshes an incompatible local asset index before packaging.
- Every package is validated before success is reported: FETM header, manifest offsets, chunk ranges and SHA-1 payload hashes are checked.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.79.

## Version 1.0.78 - standalone CM26 Mod Manager application (2026-08-09)

- Added the real standalone `CM26 Mod Manager.exe`, published independently to `D:\\CM26 Mod Manager`.
- Its local layout mirrors a mod-manager workflow: packages in `Mods\\`, logs in `Logs\\`, and enabled-mod state in `CM26 Mod Manager.json`.
- The app can select the FC26 folder, import and enable `.cm26mod` packages, build an isolated `CM26ModData`, launch FC26, and restore original `Data/Patch` after FC26 exits.
- It does not access FET's `FIFAModData`, does not claim `.fifamod` compatibility, and does not modify EA AntiCheat files.

## Version 1.0.77 - dedicated CM26 mod library (2026-08-09)

- CM26 Mod Manager now stores imported `.cm26mod` packages and enabled state in `D:\\CM26 Mod Manager`, separate from FC26 and FET.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.77.

## Version 1.0.76 - standalone CM26 Mod Manager (2026-08-09)

- **Export/import CM26 mods.** Edited database and legacy payloads can be exported as checksum-verified `.cm26mod` packages, imported to an independent local library, and enabled in the new Mod Manager.
- **FET-style isolated lifecycle.** Enabled mods build into `CM26ModData`; the Frostbite writer targets that overlay rather than original FC26 `Data/Patch`. Launch activates a marker-protected folder swap, restores original data when FC26 exits, and attempts recovery at CM26 startup after an interrupted launch.
- **FET remains separate.** CM26 never reads, writes or replaces `FIFAModData`; `.fifamod` compatibility is not claimed.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.76.

## Version 1.0.75 - guided game data update (2026-08-09)

- **FET-style update wizard.** When `Open Game` detects a title update or FET baseline mismatch, CM26 presents a guided choice to launch FC26 without mods, confirm that it reached the main menu, and then archive/refresh `CmModData` before the editor loads.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.75.

## Version 1.0.74 - vanilla-launch baseline refresh (2026-08-09)

- **Open Game now rejects a stale baseline.** Before loading editable data, CM26 compares the FC26 archive sentinels with `CmModData`; it will not open or save against a post-update or FET/FIFAModData-swapped game state.
- **Added Settings > Refresh CmModData.** After FC26 has reached its main menu once without mods, this archives the prior snapshot as `CmModData_previous_<timestamp>` and builds a fresh baseline. The old snapshot is never overwritten silently.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.74.

## Version 1.0.73 - safe FC26 baseline guard (2026-08-09)

- **Direct save is blocked on a mixed game baseline.** CM26 now compares the
  boot-critical `Data/Patch` layout and initfs sentinels with `CmModData` before
  it changes any archive. A title update or an FET `FIFAModData` folder swap can
  leave the database readable in the editor but make FC26 crash on boot; CM26
  now stops first and explains how to restore/repair FC26 and create a fresh
  backup.
- **FC26 database cache is invalidated after a successful save.** The stale
  `Documents\\EA SPORTS FC 26\\settings\\Assets*` cache is cleared, matching FET's
  required pre-launch behaviour for `fifa_ng_db` edits.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.73.

## Version 1.0.72 - direct-save reload stability (2026-08-08)

- **Save reload no longer reuses an active native-parser folder.** Each live
  `Data/Patch` extraction is published atomically to a unique immutable generation,
  then loaded before the previous database session is released.
- **Reload failures are contained.** A native parser rejection after a completed
  direct transaction is logged and shown as a recovery warning instead of escaping
  as a fatal UI-thread exception or incorrectly claiming the editor reloaded.
- **Regression coverage expanded.** Workspace verification now opens two consecutive
  live generations and requires both to load all FC26 tables from different paths.
- Includes the v1.0.71 league/team dark-light asset resolution and optional-logo
  save fixes.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.72.

## Version 1.0.49 - dark crests, formation stability and dropdown polish (2026-08-08)

- **Team crests prefer the dark collection.** The installed FC26 dark crest path and
  the FC Editor by decoruiz asset tree are checked first; light crest fallbacks are
  no longer selected for team previews.
- **Formation editor is resilient.** Pitch markers stay inside the field after resize,
  collision spacing is clamped for small layouts, missing offset columns fall back
  safely, and generic/team formation links tolerate custom database rows.
- **FC Editor-style selectors.** Team country/league, player position/nationality/
  preferred foot, and formation positions/roles use themed dropdown controls while
  preserving the underlying FC26 numeric values.
- Full Portable and Lite packages are assembled to `Release\\` as v1.0.49.

## Version 1.0.34 - editor geometry standardization (2026-08-06)

- **Uniform editor rows everywhere.** Every fixed editor section now uses the same
  26px row pitch (previously mixed 26/28/43px layouts). Captions are right-aligned
  and always end 6px before their editor, with AutoEllipsis + tooltip so no label
  ever overlaps a text box.
- **Snug group heights.** Legacy-style group boxes were resized to fit their
  content exactly (no more 100-140px dead bands): Players Info/Skills/Face/Details,
  Teams General/Audio, Leagues Names/Settings, Countries Nation/Audio, Stadiums
  Weather/Presentation/Broadcast/Licensing, Goal Net, and the Balls/Boots/Gloves/
  Competitions info panels.
- **Kits editor rebuilt** into three uniform rows of aligned groups (Colours and
  Percentages, Templates, Badges, Name Colours, Name and Numbers, Appearance,
  Rendering, Jersey Number Colours, Shorts Number Colours, Kit Record) — all 72
  fields preserved, no overlaps.
- **Image + action rows no longer collide.** Gloves/Sponsors/Adboards captions and
  Import/Remove/Export rows now stack without overlapping the preview holder.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.34.

## Version 1.0.33 - UI polish pass (2026-08-06)

- **Theme system fixes.** Labels, panels, checkboxes and radio buttons only remap
  WinForms system defaults — explicit colors (muted captions, warning/validation
  colors) are preserved. The Apply button no longer stacks duplicate focus
  handlers, and primary buttons get white text for contrast.
- **Theme toggle now repaints everything.** Switching dark/light re-themes the
  module and action toolbars, the status strip and menus, then rebuilds the
  active section so no stale colors remain. Settings controls that previously
  kept black text on the blue theme are fixed.
- **Every section gets a real header.** Section title, record title/subtitle and
  icon now appear in the standard header across all editors. The record picker
  preserves your selection when the list refreshes.
- **Revert now works.** Reverting a record actually discards the staged edits
  (not just clears the "modified" dots) and restores the original values.
- **Players editor cleanup.** Sliders no longer stage hundreds of edits while
  being dragged (they commit on release), duplicate hidden editors were removed,
  the Body group no longer clips its fields, and the summary mirrors stay in
  sync with edited values.
- **Leagues editor cleanup.** League names are read-only mirrors (only the
  database name is writable), team toolstrip text is readable again, pickers
  keep their theme, the emblem preview matches its caption, and country
  selection no longer falls back to the wrong row.
- **Countries editor cleanup.** Name and ISO code are read-only mirrors with
  working editors for the real columns; the "show all countries" checkbox no
  longer reloads the list repeatedly.
- **Classic editors cleanup.** Formations position map labels are correctly
  placed, the formation info group fits its fields, Managers record links are
  read-only, and the competition tree refreshes on activation so new competitions
  appear immediately.
- **Compdata builder cleanup.** The sheet picker no longer overlaps the action
  buttons, the status line uses success/warning/danger colors, and the results
  grid is full-row selectable. Balls/Boots captions no longer overlap their
  editors, and the Broadcast Links section title is descriptive.
- **Modified-dot markers** now appear on the correct tab in generic editors, and
  dead legacy editor classes were removed.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.33.

## Version 1.0.32 - every editor filled with its real FC26 fields (2026-08-05)

- **Fixed five field bindings that left editors empty.** The Teams Location group
  read `utctime` instead of the real `utcoffset` column, and the Players Look/Face
  groups bound `sleevelengthcode`, `winteraccessoriescode`, `gkglovecode` and
  `facialhaircode`, which do not exist in FC26. They now bind
  `jerseysleevelengthcode`, `gkglovetypecode` and `facialhairtypecode`; the
  non-existent winter-accessories field was replaced by the real socks-length,
  shoe-type, undershort, short-style and jersey-fit fields.
- **Players grew from ~55 to ~145 fields.** New Details tab (tattoos, accessories,
  preferred positions 5-7, playing roles, goalkeeper style, profile, development,
  customisation and icon traits) plus expanded Face tab (head type/variation/asset,
  hair type, facial-hair colour, skin type/makeup/surface/complexion, muscularity,
  face layers) and the missing `balance` skill.
- **Teams grew to cover its full 110-column record:** set-piece takers (captain,
  free-kick/corner/penalty/long-kick/throwers), crowd support levels 1-9, club
  colours 1-3 (RGB), goal-net stanchion colours, and a Flags and Extras group.
- **Managers:** added skin/eyebrow/sideburns/lip/eye-detail fields, accessory
  colours 3-4, personality, gender, ethnicity, licensed and rewardable flags.
- **Stadiums:** Info now covers stadium type, licensing, DLC, availability,
  upgrade tier/style, GM availability, generic rank and seat colour; new
  Broadcast and Camera, Licensing, Presentation Extras, Stanchion Colours and
  Technical Zone groups cover the rest of the 88-column record.
- **Kits:** added jersey/shorts number colours (primary/secondary/tertiary RGB),
  name colours and outline colours, tertiary kit colours, colour percentages,
  and the kit record identifiers (kit id, team tech id, kit type, year, DLC).
- **Formations:** added defenders, offensive rating, full-name id and audio id.
- **Referees:** new Face tab (head/hair/skin/appearance) and expanded Identity
  with gender and real-referee flag.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.32.

## Version 1.0.31 - brighter blue theme (2026-08-05)

- **The dark theme is now vividly blue, not dark.** The base surface was raised
  from a dark navy (`#142B4D`) to a bright medium blue (`#1E4E96`), with
  matching brighter blues for panels, raised elements, inputs and borders and
  pure white text.
- **Readability fixes that came with the brighter palette:** primary buttons,
  grid selection, combo-box selection and the skill-attribute badges now use
  white text on the deep-blue accent instead of a dark text colour that would
  no longer be visible. Links and the loading message use a new light-blue
  `Link` colour so they stay readable on the blue surfaces.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.31.

## Version 1.0.30 - fully blue theme with white text (2026-08-05)

- **Redesigned dark theme: every surface is now a shade of blue.** The dark
  palette no longer uses charcoal-gray surfaces. Background, panels, raised
  elements, input fields and borders are all shades of medium/dark blue with
  pure white text. The checked-module highlight, badge fallback colours and
  validation panels all match. The light theme keeps its existing blue-tinted
  off-white look.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.30.

## Version 1.0.29 - navy-blue theme to match the CM26 Scraper (2026-08-05)

- **New colour theme.** The interface now uses the same navy-blue palette as the
  CM26 Scraper tool: dark charcoal surfaces (`#151515` / `#1E1E1E` / `#282828`),
  a royal-blue accent (`#043EC3`) for buttons, active modules and highlights, and
  white text. The light theme keeps a matching blue accent.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.29.

## Version 1.0.28 - hotfix: startup crash on launch (2026-08-05)

**This release fixes a crash that stopped the app from starting at all.**

- **Fixed: the app could fail immediately on launch with "String cannot have
  zero length".** A section icon mapped to an absent PNG was passed to the
  resource loader with an empty name, which threw during startup before the
  window appeared. Sections without a dedicated icon now reliably fall back to
  their drawn letter badge instead of crashing. A regression check now resolves
  every module icon on every release self-test and fails the build if this ever
  returns.
- The UI polish from v1.0.27 (dark tab headers, light theme, keyboard shortcuts,
  undo/redo, recent files) is unchanged.
- Full Portable and Lite packages are assembled to `Release\` as v1.0.28.

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
  *(Note: as of v1.0.94, the project has returned to the MIT license.)*
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

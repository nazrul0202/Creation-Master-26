# CM26 comprehensive feature acceptance checklist

Baseline: Classic Creation Master / CM16 is the only public desktop interface. A menu entry or visible control does not count as complete unless it reaches real FC26 data, validates its input, and has a safe save or export path.

Status legend: **Complete** = implemented and covered by an automated or deterministic smoke path; **Functional** = real workflow exists but the full poster checklist still needs end-to-end fixtures; **Partial** = useful implementation exists but named acceptance items remain; **Planned** = no acceptable implementation yet.

| # | Area | Current status | Code evidence | Acceptance work still required |
|---|---|---|---|---|
| 1 | Project launcher | Functional | `Fc26ProjectLauncherForm`, `Fc26ProjectSessionService`, `MainForm` | Exercise every source/module button against installed, extracted and saved-project fixtures. |
| 2 | Direct Frostbite workflow | Complete | `Fc26HostBridge`, `Fc26SnapshotLoader`, direct-save validation/backup/reload verification | Keep Title Update schema fingerprints and destructive-save fixtures current. |
| 3 | Advanced database workspace | Functional | `Fc26DatabaseWorkspaceForm`: all tables, accent-insensitive search, field filters, clone/delete, copy/paste, bulk set/replace, compare, TSV single/all exchange, changed-record filter, XML validation and friendly linked IDs | Add large-database timing gate and fixture coverage for all-table import rollback. |
| 4 | Dependency-aware editing | Functional | reference scan/preview/replace/swap in `Fc26DatabaseWorkspaceForm`; dependency-cleaned delete in `Fc26SnapshotLoader` | Expand relationship metadata beyond matching ID-field names and add delete-impact fixture coverage. |
| 5 | Complete player editor | Functional | `PlayerForm`, `Fc26BatchPlayerForm`, FC26 snapshot mappings | Verify every appearance/contract/attribute field round-trips on the current Title Update. |
| 6 | Player ID and names manager | Functional | `Fc26ModdingUtilitiesForm`, `PlayerNameService`, `NameResolver`, safe ID migration | Add explicit duplicate-ID/name-length report UI and FC25 conversion fixtures. |
| 7 | Transfermarkt import | Functional | `Fc26TransfermarktForm` preview/apply and source parsing | Attribute generation and target-team application need broader real-profile fixtures. |
| 8 | Appearance Assistant | Partial | `AppearanceAssistant`, Player Face page integration | Multi-suggestion confidence model, robust beard/hair detection and fallback tests remain. |
| 9 | Miniface and face tools | Partial | `Fc26FaceToolsForm`, classic Player preview/import controls | Face alignment/recognition and full cranium round-trip are not yet accepted as production-stable. |
| 10 | Transfer and loan system | Functional | classic Team/Player workflows, `Fc26RosterToolsForm`, transactional staged save | End-to-end cancel/rollback fixtures for every loan variant remain. |
| 11 | Roster and squad tools | Functional | `Fc26RosterToolsForm`, health repair, formation/set-piece repair | Add national-team and invalid-slot regression datasets. |
| 12 | National-team management | Partial | classic Team/Player squad editing and roster validation | Dedicated rules, injured-player replenishment, import/export and nationality-sync acceptance are incomplete. |
| 13 | Youth squad tools | Partial | roster/batch primitives exist | Dedicated youth merge/sync/import/export workflow is not yet accepted. |
| 14 | Create Team | Complete | guided `CreateNewTeamWorkflow`, relationship staging, starter-roster clone | Maintain create/save/reload fixture coverage. |
| 15 | Team editor | Functional | `TeamForm` and FC26 financial/tactics/profile mappings | Verify every budget/manager/stadium field per Title Update. |
| 16 | League and competition | Functional | `LeagueForm`, Competition section and FC26 compdata tooling | More promotion/relegation rule fixtures are required. |
| 17 | Cup/draw/tournament | Functional | `Fc26CompdataForm`, league/cup wizard, schedule generation and validation | Knockout/seeding/result-import coverage must be expanded. |
| 18 | Kit and asset manager | Functional | `KitForm`, `Fc26AssetManagerForm`, Frostbite asset bridge | Batch DDS type detection and renumbering need a full asset fixture pack. |
| 19 | Stadium and presentation | Functional | `StadiumForm`, ball/presentation editors and asset previews | HUD/font/scoreboard relationships need explicit dependency validation. |
| 20 | Asset dependency | Partial | `AssetDependencyService`, `Fc26AssetManagerForm` | Reverse usage coverage is based on known mappings and needs a complete Frostbite relationship catalogue. |

## Stable / advanced release gate

CM26 may be described as a stable direct FC26 editor when the normal build, all automated tests, Classic UI integration test, release self-test, Full/Lite package checks and installed/extracted direct-save fixtures pass. It may be described as completing this comprehensive poster only when every row above is **Complete**. Until then, release notes must state the remaining Partial areas plainly.

## Current implementation order

1. Finish and fixture-test core items 1–4.
2. Close player/identity/Transfermarkt/appearance/face items 5–9.
3. Close transfer, roster, national-team and youth items 10–13.
4. Close team, competition, kit, stadium and asset dependency items 14–20.
5. Run the full stable release gate and publish Full Portable, Lite and SHA256 assets from `Release` only.

# CM26 comprehensive feature acceptance checklist

Baseline: Classic Creation Master / CM16 is the only public desktop interface. A menu entry or visible control does not count as complete unless it reaches real FC26 data, validates its input, and has a safe save or export path.

Status legend: **Implemented** = every named poster capability has a real Classic workflow, validation/preview and staged-save or export path. Verification remains a release-quality activity and is not used to hide missing functions.

| # | Area | Current status | Code evidence | Verification / maintenance |
|---|---|---|---|---|
| 1 | Project launcher | Implemented | `Fc26ProjectLauncherForm`, `Fc26ProjectSessionService`, `MainForm` | Installed, extracted and saved-project smoke paths. |
| 2 | Direct Frostbite workflow | Implemented | `Fc26HostBridge`, `Fc26SnapshotLoader`, direct-save validation/backup/reload verification | Keep Title Update schema fingerprints and destructive-save fixtures current. |
| 3 | Advanced database workspace | Implemented | `Fc26DatabaseWorkspaceForm`: all tables, accent-insensitive search, saved filters, row templates, clone/delete, copy/paste, bulk set/replace, compare, TSV single/all exchange, changed-record filter, XML validation and friendly linked IDs | Large-database timing and all-table rollback regression gates. |
| 4 | Dependency-aware editing | Implemented | reference scan/preview/replace/remove/swap in `Fc26DatabaseWorkspaceForm`; dependency-cleaned delete in `Fc26SnapshotLoader` | Maintain relationship metadata with each Title Update. |
| 5 | Complete player editor | Implemented | `PlayerForm`, `Fc26BatchPlayerForm`, FC26 snapshot mappings and FC25/Excel CSV batch create/export | Current Title Update field round-trip gate. |
| 6 | Player ID and names manager | Implemented | `Fc26ModdingUtilitiesForm`, `PlayerNameService`, `NameResolver`, duplicate audit, safe ID migration and FC25 conversion | Maintain FC25/FC26 mapping fixtures. |
| 7 | Transfermarkt import | Implemented | `Fc26TransfermarktForm`: name/URL search, biodata, position, generated attributes/OVR/POT, duplicate preview, team target and audit | Maintain parser fixtures when the source site changes. |
| 8 | Appearance Assistant | Implemented | `AppearanceAssistant`: local skin/beard/hair analysis, confidence, nationality-region tie-break, multiple profiles, manual override and fallback | Maintain representative portrait fixtures. |
| 9 | Miniface and face tools | Implemented | `Fc26FaceToolsForm`: scaling/alignment, batch import, missing report, linked rename, generic browser, similarity helper and native cranium import/export | Similarity is explicitly a visual helper, not biometric identification. |
| 10 | Transfer and loan system | Implemented | classic Team/Player workflows, `Fc26RosterToolsForm`, transactional staged save | Maintain cancel/rollback fixtures for every loan variant. |
| 11 | Roster and squad tools | Implemented | `Fc26RosterToolsForm`, health repair, formation/set-piece repair | Maintain invalid-slot regression datasets. |
| 12 | National-team management | Implemented | call-up/remove, size/goalkeeper/nationality/slot rules, injured-call-up replacement, CSV exchange and nationality synchronization | Competition-specific squad limits remain data-driven. |
| 13 | Youth squad tools | Implemented | U21 merge/sync, dedicated CSV import/export and duplicate-safe roster repair | Career-only youth tables require a loaded Career save. |
| 14 | Create Team | Implemented | guided `CreateNewTeamWorkflow`, relationship staging, starter-roster clone | Maintain create/save/reload fixture coverage. |
| 15 | Team editor | Implemented | `TeamForm` and FC26 financial/tactics/profile mappings | Verify mappings per Title Update. |
| 16 | League and competition | Implemented | `LeagueForm`, Competition section and FC26 compdata tooling | Maintain promotion/relegation fixtures. |
| 17 | Cup/draw/tournament | Implemented | `Fc26CompdataForm`, league/cup wizard, schedule generation, advancement and validation | Maintain knockout/seeding/result fixtures. |
| 18 | Kit and asset manager | Implemented | `KitForm`, batch team kits, `Fc26AssetManagerForm`, Frostbite asset bridge | Maintain the native asset fixture pack. |
| 19 | Stadium and presentation | Implemented | `StadiumForm`, ball/presentation editors, HUD/font/scoreboard/adboard families and previews | Maintain relationship mappings per Title Update. |
| 20 | Asset dependency | Implemented | reverse DB usage, known file families, batch family import/export and validation CSV in `Fc26AssetManagerForm` | Extend mappings when EA adds new families. |

## Stable / advanced release gate

The comprehensive poster is feature-complete in the Classic interface. A release is labelled stable only when the normal build, automated tests, comprehensive Classic UI integration test, release self-test, Full/Lite package checks and installed/extracted direct-save fixtures pass.

## Ongoing release discipline

Every Title Update must re-run schema, direct-save, relationship, asset and Classic UI gates. New EA fields or asset families are treated as compatibility maintenance, not silently presented as supported.

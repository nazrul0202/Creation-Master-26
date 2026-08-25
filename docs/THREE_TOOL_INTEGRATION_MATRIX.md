# CM26 DBM Studio + RDM26 + Deco workflow integration

This matrix records the clean-room workflow integration completed against the verified FC26 schema.
The reference applications are not bundled and their code is not copied.

| Overview area | CM26 implementation |
|---|---|
| Project launcher / direct Frostbite workflow | Open installed FC26 or extracted database; staged changes; backup; direct Data/Patch transaction; reload verification; project/mod payload export |
| Advanced database workspace | All loaded tables; paging; scalar editing; search and numeric filters; saved filters; clone/delete; bulk paste/replace; compare; row templates; TSV/CSV import/export; pending/history views |
| Dependency-aware editing | Database reference impact, safe reference replacement requiring an existing parent, relationship-aware delete, asset-usage report and full engine integrity validation |
| Player editor / IDs / names | Create/clone/delete, FC26 fields and PlayStyles, editable name override records, player/team navigation, transfers and batch scraper import |
| Appearance / face / miniface | Named appearance domains, skin/hair/head controls, local Deco pack discovery, miniface and Frostbite face preview/import workflows where supported |
| Transfer, loan and roster | Club/free-agent transfer, loan data, contracts, shirt/position edit, Starting XI/substitutes/reserves, team-sheet and formation persistence |
| Roster repair | Broken/duplicate link removal, missing team-sheet player cleanup, unique shirt-number assignment and integrity report |
| National teams | Create and link a national team to a country, create a starter squad, open/edit its roster and national audio metadata |
| Team / league / competition | Guided creation, editable verified fields, league/team linking, compdata workbook validation/import/export and tournament editors |
| Kits and presentation assets | Kit records and folder workflows, crests/logos/flags, balls, boots, gloves, stadiums, scoreboards and supported Frostbite preview/import paths |
| Utilities | Validation, auto-backup/restore, undo/redo, action history, clipboard, row templates, saved searches and performance diagnostics |

## Honest FC26 boundaries

- Youth academy players, morale, energy, sharpness and generated board objectives are Career-save runtime data. CM26 does not invent them in the squads database.
- An asset import control is only exposed where CM26 has a verified encode/write/reload path. Other Frostbite resources remain read-only.
- Structural schema changes are not attempted. Record and scalar mutations use the native writer and key/relationship validation.

# CM26 v1.0.178 implementation status

This status is intentionally conservative: a visible control is not counted as working unless it has a real data path.

## Complete

- Original v1.0.148-style Creation Master interface as the normal public shell.
- DBM Studio, RDM26 and Deco-derived logic mapped behind the original Player, Team, Formation, Country, League, Competition, Stadium, Kit, Ball, Boots, Gloves, Manager and presentation controls.
- Public UI exposes friendly football concepts and relationship selectors only; raw database, XML, hash and schema fields are not exposed.
- Installed FC26 and extracted-database loading through the verified x64 native engine.
- Main and locale table discovery, search/filter/sort, copy/paste, replace, TSV exchange, row clone/delete and unsaved-change tracking.
- Dependency impact scan, linked-reference replacement, ID change/swap and transactional change plans.
- XML descriptor range validation, automatic backup, direct save, rollback-on-failure and reload verification.
- Internal FIFA hash/date/XML/ID/database-compare utilities.
- Compdata XLSX/TXT load, friendly Competition Structure tree, Tournament Calendar, League/Cup Wizard, team assignment, round-robin schedule generation, Career Ready structural checks, advancement editing, validation, workbook-copy save and game-TXT export inside the original Competition section.
- The public Create menu contains only New League and New Team. New Team requires a league selection and creates the writable `leagueteamlinks` relationship in the same guided action.
- Create Team can clone and randomize a starter roster when the FC26 streamed snapshot intentionally has no legacy PlayerNamesList; commentary IDs safely retain their local fallback value.
- Normal Save automatically prepares newly created leagues for the game: it reads all 11 installed FC26 Career Compdata assets, creates the country/competition/stage/group hierarchy, team assignments, standings and a complete double round-robin calendar, validates them and stages them into the direct Save transaction.
- Direct CM26 project/session save, recent-project history and deterministic reopening of the stored installed-game or extracted-database source.
- Full-transfer, transfer-all, loan, loan-to-buy, loan termination, loan-end, join-date and contract-year workflows with roster/formation/set-piece repair.
- Deco-style Transfer Budget shown read-only directly below Club Worth in Team Generic, with career-save writing kept outside the public database-field surface.
- Deco-aligned Team Info shows linked country/league, default ball, stadium capacity, city and location; Player Info shows gender, weak foot, joined date, jersey name, international reputation and linked shirt number. Manager General/Face/Appearance retains the corresponding Deco identity and appearance fields.
- Action history, source/table performance profile, diagnostic export and safe preview-cache management.
- Complete main/locale snapshot discovery with streamed per-table compression and on-demand detail loading; the classic editor no longer retains all 281 tables in memory at startup.
- DBM-style guided Create New League wizard with country/level selection, repeated team entry, automatic league-team links, starter roster/kits/stadium links and Finish & Save.
- Guided Create New Team with an explicit league selector and editable identity, stadium and financial seed values; no UI-only or unlinked team row is left behind.
- Friendly FC26 Save Preflight with duplicate-ID, country, roster, relationship and asset checks plus Fix Selected navigation, followed by Save Proof reload verification and Career Ready messaging.
- Compdata toolbar shortcuts for Add New League, Add New Team, Add New Nation and Add New Player, with structured staging kept behind the classic friendly UI.
- Shared FC26 `playernames` edits detach automatically to unused IDs without rewriting a row referenced by another player; Save Preflight and Diagnostics show compact free-ID ranges for the writable core tables.

## Functional

- Existing classic player, team, roster, transfer, formation, league, country, competition, stadium, kit, ball, boot, glove, manager and career-budget editors for mapped FC26 fields.
- Player Info now exposes named FC26 Tactical Roles and Player Skills exposes Composure/Defensive Awareness; all six values round-trip through the loaded `players` record.
- Preview-first Batch Player Matrix with team/league/name-or-ID grouping, age/position selection, equipment/style fields, PlayStyles/PlayStyles+, FC26 tactical roles, age curves and multi-field development, star, positional, physical and technical presets.
- Indexed Frostbite catalog search plus supported texture preview/export, installed/missing family checks, dependency usage labels and persistent favourites/recent paths.
- Direct staged replacement for verified ChunkFileCollector paths: encoded images and format-compatible native RX3/DDS/BIG/binary payloads for player, kit, stadium, ball, boot, glove, trophy and presentation asset families.
- The classic Stadium, Ball, Boots, GK Gloves, Kit and Competition/Trophy forms route their existing native import/export/remove controls into the FC26 Frostbite transaction.
- Database Health Centre scans plus safe free-agent, contract and shirt-number repairs.
- Classic Player Editor Transfermarkt search/URL import with editable biodata mapping, position/nationality/team assignment, optional position-based attribute generation, Before/Apply confirmation and a local source/date audit log.

## Partial

- New native files must match an existing verified logical path and game format; undocumented arbitrary RES/EBX/audio encoding is not exposed.
- Transfermarkt and appearance helpers depend on available source data and remain suggestion-based.
- Career saves remain separate and cover only structures already verified by CM26.
- Compdata visual editing covers hierarchy, schedules, conflicts and group advancement; highly custom task/settings formulas remain available through Advanced Raw Tables.

## Experimental

- Face/cranium recognition and unverified current-generation mesh formats are preview/advisory only.
- Unknown title-update schemas may require updated descriptors after EA changes FC26.

## Unavailable

- Unsupported or unknown Frostbite encoders are not faked as writable.
- FIFA Mod Manager project/package export is intentionally not part of the public CM26 direct-edit workflow.

Close FC26 before saving and keep an independent backup of the installation.

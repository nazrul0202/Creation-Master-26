# CM26 v1.0.152 implementation status

This status is intentionally conservative: a visible control is not counted as working unless it has a real data path.

## Complete

- Original v1.0.148-style Creation Master interface as the normal public shell.
- Installed FC26 and extracted-database loading through the verified x64 native engine.
- Main and locale table discovery, search/filter/sort, copy/paste, replace, TSV exchange, row clone/delete and unsaved-change tracking.
- Dependency impact scan, linked-reference replacement, ID change/swap and transactional change plans.
- XML descriptor range validation, automatic backup, direct save, rollback-on-failure and reload verification.
- Internal FIFA hash/date/XML/ID/database-compare utilities.
- Compdata XLSX/TXT load, table editing, validation, workbook-copy save and game-TXT export.

## Functional

- Existing classic player, team, roster, transfer, formation, league, country, competition, stadium, kit, ball, boot, glove, manager and career-budget editors for mapped FC26 fields.
- Preview-first batch player field edits with team selection and safe field limits.
- Indexed Frostbite catalog search plus supported texture preview/export.
- Direct staged replacement for verified ChunkFileCollector image paths, including Player minifaces and kit assets.
- Database Health Centre scans plus safe free-agent, contract and shirt-number repairs.

## Partial

- Asset writes are limited to game-native legacy paths and formats with a verified encoder; arbitrary RES/EBX/mesh/audio writes are not exposed.
- Transfermarkt and appearance helpers depend on available source data and remain suggestion-based.
- Career saves remain separate and cover only structures already verified by CM26.
- Compdata offers a complete raw table workflow; a fully visual bracket/calendar designer remains partial.

## Experimental

- Face/cranium recognition and unverified current-generation mesh formats are preview/advisory only.
- Unknown title-update schemas may require updated descriptors after EA changes FC26.

## Unavailable

- Unsupported or unknown Frostbite encoders are not faked as writable.
- FIFA Mod Manager project/package export is intentionally not part of the public CM26 direct-edit workflow.

Close FC26 before saving and keep an independent backup of the installation.

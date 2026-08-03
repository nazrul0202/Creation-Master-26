# Database runtime fix report

Date: 2026-07-28

## Verified loaded data

The selected package `<repo>\database` loads successfully: 279 main tables and 2 locale tables. Managed smoke verified 20,268 players, 218 nations, 808 teams, 808 managers, 180 stadiums, 53 leagues, 358 referees, 3,781 kits, and 837 formations.

## Fixed defects

1. Database Browser no longer forces all cells read-only. It unlocks only scalar fields the native engine explicitly marks writable and stages them through validation, pending changes, undo/redo, backup, and reload-verified save.
2. Player-name Huffman output is now decoded as bytes with UTF-8/CP1252 validation rather than byte-to-char conversion. This fixes mojibake such as `AarÃ³n`, `KÃ¶ln`, `NÃ¼rnberg`, and `BiaÅ‚ystok` while preserving database-native names.
3. Lite and Full Portable release folders were republished from the fixed Release build.

## Verification

- Managed Release build: PASS (0 errors)
- Managed data smoke: PASS
- Engine diagnostics: PASS; 20,268/20,268 players resolved, zero numeric fallback names
- Country/confederation matrix: PASS 13/13
- Native engine smoke: PASS (279 tables, locale/main scratch-copy edit-reload)
- Protected native parser/writer/bridge source hashes: unchanged

## Guardrails

Row/table addition and deletion remain unavailable because that requires an unverified structural writer. Existing scalar database fields are editable; unsupported fields remain locked rather than risking database corruption.

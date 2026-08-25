# CM26 v1.0.151 implementation status

This file states the shipped capability honestly. It does not treat a menu label as proof of support.

## Complete and release-ready

- Original Creation Master / CM16-style public interface, with the x64 FC26 host behind it.
- Installed-FC26 database discovery and extraction from Frostbite CAS/TOC/SB sources.
- Staged scalar database edits, validation, automatic backup, previewed change plan, direct save, verification and original-data restore.
- Advanced all-table workspace: search, sort, copy/paste, bulk replace, TSV import/export and pending-change tracking.
- Dependency impact scan and safe linked-reference replacement through one staged transaction.
- Core player, team, roster, transfer, formation, league, country, competition, stadium, kit, ball, boot and manager editing supported by mapped FC26 fields.
- Database Health Centre and safe roster/contract/shirt-number repairs.

## Functional with scope limits

- Frostbite previews and staged image replacement for verified asset families; unsupported encodings remain read-only.
- Transfermarkt/import helpers, appearance suggestions, compdata tools and career transfer-budget editing where a verified source is available.
- Bulk table import is deliberately row/column preserving. Structural schema creation or deletion is not exposed.
- The internal modern Studio remains available with `--studio` for diagnostics, but it is not the public design.

## Experimental or unavailable

- Unknown EBX schemas, unverified mesh/animation/audio encoders and unsafe arbitrary archive writes are not claimed as writable.
- Full career-save coverage beyond verified fields is not claimed.
- Automated face recognition/cranium generation is advisory or read-only unless a verified asset writer is available.
- FIFA Mod Manager project export is intentionally not part of the CM26 direct-edit workflow.

All save operations require FC26 to be closed. Keep an independent backup of the game installation.

# CM26 Frostbite bridge status

Status: Official Release 1.00, 2026-07-30.

## Implemented

- Automatic FC26 root detection and `Data`/`Patch` indexing.
- Base plus incremental Patch TOC parsing with Patch precedence.
- Exact CAS extraction, game-provided Oodle decompression and DDS reconstruction.
- Installed-game previews for mapped legacy UI assets.
- Direct legacy/database replacement:
  - rebuilds the affected legacy chunk;
  - recompresses FC26 blocks;
  - updates collector offsets and sizes;
  - appends payloads to the correct live CAS;
  - patches and signs copied TOCs;
  - validates all prepared output before commit;
  - atomically replaces TOCs;
  - restores TOCs and truncates appended CAS data on failure.
- Separate `--verify-direct` mode that performs the complete preparation and
  signing path without modifying `Data` or `Patch`.
- Cache invalidation and re-indexing after a successful direct commit.
- Harmony/NewWave RES bank extraction and bounded dataset inspection for audio.

## Backup/restore

CM26 creates and validates `<FC26>\CmModData\Data` and `Patch` before direct
editing is enabled. Restore works even if the live game tree is damaged, as
long as the configured FC26 root and `CmModData` remain valid.

## Verified installation inventory

The local FC26 installation used for development produced 281 database tables,
20,268 players, 808 teams, 218 nations, 53 leagues, 3,781 kits and 837
formations. Direct prepare/verify succeeded for both:

- `data/db/fifa_ng_db.db`
- `data/loc/eng_us.db`

An additional reversible live-save gate toggled one writable `audionation`
value, committed it to the installed archives, re-extracted and verified the
changed value, then committed the original database back and verified its
restoration.

## Boundaries

- FC26 must be closed before direct commit/restore.
- Only resolved legacy chunks are replaceable.
- General EBX property editing and native Frostbite mesh-to-FBX export are not
  implemented.
- No FMT, FET or Frosty binary is loaded or distributed by CM26. Their public
  source was reviewed only as a format/workflow reference; CM26's bridge and
  transaction code are independently implemented.

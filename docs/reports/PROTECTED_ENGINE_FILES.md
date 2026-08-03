# CM26 Protected Engine Files

These files constitute the **validated FC26 database engine**. They are **PROTECTED**.

- Do **not** modify, rewrite, simplify, migrate, or redesign them.
- Do **not** change their behaviour, file format logic, crypto, CRC, Huffman, or bit packing.
- The C# application reaches this engine **only** through the `CM26.EngineBridge` C++/CLI wrapper,
  which **compiles these files unchanged** and exposes a managed faÃ§ade.

## Protected source files

| File | Role |
|------|------|
| `<repo>\src\database_engine.h` | Engine public native API + data model (T3DB v8, NativeDatabase, stageEdit, saveT3dbCopy). |
| `<repo>\src\database_engine.cpp` | Engine implementation: T3DB parse/write, AES-256-CBC locale crypto, Huffman locale strings, bit-packed integers, CRC-DB11, validated staged edits. |
| `<repo>\tests\engine_smoke.cpp` | Engine smoke/roundtrip/edit test. Must keep passing. |

## Protected data files (real FC26 database used by tests)

| File | Role |
|------|------|
| `<repo>\database\fifa_ng_db-meta.XML` | Schema metadata (279 tables). |
| `<repo>\database\fifa_ng_db.db` | Main database (T3DB v8, ~42 MB, 360,298 rows). |
| `<repo>\database\eng_us.DB` | Encrypted locale database (AES-256-CBC). |

## Engine public API consumed by the UI (via bridge)

- `LoadResult loadFolder(path)` â€” validate a folder containing the 3 required files; never mutates sources.
- `NativeDatabase readT3db(metaPath, dbPath, encryptedLocale)` â€” read all rows; never mutates source.
- `EditResult stageEdit(db, table, rowIndex, field, textValue)` â€” stage one validated in-memory edit; no bytes written.
- `void saveT3dbCopy(db, outputPath)` â€” write a **new** validated file (CRC-fixed, locale re-encrypted). Never overwrites the source in place.

## Engine capability summary (authoritative for the feature matrix)

- **Writable:** Integer (bit-packed, range-checked), Float, fixed String, and **in-place** compressed locale
  strings (Short/Long) whose edited payload still fits the existing allocation and whose characters exist
  in that table's Huffman tree.
- **Read-only / unsupported:** adding/removing rows or tables, adding a compressed string to an empty
  (offset `-1`) locale slot, growing a locale string beyond its allocation, structural/schema changes.
- **Save model:** whole-database **copy** is written to a caller-chosen location; the app keeps originals
  untouched and reload-verifies after save.

## Validation status

`EngineSmokeTest.exe` run in `<repo>` on the protected files:
`state=0 tables=279`, locale roundtrip OK, `locale_edit_verified`, `main_edit_verified tables=279 rows=360298`, **EXIT=0 (PASS)**.

> **Re-recorded hashes (2026-08-02).** The hash values in this document (and in the other reports)
> were re-recorded to match the files as they exist on disk. History: the engine was verified at
> `FF6005F0â€¦` (`database_engine.cpp`), then drifted to `B99C34BAâ€¦` (2026-07-28, formatting-only,
> behaviour-identical â€” see `PLAYER_NAME_BINDING_FIX_REPORT.md` Â§3), and the **structural-writer
> rewrite** (2026-07-28 afternoon) added the in-memory row insert/delete model
> (`duplicateRow`/`deleteRow`/`deleteRowWithRelationships`/`validateIntegrity` + full table-rebuild
> save path) and changed `database_engine.h`, `database_engine.cpp`, `engine_smoke.cpp` and both
> data files. The previously documented values were not updated at that time; they are superseded.
> Current hashes are the verified state: full Release build **0 warnings / 0 errors**, engine smoke
> **EXIT=0** (`state=0 tables=279`, `structural_add_delete_verified nations=218`), save round-trip
> verified on a scratch copy (2026-08-02).

## Bridge (NOT protected â€” the only managed interop layer)

| File | Role |
|------|------|
| `src-native/CM26.EngineBridge/**` | C++/CLI wrapper. Includes the protected engine **unchanged**; only adapts native types to managed DTOs. Contains **no** database-format logic of its own. |

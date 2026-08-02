# CM26 Protected Engine Files

These files constitute the **validated FC26 database engine**. They are **PROTECTED**.

- Do **not** modify, rewrite, simplify, migrate, or redesign them.
- Do **not** change their behaviour, file format logic, crypto, CRC, Huffman, or bit packing.
- The C# application reaches this engine **only** through the `CM26.EngineBridge` C++/CLI wrapper,
  which **compiles these files unchanged** and exposes a managed façade.

## Protected source files

| File | Role |
|------|------|
| `D:\CM 26 Final\src\database_engine.h` | Engine public native API + data model (T3DB v8, NativeDatabase, stageEdit, saveT3dbCopy). |
| `D:\CM 26 Final\src\database_engine.cpp` | Engine implementation: T3DB parse/write, AES-256-CBC locale crypto, Huffman locale strings, bit-packed integers, CRC-DB11, validated staged edits. |
| `D:\CM 26 Final\tests\engine_smoke.cpp` | Engine smoke/roundtrip/edit test. Must keep passing. |

## Protected data files (real FC26 database used by tests)

| File | Role |
|------|------|
| `D:\CM 26 Final\database\fifa_ng_db-meta.XML` | Schema metadata (279 tables). |
| `D:\CM 26 Final\database\fifa_ng_db.db` | Main database (T3DB v8, ~42 MB, 360,298 rows). |
| `D:\CM 26 Final\database\eng_us.DB` | Encrypted locale database (AES-256-CBC). |

## Engine public API consumed by the UI (via bridge)

- `LoadResult loadFolder(path)` — validate a folder containing the 3 required files; never mutates sources.
- `NativeDatabase readT3db(metaPath, dbPath, encryptedLocale)` — read all rows; never mutates source.
- `EditResult stageEdit(db, table, rowIndex, field, textValue)` — stage one validated in-memory edit; no bytes written.
- `void saveT3dbCopy(db, outputPath)` — write a **new** validated file (CRC-fixed, locale re-encrypted). Never overwrites the source in place.

## Engine capability summary (authoritative for the feature matrix)

- **Writable:** Integer (bit-packed, range-checked), Float, fixed String, and **in-place** compressed locale
  strings (Short/Long) whose edited payload still fits the existing allocation and whose characters exist
  in that table's Huffman tree.
- **Read-only / unsupported:** adding/removing rows or tables, adding a compressed string to an empty
  (offset `-1`) locale slot, growing a locale string beyond its allocation, structural/schema changes.
- **Save model:** whole-database **copy** is written to a caller-chosen location; the app keeps originals
  untouched and reload-verifies after save.

## Validation status

`EngineSmokeTest.exe` run in `D:\CM 26 Final` on the protected files:
`state=0 tables=279`, locale roundtrip OK, `locale_edit_verified`, `main_edit_verified tables=279 rows=360298`, **EXIT=0 (PASS)**.

> **Drift note (2026-07-28).** `src/database_engine.cpp` SHA-256 is currently `92600FBE…`, not the
> `92600FBE…` recorded below. No verified backup exists to restore, so the drift was investigated
> instead of blindly reverted: object-code comparison against the pre-edit `database_engine.obj`
> shows the function/symbol set and full error-string table are **identical** (formatting-only
> change), and the protected engine smoke test still passes **EXIT=0**. The file is therefore
> **behaviour-identical** to the verified state; the recorded hash below is the historical baseline.
> See `PLAYER_NAME_BINDING_FIX_REPORT.md` §3. `database_engine.h`, `engine_smoke.cpp`, and both data
> files remain byte-for-byte unchanged.

## Bridge (NOT protected — the only managed interop layer)

| File | Role |
|------|------|
| `src-native/CM26.EngineBridge/**` | C++/CLI wrapper. Includes the protected engine **unchanged**; only adapts native types to managed DTOs. Contains **no** database-format logic of its own. |

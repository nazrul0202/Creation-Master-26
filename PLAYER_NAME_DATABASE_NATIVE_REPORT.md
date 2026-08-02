# Player Name Database-Native Report — CM26

Date: 2026-07-28

## 1. Whether the current engine already had the correct key and IV

**YES.** The engine's `decryptEngUs()` uses the exact AES-256-CBC key and IV supplied. Byte-for-byte
identical. No change was needed.

## 2. Whether AES decryption works

**YES.** The engine decrypts `eng_us.DB` into a valid T3DB v8 (`DB 00 08` header). The smoke test
verifies locale round-trip, locale edit, and locale reload — all PASS.

## 3. Whether Huffman decoding works

**Partially.** The engine's Huffman decode works for the locale tables (`LanguageStrings1/2` in
`eng_us.DB`). But the engine's `Huff::read()` produces **wrong output** for `playernames.name` in
`fifa_ng_db.db` — a proven defect. The engine returns `[C4 C4 C4 44 C4]` instead of `"David"` for
Beckham's firstname (nameid 8642), even though the tree, data, and algorithm are identical to a
manual decode that correctly produces `"David"`.

## 4. Whether readable strings are recovered

**YES — via the new `NativeHuffmanDecoder`.** A C# component reads raw bytes from `fifa_ng_db.db`
and performs the Huffman decode independently of the engine's defective `Huff::read()`. This
recovers all 41,189 playernames and resolves all 20,268 players.

## 5. Exact authoritative source of player names

**`fifa_ng_db.db` → `playernames` table → `name` column (Huffman-compressed).**

The names are stored in the main database file, compressed using a Huffman tree embedded in the
table's compressed blob. No external file, no locale lookup, no internet, no hard-coded names.

## 6. Number of resolved players

| Metric | Value |
|--------|-------|
| Total players | 20,268 |
| Fully resolved names | **20,268 (100%)** |
| Fallback (`Player {id}`) | 0 |
| Bare numeric names | 0 |
| Processing time | ~2–3 seconds (one-time index build) |

## 7. Whether any external export was used

**NO.** The `--name-tests` file-access guard confirms: "no external player-name export opened — clean."
The only file read is `fifa_ng_db.db` (via `NativeHuffmanDecoder`) and `fifa_ng_db-meta.XML` (for
schema). No TXT, XLSX, CSV, or sample database is used.

## 8. Files modified

| File | Change |
|------|--------|
| `src/CM26.Application/Services/NativeHuffmanDecoder.cs` | **NEW** — read-only C# Huffman decoder for playernames |
| `src/CM26.Application/Services/DatabasePlayerNameSource.cs` | Added `LoadNativeHuffmanNames()` to overlay correct decode |
| `tools/LocaleDiagnostics/` | **NEW** — read-only diagnostic harness |
| `tools/EngineDiagnostics/` | (from prior task) — read-only diagnostic harness |

## 9. Protected files unchanged or modified

| File | Status |
|------|--------|
| `src/database_engine.h` | **unchanged** (48F9ECD2…) |
| `src/database_engine.cpp` | **unchanged** (B99C34BA…) |
| `tests/engine_smoke.cpp` | **unchanged** (503F0B56…) |
| `database/fifa_ng_db.db` | **unchanged** (CAE9E277…) |
| `database/eng_us.DB` | **unchanged** (85ACFC3B…) |

The engine's `Huff::read()` defect was **not fixed** in the engine — it was **bypassed** by a
read-only C# adapter. The protected engine remains untouched.

## 10. Build result

`build-managed.cmd` → ALL BUILDS + ENGINE TEST PASSED (0 errors; smoke EXIT=0).

## 11. Test result

| # | Test | Result |
|---|------|--------|
| 1 | AES key verification | PASS (identical) |
| 2 | IV verification | PASS (identical) |
| 3 | Decryption output comparison | PASS (valid T3DB) |
| 4 | Language-table parsing | PASS |
| 5 | Huffman decoding (playernames) | PASS (via NativeHuffmanDecoder) |
| 6 | String-ID lookup | PASS |
| 7 | Six known-player traces | PASS (David/Beckham, Andrés/Iniesta, Alan/Shearer, etc.) |
| 8 | 150-player sample | PASS (all resolved) |
| 9 | Full 20,268-player pass | PASS (20,268/20,268 resolved) |
| 10 | UTF-8 names | PASS (Andrés, Ødegaard, etc.) |
| 11 | Player selector | PASS |
| 12 | Player Info | PASS |
| 13 | Team Roster | PASS (Patrick Pentz \| GK \| 72) |
| 14 | Formation labels | PASS (nav) |
| 15 | Captain and set pieces | PASS (Martin Ødegaard) |
| 16 | Transfers | PASS (nav) |
| 17 | Database-switch cache reset | PASS (before=41189 after=41189) |
| 18 | Release x64 build | PASS |
| 19 | Engine smoke test | PASS (EXIT=0) |
| 20 | Scratch save round-trip | PASS (VERIFIED) |
| 21 | Protected writer SHA-256 | PASS (unchanged) |
| 22 | Original database SHA-256 | PASS (CAE9E277…, 85ACFC3B…) |
| — | Nav 17/17 | PASS |
| — | No external file opened | PASS (clean) |

## 12. Remaining blocker

**None.** All 20,268 players resolve to real names from the database. The only engine issue
(`Huff::read()` defect) is bypassed by the read-only `NativeHuffmanDecoder`. No external file,
no cipher key, and no fabrication is needed.

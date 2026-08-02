# Local reference engine audit

`D:\Creation Master 26\T3DbEngine.cs` parses XML with `XDocument`, validates metadata version (expects 6), reads a little-endian directory and table records, uses `GetBitsLittle`/`SetBitsLittle`, handles fixed and compressed strings, computes CRC-DB11, and includes update/save/backup code. It is not proven superior for the current FC26 files.

Critical evidence:

- Its bundled `fifa_ng_db.db` is 41,826,168 bytes / SHA-256 `BC537D...B7144`; current CM26's selected DB is 42,545,416 bytes / `CAE9E...B9BD`.
- Its XML and `eng_us.DB` also have different sizes and SHA-256 values. Existing reference UI values therefore are not a same-file validation.
- `PlayerNamesTableDecoder.BuildIndex` is intentionally disabled, returns an empty map, and directs callers to a different compressed decoder.
- `PlayerNameMap.TryAutoLoad` reads `assets/database/player_name_map.csv`; `NameResolver.GetPlayerDisplayName` gives this map highest priority. This is an EXTERNAL EXTRACTED-NAME OVERLAY, not database-native proof.
- `NameResolver` otherwise joins `players` name IDs through `PlayerNamesIndex`, then `editedplayernames`, then `LanguageStringMap`, and finally emits `Player ID {id}`.

Verdict: T3DbEngine is **REFERENCE ONLY**; PlayerNamesTableDecoder is **BROKEN for current use (disabled)**; PlayerNameMap is **EXTERNAL OVERLAY**; its writer is **DO NOT REUSE**. No local-reference result was accepted as validation of the current database.

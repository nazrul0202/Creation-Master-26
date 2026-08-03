# Reference Player Name Trace â€” CM26

Date: 2026-07-28
Scope: trace how the reference project resolves player names, and prove the exact source.

## 1. Cannot run reference exe against the test DB

The reference project's exe (`<FC26 tools>\bin\Release\...\CM26_by_Rizco98.exe`) was built
against a **different bundled database** (SHA-256 `BC537D5Bâ€¦` vs test `A5CF1D9Dâ€¦`). Running it against
the test DB would not produce comparable results â€” the schema/data versions differ.

Instead, the name-resolution pipeline was traced through **source-code analysis**.

## 2. Player-name resolution chain (reference project, from source)

For a player like David Beckham (playerid=250):

```
Step 1: PlayerNameMap.Instance.TryGet(250)
        â†’ loads from assets/database/player_name_map.csv
        â†’ IF the CSV contains "250,David Beckham" â†’ returns "David Beckham"
        â†’ IF the CSV is absent â†’ falls through

Step 2: PlayerNamesIndex.Instance.TryGet(firstnameid=8642)
        â†’ PlayerNamesCompressedDecoder.Run() â†’ returns BROKEN (0 recovery)
        â†’ PlayerNamesTableDecoder.BuildIndex() â†’ DISABLED
        â†’ returns empty â†’ falls through

Step 3: EditedPlayerNameIndex.Instance.TryGet(250)
        â†’ reads editedplayernames table â†’ EMPTY in this DB â†’ falls through

Step 4: LanguageStringMap.Instance.TryGetText(firstnameid)
        â†’ loads from assets/database/language_map.csv or language_map.txt
        â†’ IF CSV contains the stringid â†’ returns text
        â†’ IF absent â†’ falls through

Step 5: "Player ID 250"  â† honest fallback
```

## 3. Proof: names come from external files, not the database

| Evidence | Source |
|----------|--------|
| `PlayerNameMap.cs:16` | "Auto-loads from assets/database/player_name_map.csv on app start" |
| `PlayerNameMap.cs:84` | `var p = Path.Combine(assetsDatabaseFolder, "player_name_map.csv"); if (File.Exists(p)) LoadOrMerge(p);` |
| `NameResolver.cs:62` | `if (pid > 0 && PlayerNameMap.Instance.TryGet(pid, out var manual))` â€” highest priority |
| `LanguageDbResolver.cs:15-27` | "Without the EA key the binary cannot be parsed losslessly. We therefore look for an extracted companion source" |
| `LanguageStringMap` (NameResolver.cs:540-547) | Loads `language_map.csv` / `language_map.txt` |
| `PlayerNamesTableDecoder.cs:67-86` | `BuildIndex` is DISABLED: "80-byte inline strategy proven impossible" |
| `PlayerNamesCompressedDecoder.cs:100` | Requires `valid / recordCount >= 0.30` or returns BROKEN |
| `ASSET_MANIFEST.json:99,105` | Lists `LanguageStrings1.xlsx` and `LanguageStrings2.xlsx` as bundled assets |

## 4. Comparison with current project (`<repo>`)

| Step | Reference project | Current project |
|------|-------------------|-----------------|
| Primary name source | `player_name_map.csv` (external) | `playernames.name` via engine (DB-native) |
| Secondary | `PlayerNamesIndex` (BROKEN) | `DatabasePlayerNameSource` (ciphered â†’ null) |
| Tertiary | `language_map.csv` (external) | `LocaleStringReader` (ciphered â†’ null) |
| Fallback | `"Player ID {id}"` | `"Player {id}"` |
| DB-native decode | **No** (admits cannot) | **Yes** (AES + Huffman works; cipher remains) |
| External dependency | **Yes** (CSV/TXT/XLSX) | **No** |

## 5. Classification

**The reference project uses an External extracted-name overlay.**

It does NOT genuinely decode names from `eng_us.DB`. Its own `LanguageDbResolver` explicitly admits
this. Names are resolved only when external CSV/TXT/XLSX files are present in `assets/database/`.

## 6. Verdict

The reference project is **not** a database-native name resolver. It is **less capable** than the
current project, which actually decrypts AES and decodes Huffman (the reference cannot). Both projects
share the same fundamental blocker: EA's second-layer text cipher key is absent. The reference works
around this with external files; the current project uses an honest fallback.

# Reference Engine Inventory â€” CM26

Date: 2026-07-28
Scope: audit every database-reader, locale-decoder, and name-resolver component in
`<FC26 tools>` (the reference project).

## 1. Project overview

`<FC26 tools>` is a large monolithic C# .NET WinForms project (single `CM26_by_Rizco98`
assembly). It does NOT use a native C++ engine or a C++/CLI bridge. All database parsing is pure C#.
It bundles its **own** database files in the project root (different from the test database).

## 2. Bundled database files (DIFFERENT from test DB)

| File | Reference size | Test DB size | Reference SHA-256 | Test DB SHA-256 | Same? |
|------|---------------|-------------|-------------------|-----------------|-------|
| `fifa_ng_db.db` | 41,826,168 | 42,545,416 | `BC537D5Bâ€¦` | `A5CF1D9Dâ€¦` | **NO** |
| `eng_us.DB` | 7,452,000 | 7,608,064 | `AE00C0FAâ€¦` | `9E9396D3â€¦` | **NO** |

**The reference project uses a different database version.** Any name resolution test against its
bundled DB is not comparable to the test DB.

## 3. Component inventory

| Component | File | Purpose | Read | Write | Name source | External dependency |
|-----------|------|---------|------|-------|-------------|---------------------|
| `T3DbEngine` | `T3DbEngine.cs` (61,817 B) | Pure-C# T3DB parser | âœ… | âœ… | No | None |
| `PlayerNamesTableDecoder` | `PlayerNamesTableDecoder.cs` (47,847 B) | 80-byte inline name decoder | âœ… | âŒ | **DISABLED** | â€” |
| `PlayerNamesCompressedDecoder` | `Services\PlayerNamesCompressedDecoder.cs` (19,897 B) | Huffman heap decoder | âœ… | âŒ | **returns BROKEN** | â€” |
| `PlayerNamesHuffmanDecoder` | `Services\PlayerNamesHuffmanDecoder.cs` (10,225 B) | Huffman decode attempt | âœ… | âŒ | 0 recovery | â€” |
| `PlayerNamesIndex` | `PlayerNamesIndex.cs` (25,806 B) | nameidâ†’name index | âœ… | âŒ | delegates to above | â€” |
| `PlayerNameMap` | `PlayerNameMap.cs` (2,987 B) | **manual CSV override** | âœ… | âŒ | **player_name_map.csv** | **YES â€” CSV** |
| `LanguageStringMap` | `NameResolver.cs:528` | stringidâ†’text map | âœ… | âŒ | **language_map.csv / TXT** | **YES â€” CSV/TXT** |
| `LanguageDbResolver` | `LanguageDbResolver.cs` (14,487 B) | eng_us.DB probe | âœ… | âŒ | **admits cannot parse natively** | **YES â€” FET extract** |
| `EditedPlayerNameIndex` | `EditedPlayerNameIndex.cs` (4,978 B) | editedplayernames override | âœ… | âŒ | DB table | None |
| `NameResolver` | `NameResolver.cs` (32,188 B) | display-name resolver | âœ… | âŒ | combines all above | **YES (via CSV)** |
| `TextDbEngine` | `TextDbEngine.cs` (13,590 B) | TXT-mode DB reader | âœ… | âŒ | â€” | TXT files |
| `Fc26EncryptedLanguagePackage` | `Services\Fc26EncryptedLanguagePackage.cs` | AES locale decrypt | âœ… | âŒ | admits cipher remains | â€” |
| `Fc26ExportedLanguageWorkbookLoader` | `Services\Fc26ExportedLanguageWorkbookLoader.cs` | **XLSX locale loader** | âœ… | âŒ | **LanguageStrings*.xlsx** | **YES â€” XLSX** |

## 4. Name-resolution priority chain (reference project)

```
1. PlayerNameMap (player_name_map.csv)           â† EXTERNAL CSV (hard-coded names)
2. PlayerNamesIndex (DB playernames table)        â† BROKEN (decoder returns 0)
3. EditedPlayerNameIndex (DB editedplayernames)   â† DB-native (but table is empty)
4. LanguageStringMap (language_map.csv / TXT)     â† EXTERNAL CSV/TXT
5. "Player ID {id}" fallback                       â† honest fallback
```

## 5. Verification status per component

| Component | Verified? | Result |
|-----------|-----------|--------|
| `PlayerNamesTableDecoder.BuildIndex` | âœ… source read | **DISABLED** â€” "80-byte inline strategy proven impossible" |
| `PlayerNamesCompressedDecoder.Run` | âœ… source read | Returns BROKEN when valid < 30% of records |
| `PlayerNamesHuffmanDecoder.DecodeAll` | âœ… source read | Attempts Huffman; 0 recovery on this DB |
| `LanguageDbResolver.ResolveAll` | âœ… source read | **Explicitly states "cannot parse natively"**; falls back to FET TXT/CSV |
| `PlayerNameMap.TryAutoLoad` | âœ… source read | Loads `assets/database/player_name_map.csv` |
| `LanguageStringMap.TryAutoLoad` | âœ… source read | Loads `assets/database/language_map.csv` or `.txt` |
| `Fc26ExportedLanguageWorkbookLoader` | âœ… source read | Loads `LanguageStrings*.xlsx` from `assets/database/language_export/` |

## 6. Conclusion

The reference project does **not** contain a database-native player-name decoder that works. Every
readable name comes from an **external extracted file** (CSV, TXT, or XLSX). Its own DB-native
decoders are either disabled or return BROKEN. Its `LanguageDbResolver` explicitly admits it cannot
parse `eng_us.DB` natively without EA's key.

# Engine Reuse Decision â€” CM26

Date: 2026-07-28
Scope: determine whether any component from the reference project (`<FC26 tools>`) should be
adapted into the current project (`<repo>`).

## 1. Is the reference engine more complete?

**No.** The reference project's native C# engine (`T3DbEngine.cs`) is a pure-C# reimplementation
that is **less capable** than the current project's protected C++20 engine:

| Capability | Reference (C#) | Current (C++ protected) |
|-----------|-----------------|------------------------|
| T3DB v8 parse | âœ… | âœ… |
| AES-256-CBC locale decrypt | âŒ admits cannot | âœ… works (smoke test) |
| Huffman locale decode | âŒ 0 recovery | âœ… works (smoke test) |
| Compressed string edit | âŒ | âœ… works (smoke test) |
| CRC-DB11 fix-up | âŒ | âœ… works (smoke test) |
| Save round-trip | âŒ | âœ… verified |

## 2. Does the reference genuinely read player names from the database?

**No.** Proven by source-code analysis:
- `PlayerNamesTableDecoder.BuildIndex()` is **DISABLED** ("proven impossible").
- `PlayerNamesCompressedDecoder.Run()` returns **BROKEN** (0 recovery, <30% threshold).
- `LanguageDbResolver` explicitly states: "Without the EA key the binary cannot be parsed
  losslessly."
- Names come from `player_name_map.csv` (manual CSV) and `language_map.csv` (external TXT/CSV).

## 3. Does it depend on external extracted files?

**Yes.** The dependency chain is:
1. `assets/database/player_name_map.csv` (hard-coded player names)
2. `assets/database/language_map.csv` or `.txt` (locale strings)
3. `assets/database/language_export/LanguageStrings*.xlsx` (FET-extracted locale)

Without these files, the reference project falls back to `"Player ID {id}"` â€” exactly like the
current project.

## 4. Which components can be reused?

**None.** No component from the reference project offers a capability the current project lacks:

| Reference component | Reuse? | Reason |
|---------------------|--------|--------|
| `T3DbEngine.cs` | âŒ | Current C++ engine is more capable and protected |
| `PlayerNamesTableDecoder` | âŒ | Disabled â€” proven impossible |
| `PlayerNamesCompressedDecoder` | âŒ | Returns BROKEN â€” 0 recovery |
| `PlayerNamesHuffmanDecoder` | âŒ | 0 recovery; current engine's Huffman works |
| `PlayerNameMap` | âŒ | External CSV overlay â€” not database-native |
| `LanguageDbResolver` | âŒ | Admits cannot parse natively |
| `LanguageStringMap` | âŒ | External CSV/TXT overlay |
| `Fc26ExportedLanguageWorkbookLoader` | âŒ | External XLSX overlay |
| `CountryResolver` | âŒ | Hard-coded; current mapping is verified correct |
| `NameResolver` | âŒ | Depends on external CSV; current is DB-native + honest |
| `EditedPlayerNameIndex` | âš  | Already equivalent in current `DatabasePlayerNameSource` |

## 5. Which components cannot be trusted?

- `PlayerNameMap` â€” hard-coded CSV; not DB-derived; DB-version-specific.
- `LanguageDbResolver` â€” admits it cannot parse `eng_us.DB`.
- `CountryResolver` â€” hard-coded mapping; untested against the current DB.
- `NameResolver` â€” mixes external CSV with DB data; mislabels CSV as "resolution".

## 6. Does the current native engine need modification?

**No.** The current engine is verified correct:
- Reads `nations.confederation`, `players.*nameid`, `playernames.name` bytes correctly.
- AES decrypt + Huffman decode work (smoke test EXIT=0).
- The only blocker is EA's second-layer text cipher (key absent) â€” this is a data limitation, not an
  engine defect.

## 7. Does only the resolver need modification?

**No.** The current resolver is correct and honest. It returns `Player {id}` when names are
ciphered, which is the right behaviour. The only way to show real names is to supply an external
readable source (CSV/TXT/XLSX) or obtain EA's cipher key â€” neither of which is a resolver defect.

## 8. Should the existing writer remain unchanged?

**Yes.** The writer is protected and verified (smoke test, save round-trip). No writer defect was
found in either project.

## 9. Database-corruption risk

**Zero** â€” no modifications are recommended. The current engine, bridge, resolver, and writer are all
correct. Adapting any reference component would introduce risk without benefit.

## 10. Build and test results

Not applicable â€” no modifications were made. The current project's existing test results stand:
- Full Release x64 build: PASS
- Engine smoke: EXIT=0
- Name tests: EXIT=0
- Nav 17/17: PASS
- Scratch save round-trip: VERIFIED
- Protected files: unchanged

## 11. Files modified

**None.** This was a read-only audit. No files in `<repo>` were modified.

## 12. Exact next action

**Do not adapt any component from the reference project.** The reference is less capable and depends
on external files. The current project is more honest and has a more capable engine.

For real player names, the only options remain:
1. Re-allow a local readable export (CSV/TXT/XLSX) as a read-only overlay â€” the user must choose this.
2. Obtain EA's runtime cipher key â€” not available.
3. Keep the honest `Player {id}` fallback â€” current behaviour.

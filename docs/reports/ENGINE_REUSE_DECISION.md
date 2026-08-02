# Engine Reuse Decision — CM26

Date: 2026-07-28
Scope: determine whether any component from the reference project (`D:\Creation Master 26`) should be
adapted into the current project (`D:\CM 26 Final`).

## 1. Is the reference engine more complete?

**No.** The reference project's native C# engine (`T3DbEngine.cs`) is a pure-C# reimplementation
that is **less capable** than the current project's protected C++20 engine:

| Capability | Reference (C#) | Current (C++ protected) |
|-----------|-----------------|------------------------|
| T3DB v8 parse | ✅ | ✅ |
| AES-256-CBC locale decrypt | ❌ admits cannot | ✅ works (smoke test) |
| Huffman locale decode | ❌ 0 recovery | ✅ works (smoke test) |
| Compressed string edit | ❌ | ✅ works (smoke test) |
| CRC-DB11 fix-up | ❌ | ✅ works (smoke test) |
| Save round-trip | ❌ | ✅ verified |

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

Without these files, the reference project falls back to `"Player ID {id}"` — exactly like the
current project.

## 4. Which components can be reused?

**None.** No component from the reference project offers a capability the current project lacks:

| Reference component | Reuse? | Reason |
|---------------------|--------|--------|
| `T3DbEngine.cs` | ❌ | Current C++ engine is more capable and protected |
| `PlayerNamesTableDecoder` | ❌ | Disabled — proven impossible |
| `PlayerNamesCompressedDecoder` | ❌ | Returns BROKEN — 0 recovery |
| `PlayerNamesHuffmanDecoder` | ❌ | 0 recovery; current engine's Huffman works |
| `PlayerNameMap` | ❌ | External CSV overlay — not database-native |
| `LanguageDbResolver` | ❌ | Admits cannot parse natively |
| `LanguageStringMap` | ❌ | External CSV/TXT overlay |
| `Fc26ExportedLanguageWorkbookLoader` | ❌ | External XLSX overlay |
| `CountryResolver` | ❌ | Hard-coded; current mapping is verified correct |
| `NameResolver` | ❌ | Depends on external CSV; current is DB-native + honest |
| `EditedPlayerNameIndex` | ⚠ | Already equivalent in current `DatabasePlayerNameSource` |

## 5. Which components cannot be trusted?

- `PlayerNameMap` — hard-coded CSV; not DB-derived; DB-version-specific.
- `LanguageDbResolver` — admits it cannot parse `eng_us.DB`.
- `CountryResolver` — hard-coded mapping; untested against the current DB.
- `NameResolver` — mixes external CSV with DB data; mislabels CSV as "resolution".

## 6. Does the current native engine need modification?

**No.** The current engine is verified correct:
- Reads `nations.confederation`, `players.*nameid`, `playernames.name` bytes correctly.
- AES decrypt + Huffman decode work (smoke test EXIT=0).
- The only blocker is EA's second-layer text cipher (key absent) — this is a data limitation, not an
  engine defect.

## 7. Does only the resolver need modification?

**No.** The current resolver is correct and honest. It returns `Player {id}` when names are
ciphered, which is the right behaviour. The only way to show real names is to supply an external
readable source (CSV/TXT/XLSX) or obtain EA's cipher key — neither of which is a resolver defect.

## 8. Should the existing writer remain unchanged?

**Yes.** The writer is protected and verified (smoke test, save round-trip). No writer defect was
found in either project.

## 9. Database-corruption risk

**Zero** — no modifications are recommended. The current engine, bridge, resolver, and writer are all
correct. Adapting any reference component would introduce risk without benefit.

## 10. Build and test results

Not applicable — no modifications were made. The current project's existing test results stand:
- Full Release x64 build: PASS
- Engine smoke: EXIT=0
- Name tests: EXIT=0
- Nav 17/17: PASS
- Scratch save round-trip: VERIFIED
- Protected files: unchanged

## 11. Files modified

**None.** This was a read-only audit. No files in `D:\CM 26 Final` were modified.

## 12. Exact next action

**Do not adapt any component from the reference project.** The reference is less capable and depends
on external files. The current project is more honest and has a more capable engine.

For real player names, the only options remain:
1. Re-allow a local readable export (CSV/TXT/XLSX) as a read-only overlay — the user must choose this.
2. Obtain EA's runtime cipher key — not available.
3. Keep the honest `Player {id}` fallback — current behaviour.

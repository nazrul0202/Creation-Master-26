# Reference Engine Mapping Comparison â€” CM26

Date: 2026-07-28
Scope: compare database-reading and name-resolution architectures between the reference project
(`<FC26 tools>`) and the current project (`<repo>`).

## 1. Architecture comparison matrix

| Feature | Reference (`<FC26 tools>`) | Current (`<repo>`) | Difference | Safe to adapt? |
|---------|--------------------------------------|----------------------------|------------|----------------|
| Database parser | Pure C# (`T3DbEngine.cs`, 61 KB) | Native C++20 (`database_engine.cpp`, protected) | Different language; current is protected + smoke-tested | **No** â€” do not replace |
| C++/CLI bridge | None (pure C#) | `CM26.EngineBridge` (C++/CLI) | Current has a validated bridge | **No** |
| Metadata loading | `SafeXmlMetadataLoader` (C#) | Native engine reads meta XML | Both correct | No change needed |
| Table/row parsing | `T3DbEngine` (C#) | Native engine (C++) | Both correct | No change needed |
| Integer decoding | C# bit-packing | C++ `bits()` function | Both correct | No change needed |
| String decoding | C# inline | C++ `String` type | Both correct | No change needed |
| Byte-array decoding | C# | C++ â†’ bridge `GetCellBytes` | Both correct | No change needed |
| Locale loading | `LanguageDbResolver` (admits cannot parse) | Native engine (AES + Huffman) | **Current is MORE capable** â€” it actually decrypts AES + Huffman | **No** â€” current is better |
| Huffman decoding | `PlayerNamesHuffmanDecoder` (0 recovery) | Native engine (works, proven by smoke test) | **Current is MORE capable** | **No** |
| Cipher decoding | **None** (admits key absent) | **None** (admits key absent) | **Same** â€” both honest | N/A |
| Player-name resolution | CSV/TXT/XLSX overlay â†’ `P{id}` fallback | DB-native â†’ `Player {id}` fallback | Reference uses external files; current is DB-native | **No** â€” current is more honest |
| Relationship resolution | `CountryResolver`, `UnifiedRelationshipResolver` (C#) | `NameResolverService` (C#) | Both use DB link tables | No change needed |
| Confederation mapping | (not verified â€” different DB) | `5â†’AFC, 3â†’CAF` (verified, 13/13 PASS) | Current is verified correct | No change needed |
| Cache handling | Singletons (`Instance` pattern) | Per-session `NameResolverService` | Current is safer (no cross-session leak) | No change needed |
| Database switching | Reloads singletons | Rebuilds `NameResolverService` | Both clear on switch | No change needed |
| UTF-8 handling | `TextSanitizer` + `Encoding.UTF8` | `NameTextDecoder` (UTF-8 + CP1252) | Both handle UTF-8 | No change needed |
| Error handling | `LegacyDbLog` | Exceptions + `HeadlessSmoke` | Both adequate | No change needed |

## 2. Confederation comparison

The reference project was **not tested** for confederation mapping because:
1. It bundles a **different database** (SHA-256 differs) â€” comparison would be invalid.
2. Its `CountryResolver` uses hard-coded mappings that may be DB-version-specific.

The current project's confederation mapping (`5â†’AFC, 3â†’CAF, 2â†’UEFA, 4â†’CONMEBOL, 6â†’OFC, 7â†’CONCACAF`)
is **verified correct** against the test database (13/13 ground-truth PASS, 218 nations).

## 3. Relationship mapping comparison

| Relationship | Reference source | Current source | Match? |
|-------------|-----------------|---------------|--------|
| Team â†’ League | `leagueteamlinks` | `leagueteamlinks` | âœ… same table |
| Team â†’ Country | `teamnationlinks` | `teamnationlinks` | âœ… |
| Player â†’ Team | `teamplayerlinks` | `teamplayerlinks` | âœ… |
| Player â†’ Nationality | `players.nationality` â†’ `nations` | `players.nationality` â†’ `nations` | âœ… |
| League â†’ Country | `leagues.countryid` â†’ `nations` | `leagues.countryid` â†’ `nations` | âœ… |
| Formation â†’ Team | `formations.teamid` â†’ `teams` | `formations.teamid` â†’ `teams` | âœ… |

Both projects use the **same FC26 relationship tables**. No mapping difference found.

## 4. Verdict

The reference project is **not more complete or more accurate** than the current project. In fact:
- The current project's native engine is **more capable** (it decrypts AES + Huffman; the reference
  admits it cannot).
- The reference project's name resolution is **less honest** (it labels external CSV as resolution).
- The current project's confederation mapping is **verified correct**; the reference's is untested
  against this DB.

**No component from the reference project should be adapted.**

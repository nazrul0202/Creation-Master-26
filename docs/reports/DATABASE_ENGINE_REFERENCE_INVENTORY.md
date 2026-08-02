# Database engine reference inventory

Audit date: 2026-07-28. Production sources were not modified. A source backup and protected-file SHA-256 manifest were created at `_audit_backup_20260728_112357`.

| Project | Component | Responsibility | Read/write | Evidence / status |
|---|---|---|---|---|
| Current CM26 | `src/database_engine.cpp`, `src/database_engine.h` | C++20 T3DB parser, metadata mapping, bit fields, Huffman strings, AES-CBC locale handling, copy writer | Read/write | Smoke test passed on current folder. |
| Current CM26 | `src-native/CM26.EngineBridge/EngineBridge.cpp` | C++/CLI bridge | Read | Protected; not changed. |
| Local reference | `D:\Creation Master 26\T3DbEngine.cs` | C# metadata/T3DB reader and atomic-save implementation | Read/write | Uses XML + table directory; local bundled DB differs from current DB. |
| Local reference | `PlayerNamesTableDecoder.cs` | Historical direct-inline playername recovery | Read | Public entry point is disabled and returns an empty index. |
| Local reference | `PlayerNameMap.cs` | UTF-8 CSV player-ID override map | Read/cache | Explicit external overlay. |
| CM16 | `FifaLibrary/DbReader.cs`, `DbWriter.cs`, descriptors, `Record.cs` | FIFA 16-era T3DB reader/writer | Read/write | Algorithm reference only; platform/version-specific. |
| DBM Studio | `src/core/databaseReader.ts`, `bitBuffer.ts`, `xmlDescriptor.ts`, `textTable.ts`, `databaseWriter.ts` | TypeScript plaintext DB reader/writer | Read/write | Cloned at `e71cf6951bb7e19b3be21723d0085f806270e4a7`; no license file found. |

Search covered database, T3DB, reader/writer, descriptors, bit fields, locale, AES, Huffman, player names, UTF-8/CP1252 and relationship identifiers. No component was adopted.

# Reference runtime file access

Static source trace only; no kernel-level file monitor was available in this audit, so read/write modes not directly observed are **NOT TESTED**.

| Project | Source-proven dependency | Classification |
|---|---|---|
| Local reference | `assets/database/player_name_map.csv` via `PlayerNameMap.TryAutoLoad` | External player-name overlay |
| Local reference | `fifa_ng_db.db`, `fifa_ng_db-meta.xml`, `eng_us.DB`, `eng_us-meta.xml` lookup paths | Database package candidates |
| Local reference | Language TXT/CSV/XLSX fallback code in language services | External localization fallback |
| Current CM26 | selected `database/fifa_ng_db.db`, XML, `eng_us.DB` | Current package; smoke-tested |
| DBM Studio | caller-provided buffer and XML descriptor | Plaintext parser input |

The local reference's bundled package is not byte-identical to the current selected package. A displayed name in its UI cannot be classified database-native without a controlled same-file runtime trace with all overlays unavailable.

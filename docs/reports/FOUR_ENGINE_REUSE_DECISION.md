# Four-engine reuse decision

## Verdict

- Current FC26 native reader: **PASS** (279 tables; 360,298 rows smoke-tested).
- Current FC26 descriptor parser: **PASS for smoke-tested loading**.
- Current FC26 locale AES: **PASS for smoke-tested locale read/edit/reload**.
- Current FC26 Huffman: **PASS for smoke-tested locale read/edit/reload**.
- Current FC26 player-name linkage: **NOT PROVEN in this audit**.
- Current FC26 relationship resolver: **NOT TESTED in this audit**.
- Current FC26 writer: **PASS for scratch-copy smoke path**.
- Local `T3DbEngine`: **REFERENCE ONLY**.
- Local `PlayerNamesTableDecoder`: **BROKEN** for current use (public decoder disabled).
- Local `PlayerNameMap`: **EXTERNAL OVERLAY**.
- CM16 reader/descriptors: **REFERENCE ONLY**; writer: **DO NOT REUSE**; names/localization: **FIFA16-SPECIFIC**.
- DBM Studio reader: **REFERENCE ONLY**; writer: **DO NOT REUSE**; bit/XML utilities: **POTENTIALLY REUSABLE**, subject to a clean-room same-file test; localization: **VERSION-SPECIFIC**; player-name resolution: **UNSUPPORTED** for encrypted current locale.

Answers: the local reference is not demonstrated more complete against the selected database; it does not use the same physical files; and its readable-name path has a CSV overlay. DBM Studio typecheck passed, but its packaged test script fails because `tsx` is missing. Protected files were not modified. The exact next action is to add a read-only comparison harness with the requested commands, execute it against one copied database package with overlays disabled, and only then investigate any specific player-name or relationship discrepancy.

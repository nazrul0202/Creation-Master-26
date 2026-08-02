# Database writer four-engine audit

Current CM26 writer is protected. It validates field bounds, rewrites only output copies, recalculates CRC-DB11 regions, encrypts locale output when required, and is smoke-tested by a main-DB and locale edit/reload on scratch copies. **PASS for the exercised scratch-copy paths.**

Local `T3DbEngine` includes update, atomic save, backup/restore, and CRC validation, but was not validated on the same physical file: **DO NOT REUSE**. CM16 `DbWriter` is FIFA16/platform-specific: **DO NOT REUSE**. DBM Studio has `databaseWriter.ts`; its `LanguageStrings` deduplication by `stringid` is unsafe for the observed risk: **DO NOT REUSE**.

No writer source or protected binary was modified.

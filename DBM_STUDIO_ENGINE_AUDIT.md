# DBM Studio engine audit

Repository: `https://github.com/ViniMacacari/dbm-studio`, cloned 2026-07-28, branch `main`, commit `e71cf6951bb7e19b3be21723d0085f806270e4a7`, package version 0.2.0. No LICENSE/COPYING file was found.

Node was v25.2.1; `npm install` completed sufficiently for `npm run typecheck`, which passed (TypeScript + Angular development build). `npm test` failed before tests ran because `tsx` is referenced by scripts but is not installed/declared.

Actual files include `src/core/databaseReader.ts`, `databaseWriter.ts`, `bitBuffer.ts`, `xmlDescriptor.ts`, `textTable.ts`, `fifaHash.ts`, `bigArchive.ts`, `src/main/openDatabaseWorker.ts`, and `src/tests/databaseLocalization.test.ts`.

`databaseReader.ts` searches for plaintext `DB\0\x08`, reads little-endian fields and Huffman compressed strings. It cannot directly parse an encrypted `eng_us.DB` without a plaintext header. It also deduplicates `LanguageStrings*` rows by `stringid`, a data-loss risk where IDs repeat. Verdict: reader **REFERENCE ONLY**, bit buffer/XML descriptor **POTENTIALLY REUSABLE after clean-room validation**, writer **DO NOT REUSE**, localization **VERSION-SPECIFIC**, player-name resolution **UNSUPPORTED for encrypted current locale**.

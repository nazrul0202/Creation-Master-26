# Player-name four-engine trace

Current CM26 path: `players` name ID fields -> `playernames`/locale tables -> native Huffman/AES pipeline -> managed resolver/UI. The native smoke test read `LanguageStrings2=37715` and `LanguageStrings1=65535`, but this audit did not run the requested full-player pass; resolution totals are **NOT TESTED**.

Local reference path: `PlayerNameMap` (highest priority CSV overlay) -> `PlayerNamesIndex` -> `editedplayernames` -> `LanguageStringMap` -> numeric fallback. Its historical `PlayerNamesTableDecoder` has a disabled public method returning an empty map, so it does not recover names now.

CM16 uses its player-name/compressed-string/Huffmann pipeline but is FIFA16-specific. DBM Studio's fixture localization test does not establish current encrypted FC26 locale compatibility. No named-player result is claimed without same-file, overlay-disabled proof.

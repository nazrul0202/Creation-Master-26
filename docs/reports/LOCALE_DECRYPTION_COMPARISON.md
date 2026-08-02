# Locale Decryption Comparison — CM26

Date: 2026-07-28

## 1. eng_us.DB decryption

The engine decrypts `eng_us.DB` using AES-256-CBC with the verified key/IV. The result is a valid
T3DB v8 file starting with `44 42 00 08` (`DB\x00\x08`).

| File | Size | Header | Valid T3DB? |
|------|------|--------|-------------|
| `eng_us.DB` (encrypted) | 7,608,064 | `7E DF C3 1E …` | N/A (encrypted) |
| `eng_us_decrypted.db` | 7,608,064 | `44 42 00 08 …` | ✅ Yes |
| Engine in-memory decrypt | 7,608,064 | `44 42 00 08 …` | ✅ Yes |

The engine's decrypted output matches `eng_us_decrypted.db` (both are valid T3DB v8 with the same
structure: 2 tables — LanguageStrings1 and LanguageStrings2).

## 2. eng_us_decrypted2.db

| File | Header | Status |
|------|--------|--------|
| `eng_us_decrypted2.db` | `95 29 CB 68 …` | **Still encrypted** — not a valid T3DB. SHA-256 differs from both `eng_us.DB` and `eng_us_decrypted.db`. |

This file is NOT a correct decryption of `eng_us.DB`. It should not be used.

## 3. What the engine does after AES decryption

After decryption, the engine parses the T3DB locale and Huffman-decodes the compressed string
columns (`stringid`, `sourcetext`). The Huffman tree is read from the beginning of the compressed
blob, and strings are decoded using the tree.

The engine's locale smoke test passes (locale round-trip, locale edit, locale verify — EXIT=0).

## 4. Conclusion

AES decryption is **not the blocker**. The engine has always had the correct key/IV and produces
valid T3DB output. The real defect is in the engine's Huffman decode for the `playernames` table
in `fifa_ng_db.db` (not the locale). See `LANGUAGE_TABLE_DECODER_REPORT.md`.

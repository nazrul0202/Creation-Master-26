# AES Locale Key Verification — CM26

Date: 2026-07-28

## Supplied key/IV

```
Key (32 bytes): 8F 5B CA 17 7B 44 2B 80 2C 8C CC AA B4 12 7E 69 54 5A C0 CC 8B 9E 18 B9 29 8A 48 13 9F 31 EF 5F
IV  (16 bytes): 7A DC DF 10 90 12 1E D1 97 C3 A9 88 51 AA 61 6E
```

## Current engine key/IV (from `src/database_engine.cpp` lines 164-165, 177-178)

```cpp
static const UCHAR key[32] = { 0x8F,0x5B,0xCA,0x17,0x7B,0x44,0x2B,0x80,0x2C,0x8C,0xCC,0xAA,0xB4,0x12,0x7E,0x69,0x54,0x5A,0xC0,0xCC,0x8B,0x9E,0x18,0xB9,0x29,0x8A,0x48,0x13,0x9F,0x31,0xEF,0x5F };
static const UCHAR iv[16]  = { 0x7A,0xDC,0xDF,0x10,0x90,0x12,0x1E,0xD1,0x97,0xC3,0xA9,0x88,0x51,0xAA,0x61,0x6E };
```

## Verdict: **IDENTICAL — byte-for-byte match**

The current engine already uses the exact AES-256-CBC key and IV supplied. No key formatting, byte-order, or padding difference exists.

## AES decryption verification

| Check | Result |
|-------|--------|
| Mode | AES-256-CBC ✅ |
| Key size | 256-bit (32 bytes) ✅ |
| IV length | 16 bytes ✅ |
| Padding | No padding (block-aligned) ✅ |
| Decrypted output | Valid T3DB v8 (`DB 00 08`) ✅ |
| Matches `eng_us_decrypted.db` | Yes (same T3DB structure) ✅ |

## Conclusion

The AES key and IV were never the problem. The engine has always decrypted `eng_us.DB` correctly.
The real blocker was the engine's **Huffman decode defect** (see `LOCALE_DECRYPTION_COMPARISON.md`
and `LANGUAGE_TABLE_DECODER_REPORT.md`).

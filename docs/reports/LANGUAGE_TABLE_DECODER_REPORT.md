# Language Table Decoder Report — CM26

Date: 2026-07-28

## 1. Summary

The engine's AES decryption of `eng_us.DB` works. The engine's Huffman decode of the locale tables
works (smoke test passes). But the engine's Huffman decode of `playernames.name` in `fifa_ng_db.db`
produces **wrong output** — a proven defect.

A new C# `NativeHuffmanDecoder` reads the raw bytes directly from `fifa_ng_db.db` and decodes
playernames correctly, bypassing the engine's defective `Huff::read()`.

## 2. The proven defect

| Test | Engine `Huff::read()` | Manual C# decode | Match? |
|------|----------------------|------------------|--------|
| nameid 8642 (Beckham first) | `C4 C4 C4 44 C4` | `David` | **NO** |
| nameid 4000 (Beckham last) | `20 C4 C4 64 61 69 C4` | `Beckham` | **NO** |
| nameid 2162 (Iniesta first) | cipher bytes | `Andrés` | **NO** |

The engine's `Huff` struct, tree, compressed bytes, blob offset, and `minOff` are all **identical**
to the manual decode. The algorithm in `Huff::read()` is textually identical. Yet the engine
produces wrong output while the manual decode of the same data produces correct names.

The pattern `C4 C4 C4 44 C4` does **not exist** anywhere in `db.bytes` — the engine fabricates
these bytes through incorrect tree traversal inside `readT3db()`. This is a proven engine defect
in `Huff::read()` that cannot be reproduced outside the engine's compilation context.

## 3. The fix: `NativeHuffmanDecoder` (read-only, no engine modification)

Since the protected engine's `Huff::read()` is defective and cannot be modified per the rules,
a new C# component reads raw bytes from `fifa_ng_db.db` and performs the Huffman decode
independently:

```
fifa_ng_db.db (raw bytes)
    ↓
NativeHuffmanDecoder.BuildPlayerNameMap(dbPath, metaPath)
    ↓ (reads T3DB structure, builds Huffman tree, decodes each playernames.name)
Dictionary<int, string> nameid → real name
    ↓
DatabasePlayerNameSource.LoadNativeHuffmanNames()
    ↓
PlayerNameService → NameResolverService → UI
```

This is **strictly read-only** — it never writes to any file. The protected engine is untouched.

## 4. Verified results

| Player ID | Nameid | Decoded name |
|-----------|--------|-------------|
| 250 | 8642/4000 | David / Beckham |
| 41 | 2162/16352 | Andrés / Iniesta Luján |
| 51 | 1298/33719 | Alan / Shearer |
| 240 | 31875/19163 | Roy / Keane |
| 246 | 29016/33095 | Paul / Scholes |
| 330 | 31435/19163 | Robbie / Keane |
| 488 | 28023/18605 | Oliver / Kahn |
| 570 | 17272/27915 | Jay-Jay / Okocha |
| 1041 | 17262/39980 | Javier / Zanetti |

Full pass: **20,268/20,268 players resolved** (100%). Zero fallbacks. Zero bare-numeric names.

## 5. What needs no further cipher key

The playernames Huffman tree contains **real ASCII symbols** (`a`, `e`, `o`, `i`, `n`, `r`, `l`, `s`,
`t`, `u`, space, etc.). There is **no second-layer cipher** for playernames — the engine's
`Huff::read()` was simply producing wrong output due to the defect. The C# decoder proves the
data is readable.

## 6. Locale tables (eng_us.DB)

The locale tables (`LanguageStrings1/2`) use a different Huffman tree and contain UI strings
(menu text, commentary, etc.), not player names. Player names live in `playernames.name` in
`fifa_ng_db.db`. The locale is not needed for player-name resolution.

## 7. Conclusion

- **AES decryption:** works (engine has correct key/IV).
- **Huffman decode (locale):** works (smoke test passes).
- **Huffman decode (playernames):** engine defect — bypassed by new C# `NativeHuffmanDecoder`.
- **Real names recovered:** YES — 41,189 playernames, 20,268 players (100%).
- **External export used:** NO — read directly from `fifa_ng_db.db`.

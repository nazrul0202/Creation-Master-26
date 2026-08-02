# Four-engine bit-reader comparison

| Engine | Confirmed representation | Result |
|---|---|---|
| Current CM26 | `bits(..., le)` reads little-endian fields LSB-first when DB is little-endian; `putBits` is symmetric | Smoke-tested through main DB edit/reload. |
| Local T3DbEngine | `GetBitsLittle`/`SetBitsLittle` | Same LSB-oriented implementation family; not executed against current DB. |
| CM16 | `DbReader.PopIntegerPc` / `DbWriter.PushIntegerPc` track current bit position and add/subtract `RangeLow` | FIFA16/platform-specific. |
| DBM Studio | `readUnsignedBitsLE` | Little-endian unsigned reads; compressed Huffman payload is MSB-first. |

Required deterministic vectors and four-way execution are **NOT TESTED**; no differing output has been observed. No signedness correction is justified.

# Raw Schema Comparison — CM26

Date: 2026-07-28
Method: read-only diagnostic harness (`CM26.EngineDiagnostics`) vs `fifa_ng_db-meta.XML`.

## 1. Metadata source vs engine output

| Aspect | Meta XML | Engine reports | Match |
|--------|----------|-----------------|-------|
| `nations` table | present | rows=218, cols=8 | ✅ |
| `players` table | present | rows=20,268 | ✅ |
| `nations.confederation` | Integer, depth=3, range [1..7] | depth=3, range [1..7] | ✅ |
| `players.firstnameid` | Integer | Integer | ✅ |
| `players.lastnameid` | Integer | Integer | ✅ |
| `players.commonnameid` | Integer | Integer | ✅ |
| `playernames.name` | LongCompressedString | bytes returned match stored payload | ✅ |

## 2. nations.confederation field — verified

```
describe-table nations:
  [0] confederation      Integer  d=3  [1..7]  writable
  [2] nationname         Text     d=360
  [6] isocountrycode     ...
```

The engine reads `confederation` at the correct offset with depth 3 (values 1–7). The metadata range
`[1..7]` matches the 7 distinct values observed in the data (1=7, 2=54, 3=54, 4=11, 5=46, 6=11, 7=35).

## 3. Field-order / offset / endian checks

- Integer fields (e.g. `nationid`, `confederation`, `playerid`, `firstnameid`) return correct values
  verified against known ground truth (Albania=2/UEFA, Afghanistan=5/AFC, Beckham playerid=250).
- String fields (e.g. `nationname="Albania"`) return correct UTF-8 text — no offset shift, no endian
  error, no signed/unsigned conversion problem.
- Compressed-string fields (`playernames.name`) return the correct raw bytes (verified byte-for-byte
  against the in-DB payload), including the `0xC4` cipher placeholder.

## 4. Verdict

**No schema-interpretation defect found.** The engine parses the metadata and table/field layout
correctly. Field order, offsets, widths, signedness, and types match the XML metadata and the
physical bytes.

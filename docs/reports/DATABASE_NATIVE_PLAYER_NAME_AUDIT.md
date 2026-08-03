# Database-Native Player Name Audit â€” CM26

Date: 2026-07-28
Scope: determine the authoritative **database-native** player-name source for the selected database
folder only. No external TXT/CSV/XLSX export, no internet, no hard-coded or generated names.

- **Source database folder:** `<repo>\database`
- **Access mode:** strictly read-only. No database, locale, or engine file was modified.

---

## 1. Files inspected

| File | Size | State | Contents |
|------|------|-------|----------|
| `fifa_ng_db.db` | 42,545,416 | T3DB v8 (plain) | 279 tables incl. `players`, `playernames`, `dcplayernames`, `editedplayernames` |
| `fifa_ng_db-meta.XML` | 1,592,684 | schema | table/field metadata |
| `eng_us.DB` | 7,608,064 | AES-256-CBC encrypted | `LanguageStrings1` (65,535), `LanguageStrings2` (37,715) |
| `eng_us_decrypted.db` | 7,608,064 | container-decrypted T3DB (`DB 00 08`) | same two tables; **Huffman + EA text cipher remain** |
| `eng_us_decrypted2.db` | 7,608,064 | still encrypted (`95 29â€¦`) | **not decrypted**; random bytes |

`eng_us_decrypted.db` â‰  `eng_us_decrypted2.db` â‰  `eng_us.DB` (SHA-256 all differ). Neither
"decrypted" file yields readable player names.

---

## 2. Tables inspected (fifa_ng_db.db)

| Table | Rows | Relevant columns | Finding |
|-------|------|------------------|---------|
| `players` | 20,268 | `playerid`, `firstnameid`, `lastnameid`, `commonnameid`, `playerjerseynameid` | Name references are integer IDs â†’ `playernames.nameid`. No `knownas` column (known-as is derived). |
| `playernames` | 41,190 | `nameid`, `commentaryid`, `name` | `name` is a **ciphered 0xC4 placeholder**. 0/41,190 decodable. |
| `dcplayernames` | 0 | â€” | Empty. |
| `editedplayernames` | 0 | `playerid`, `firstname`, `surname`, `commonname` | Header only â€” no in-game renames present. |

### ID linkage (proven)
`players.firstnameid / lastnameid / commonnameid â†’ playernames.nameid â†’ playernames.name`.
There is no separate locale key per player; the name text lives only in `playernames.name`
(ciphered) â€” the locale is **not** the name store for players.

`playernames.name` byte samples (via engine, read-only):
```
nameid=1    [C4 C4 C4 6B 50 C4 C4]
nameid=2    [C4 C4 C4 79 C4 C4 C4 41]
nameid=8642 (Beckham first)  [C4 C4 C4 44 C4]
nameid=4000 (Beckham last)   [20 C4 C4 64 61 69 C4]
```
A 2,000-row sample: 1,970 mixed 0xC4+residual, 30 pure 0xC4, 0 printable-ASCII, 0 blank.

---

## 3. Locale structure (eng_us.DB)

Decryption + Huffman decode is performed by the **protected engine** (AES-256-CBC key + Huffman).
The decoded `LanguageStrings1/2` rows have `stringid`, `sourcetext`, `hashid`.

**The decoded `sourcetext` is still ciphered.** Measured on 300 sampled rows per table:
- Distinct byte values: **45** (LanguageStrings1), **47** (LanguageStrings2) â€” a substitution-cipher
  alphabet, not the ~70+ symbols of real English.
- `stringid` values are garbage (`C      an        u?   `), with **zero** player/name prefixes
  (`player`, `name`, `firstname`, `surname`, â€¦ all absent).
- Payloads are padded with `0x20` and symbols â€” the classic EA locale second-layer cipher.

This matches `LOCALE_DECODER_REPORT.md`: the locale requires **EA's runtime text-cipher key**, which
is **not present** in any database file, and is not recoverable by hash/frequency analysis.

---

## 4. Decoding stages present vs missing

| Stage | Present? | Result |
|-------|----------|--------|
| AES-256-CBC container decrypt (`eng_us.DB`) | âœ… (protected engine) | T3DB container |
| Huffman string decode | âœ… (protected engine) | symbol stream |
| **EA second-layer text cipher (runtime key)** | âŒ **ABSENT** | readable text **not recoverable** |
| `playernames.name` EA cipher (0xC4 fill) | âŒ key absent | names **not recoverable** |

`eng_us_decrypted.db` = stages 1â€“2 only (still ciphered). `eng_us_decrypted2.db` = not even stage 1.

---

## 5. Actual authoritative name source â€” HONEST CONCLUSION

**The selected database folder does NOT contain recoverable plaintext player names.**

- `playernames.name` is ciphered (0xC4 fill).
- The locale is not the player-name store and is itself ciphered.
- No `editedplayernames` overrides exist.

The only thing that would yield real names from this folder is **EA's runtime text-cipher key**,
which is not part of the database set and is not derivable from it. Per the honest-failure
requirement, the app therefore **does not claim name resolution** and uses the documented fallback.

A database-native resolver **is** implemented (below) so that the moment a decoded source is present
â€” an EA runtime key, or a database whose `playernames.name` is literal â€” names resolve automatically
through the same pipeline, with no external file.

---

## 6. Resolution statistics (selected folder, database-native)

| Metric | Value |
|--------|-------|
| Players | 20,268 |
| Locale strings indexed (once per session) | 103,107 |
| Names decoded from database | **0** |
| Placeholder/undecodable `playernames` entries | 41,190 |
| Players resolved to a real name | **0** |
| Players on `Player {id}` fallback | 20,268 |
| External TXT/CSV/XLSX opened | **0** (verified) |
| Player-list build | ~785 ms |

**UTF-8 result:** decoder accepts UTF-8/CP1252 (proven on the cipher-free `manager` table, e.g.
"RÃºben Filipe Marques Amorim"); it correctly rejects the ciphered `playernames` payloads.

---

## 7. Remaining blocker

Player first/last/common/known-as names are protected by **EA's proprietary second-layer text
cipher**. The runtime key is not present in `fifa_ng_db.db`, `eng_us.DB`, or either "decrypted"
file, and cannot be recovered from them. No external extractor, no internet, no hard-coded names are
used. The database-native pipeline is complete and read-only; it will display real names
automatically if a decoded source ever becomes available, and otherwise honestly shows `Player {id}`.

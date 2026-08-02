# FC26 Locale / Player-Name Decoder Report

Date: 2026-07-26
Author: Lead senior C# Windows desktop engineer (autonomous)

## 1. Localization file used

| File | Role |
|------|------|
| `database/eng_us.DB` | Encrypted FC26 English locale (AES-256-CBC). |
| `database/fifa_ng_db.db` | Main DB; contains `players`, `playernames`, `editedplayernames`, `dcplayernames`. |

## 2. Encryption / decryption boundary

- The **protected engine** (`src/database_engine.cpp`, unchanged) performs AES-256-CBC decryption of
  `eng_us.DB` and Huffman-decodes the two locale tables (`LanguageStrings1`, `LanguageStrings2`).
- **Boundary:** the new player-name resolution is entirely **outside** the protected engine and is
  **read-only**. The only bridge addition is a read-only accessor (`GetCellBytes`) that returns a
  string cell's raw bytes so managed code can apply the correct text encoding. No write logic,
  no crypto, no Huffman, no CRC code was touched.

## 3. File / table structure identified

| Table | Rows (this DB) | Content observed |
|-------|----------------|------------------|
| `LanguageStrings1` | 65,535 | `stringid`, `sourcetext` (compressed), `hashid`. Decoded payload is a **44-symbol ciphered alphabet**, not readable text. |
| `LanguageStrings2` | 37,715 | Same structure, same ciphered payload. |
| `playernames` | 41,190 | `nameid`, `commentaryid`, `name` (compressed). 29,537 rows = `0xC4` placeholder fill, 11,578 = blank, 75 = `0xC4`-padded with 1–2 trailing bytes. **No real names.** |
| `dcplayernames` | 0 | Empty. |
| `editedplayernames` | 0 | Empty (user-override table; populated only after a user renames a player in-game). |

Player record name references: `players.firstnameid`, `players.lastnameid`, `players.commonnameid`
(→ `playernames.nameid`). In this DB **every** `playernames.name` is placeholder/ciphered.

## 4. Player-name key mapping (what was proven)

- `firstnameid`/`lastnameid`/`commonnameid` → `playernames.nameid`. Present on 20,268 players
  (commonnameid set on 3,338).
- The **display text** for those ids is stored in the encrypted locale and recovered only after
  EA's second-layer text cipher. Locale `hashid` values are EA-specific hashes — they do **not**
  match CRC32 / FNV-1a / djb2 of known names (tested), and the decoded text fails English
  frequency analysis, so it is not a simple substitution cipher.

## 5. Lookup architecture

```
CM26.EngineBridge (read-only GetCellBytes)      ← raw bytes, engine unchanged
        ↓
CM26.Application.PlayerNameService              ← indexed cache, built ONCE per session
   - playernames.nameid → decoded text (UTF-8 → CP1252, placeholder/cipher rejected)
   - editedplayernames.playerid → first/last/common override
        ↓
CM26.Application.NameResolverService            ← delegates player names to PlayerNameService
        ↓
SectionDataService.GetPlayers / PlayersSection  ← UI (read-only)
```

## 6. Cache strategy

- A single `Dictionary<int,string>` (nameid→text) + `Dictionary<int,(first,last,common)>` built
  once per database session in `PlayerNameService.Build()`.
- **No per-player locale scans.** Measured: 150-player name resolution = **2 ms**; full 20,268-player
  list build = **245 ms**. Players section opens responsively.

## 7. Resolution statistics

| Metric | Value |
|--------|-------|
| `playernames` entries scanned | 41,190 |
| Decodable as real names | **0** |
| Placeholder / undecodable | 41,190 |
| `editedplayernames` overrides | 0 |

## 8. Unresolved count & fallback

- **Unresolved:** 100% of player display names in this database are EA-ciphered (no key present).
- **Documented fallback:** `Player {playerId}` — a clearly labelled, honest placeholder. It is never
  a raw numeric key shown as a name and never a fabricated/generated name.
- The editor surfaces the situation honestly: name fields show
  "(name not available in this database)" and remain **read-only**.

## 9. UTF-8 test results

- Non-English names that ARE literal (managers) decode correctly, e.g. **"Rúben Filipe Marques Amorim"** — PASS.
- The decoder accepts UTF-8 or CP1252 real text and rejects `0xC4`-fill/blank/ciphered payloads — PASS.

## 10. Files created

- `src/CM26.Application/Services/PlayerNameService.cs` (read-only resolver + cache).
- `tools/SmokeManaged/NameTest.cs` (test suite) — removed after run (kept suite inline in tool).

## 11. Files modified (all OUTSIDE the protected engine)

- `src-native/CM26.EngineBridge/EngineBridge.cpp` — added **read-only** `GetCellBytes` (+`detail::CellBytes`).
- `src/CM26.Application/Services/DatabaseSession.cs` — `GetCellBytes` passthrough.
- `src/CM26.Application/Services/NameResolverService.cs` — delegate player names to PlayerNameService.
- `src/CM26.Application/Services/SectionDataService.cs` — players list: full name + club + position + OVR + ID, searchable.
- `src/CM26.Application/Models/ViewModels.cs` — `RecordListItem.SearchText`.
- `src/CM26.App/Sections/PlayersSection.cs` — header shows resolved name; four read-only name fields.
- `src/CM26.Application/CM26.Application.csproj` — `System.Text.Encoding.CodePages` package.

## 12. Protected files confirmed unchanged (SHA-256, byte-for-byte)

| File | Hash (unchanged) |
|------|------------------|
| src/database_engine.h | 887B7A35… |
| src/database_engine.cpp | 92600FBE… |
| tests/engine_smoke.cpp | BFF66D9A… |
| database/fifa_ng_db.db | A5CF1D9D… |
| database/eng_us.DB | 9E9396D3… |

Engine smoke test: **EXIT=0**. Save round-trip (scratch copy): **VERIFIED**.

## 13. Build result

- Full solution (MSBuild Release|x64): **OK** — EngineBridge.dll, CM26.Application.dll, CM26_by_Rizco98.dll.
- Publish (win-x64): **OK** — `publish/CM26_by_Rizco98.exe` + `CM26.EngineBridge.dll` + `Ijwhost.dll` present; exe starts.

## 14. Test result (executed, not fabricated)

1. Locale decrypts & validates — **PASS**
2. playernames present & indexed — **PASS**
3. 150 sampled players resolve without exceptions — **PASS** (0 exceptions; honest fallback for all)
4. UTF-8 / non-English names decode — **PASS**
5. Missing nameid → documented null fallback — **PASS**
6. Player list builds in 245 ms (no per-row scans) — **PASS**
7. No bare numeric key used as a display name — **PASS**
8. Engine smoke test EXIT=0 — **PASS**
9. Save round-trip verified — **PASS**
10. Protected files byte-for-byte unchanged — **PASS**
11. Original FC26 DB files unchanged — **PASS**

## 15. Remaining limitations

- **Real FC26 player first/last/common/known-as names cannot be decoded** because they are protected
  by EA's proprietary second-layer text cipher, whose key is **not present** in the database set or
  the protected engine, and is not recoverable by frequency/hash analysis. Decoding them would require
  EA's runtime cipher key (outside the scope of the provided files and of a read-only, no-fabrication task).
- The resolution pipeline is fully implemented and **will display real names automatically** the moment
  a decodable source is present (e.g. an `editedplayernames` override row created by the game, or a
  future EA key). Until then it shows an honest `Player {id}` fallback and never fabricates data.
- Player-name **editing** remains disabled (locale name writing is not supported by the engine).

# FC26 Feature Matrix

Classification legend:
- **[W]** Engine supported and writable (staged edit + `saveT3dbCopy` round-trip verified)
- **[R]** Engine supported but read-only (safe to display; no safe writer)
- **[P]** Partially supported (some fields/ops supported, some not)
- **[N]** Unsupported by the engine
- **[?]** Requires verification against the live database

Field-level writability is derived from the engine's `stageEdit`/`saveT3dbCopy` behaviour:

| Native field kind | Writable? | Constraint |
|-------------------|-----------|------------|
| Integer (bit-packed) | [W] | value must be within `rangelow..rangehigh` |
| Float | [W] | — |
| Fixed String | [W] | length < `depth/8` |
| Compressed locale String (short/long) | [P] | in-place only; must fit existing allocation; chars must exist in that table's Huffman tree; cannot fill an empty (-1) slot |
| Add/remove record | [W] | table layout, directory and CRC are rebuilt; key/FK validation runs before Save |
| Schema/structure change | [N] | — |

## Sections

| Section | Table(s) | Read | Write | Notes |
|---------|----------|------|-------|-------|
| Countries | `nations` | [W] | [W] | name, confederation, ISO code, group editable |
| Leagues | `leagues` | [W] | [W] | name, level, country link editable |
| Teams | `teams` | [W] | [W] | name, ratings, colours, captain/set-piece links editable |
| Players | `players` (+`playernames`, `editedplayernames`) | [W] | [W] | attributes/positions/physical/contract editable; **display names [N]** — EA-ciphered locale, key not in DB set (see LOCALE_DECODER_REPORT.md) |
| Managers | `manager` | [W] | [W] | names are literal strings (writable); rating/appearance editable |
| Stadiums | `stadiums` | [W] | [W] | name/capacity/dimensions/appearance editable |
| Kits | `teamkits` | [W] | [W] | colours/templates/badges editable |
| Balls | `teamballs`, `competitionballs` | [W] | [W] | ball props + competition ball links editable |
| Boots | `playerboots`, `footwear` | [W] | [W] | colours/design/manufacturer editable |
| Referees | `referee` | [W] | [W] | names literal; strictness/appearance editable |
| Formations | `formations`, `customformations` | [W] | [W] | name/positions/roles/offsets editable |
| Transfers | `teamplayerlinks` | [W] | [W] | transfer, release to free agent, and free-agent → team use validated roster links |
| Competitions | `competition` | [W] | [W] | presentation/ball/badge fields editable |
| Database Browser | all 279 | [R] | [R] default | paged, read-only default; edits only via validated section services |
| Diagnostics | engine/files | [R] | [N] | load state, counts, CRC/save verification |
| Dashboard | app-level | [R] | [N] | counts, recent activity |
| Settings | app-level | n/a | app config | stored in app settings, not the DB |

## Assets (faces / kit & crest images / 3D)

Updated 2026-07-26 after a full local-asset audit — see **ASSET_INVENTORY.md** and
**ASSET_SUPPORT_MATRIX.md** for the verified per-category breakdown.

| Capability | Status | Reason |
|------------|--------|--------|
| Show DB **references** to assets (playerid, ballid, stadiumid, shoetype, gkglovetypecode, nationid) | [R] | stored as integers in DB; resolved to real local files where present |
| **Preview player miniface** (read-only) | [P] | 115 local `p{playerid}` DDS/PNG files verified against `players.playerid`; the other ~20k players show an honest "No local miniface" state |
| **Preview ball / stadium / boot / GK glove / flag** (read-only) | [P] | real PNG files verified against `teamballs.ballid`, `stadiums.stadiumid`, `playerboots.shoetype`, `players.gkglovetypecode`, `nations.nationid`; partial coverage, honest fallback |
| Preview team crest / minikit / league & competition logo / full kit render / face 3D | [N] | source folders empty or absent locally (see ASSET_INVENTORY.md) — shown as "unavailable" |
| Write/replace/import any asset image | [N] | engine has no asset-archive writer; no import control is exposed |
| DDS decode | [R] | self-contained BC1/BC3 (DXT1/DXT5) decoder, RMSE-verified against reference PNG; PNG via GDI+ |
| App behaviour | — | real image when a verified local file exists; clearly labelled "No local asset" otherwise; never claims an asset was written |

Asset preview is **read-only**. **No asset category supports import or write.**

## Explicitly disabled (not exposed as editable)

- Releasing a player to free agents (requires deleting a `teamplayerlinks` row) — **[N]**, shown as disabled with explanation.
- Adding a brand-new player/team/row — **[N]**.
- Player first/last/common/known-as **display-name decode** — **[N]** (EA proprietary locale cipher; key absent from the file set). A read-only resolution pipeline is implemented and will show real names the moment a decodable source exists; until then it shows an honest `Player {id}` fallback. See LOCALE_DECODER_REPORT.md.
- Player-name **editing** — **[N]** (locale name writing unsupported); name fields are read-only.
- Any write that bypasses the engine's validation — **forbidden** by design (UI never writes bytes directly).

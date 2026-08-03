# CM26 Asset Inventory

Date: 2026-07-26
Auditor: Lead senior C# Windows desktop engineer (autonomous)

This document records **only assets that physically exist on local disk** and whose
linkage to the FC26 database was **verified against the real loaded database** (not assumed).
An asset is *not* listed as available merely because the database contains an ID column â€”
each category below was cross-checked by reading actual files and, where an ID key exists,
by confirming the ID occurs in the corresponding DB table.

## How linkage was verified

The app service stack (`AppServices` â†’ `DatabaseSession`) loads the real DB
(`fifa_ng_db.db`, 360,298 rows / 279 tables). Temporary headless commands
(`--verify-assets`, `--dump-schema`, `--distinct`) were used to:

1. Dump the real column list of candidate tables (`teamballs`, `stadiums`, `playerboots`, `players`, `nations`).
2. Dump the distinct value set + range of candidate key columns.
3. Confirm sampled asset filenames (IDs) actually occur in the DB.

All temporary commands were removed after use. Tests ran against a **scratch copy** of the DB.

## Verified asset locations (real files)

| # | Asset category | Source folder (local) | File format | Keying | Files present |
|---|----------------|-----------------------|-------------|--------|---------------|
| 1 | Player minifaces | `<FC26 tools>\miniface\**\p{playerid}.dds` / `.png` | DDS (DXT5) + PNG (32bpp ARGB) | `players.playerid` | **118 DDS + 39 PNG** |
| 2 | Balls | `<FC26 tools>\FC Editor by decoruiz Alpha v21\assets\26\ballid\{ballid}.png` | PNG | `teamballs.ballid` | **294 PNG** |
| 3 | Stadium previews | `<FC26 tools>\FC Editor by decoruiz Alpha v21\assets\stadiumid\{stadiumid}.png` | PNG | `stadiums.stadiumid` | **182 PNG** |
| 4 | Boots | `<FC26 tools>\FC Editor by decoruiz Alpha v21\assets\26\shoetypecode\{shoetype}.png` | PNG | `playerboots.shoetype` | **256 PNG** |
| 5 | GK gloves | `<FC26 tools>\FC Editor by decoruiz Alpha v21\assets\26\gkglovetypecode\{code}.png` | PNG | `players.gkglovetypecode` | **115 PNG** |
| 6 | Country flags | `<FC26 tools>\FC Editor by decoruiz Alpha v21\FC Editor by decoruiz Alpha v21.exe_extracted\art\flags\{nationid}.png` | PNG | `nations.nationid` | **156 PNG** |

## Verified DB key ranges (from the live DB)

| Table.Column | Distinct | Min | Max | Notes |
|--------------|----------|-----|-----|-------|
| `players.playerid` | 20,268 | â€” | â€” | miniface IDs keyed here |
| `teamballs.ballid` | 149 | 0 | 199 | ballid PNG range 0â€“199 aligns |
| `stadiums.stadiumid` | 180 | 1 | 534 | stadium PNG covers many but **not all** (e.g. 534 absent) |
| `playerboots.shoetype` | 294 | 0 | 546 | shoetypecode PNG covers a subset |
| `players.gkglovetypecode` | 107 | 0 | 146 | gkglovetypecode PNG covers most |
| `nations.nationid` | 218 | 1 | 225 | flags PNG covers 156 of 218 |

### Miniface linkage proof (executed)

25 sampled `p{id}` filenames were checked against the `players` table via `--verify-assets`:
**25 FOUND, 0 missing.** Two deliberately fake IDs (`999999`, `123456`) correctly returned
**NOT in players table**. The `p{playerid}` â†’ `players.playerid` mapping is therefore real.

### Miniface DDS format proof

All 118 miniface DDS headers were parsed. Distribution:
- `81Ã—` â€” 180Ã—180, 1 mip, **DXT5** (`fourcc=DXT5`, `pfFlags=0x4`)
- `37Ã—` â€” 160Ã—160, 1 mip, **DXT5**

PNG minifaces are 180Ã—180 `Format32bppArgb` (alpha preserved). DXT5 carries an alpha channel.

## Asset categories that are genuinely UNAVAILABLE (empty or absent)

These were checked and are **empty on disk** â€” they are listed honestly as unavailable,
never substituted with fabricated art:

| Category | Expected folder / source | Finding |
|----------|--------------------------|---------|
| Team crests | `...\assets\26\crest\dark`, `...\assets\26\crest\light` | **0 files (empty folders)** |
| Team minikits | `...\assets\26\minikits`, `...\assets\minikits` | **0 files (empty folders)** |
| Competition / league logos | no FC26-keyed folder found | absent |
| Kit textures (full render) | no FC26-keyed folder found (`Kits Collection` = PSG PES-conversion PNG/WEBP, not DB-keyed) | absent |
| Face 3D models / head renders | `assets\26\heads` does **not** exist (only FC25 `assets\25\heads`) | absent for FC26 |
| EBX / RES / CHUNK archives | searched entire `<FC26 tools>` | **0 .ebx / .res / .chunk files** |
| Frosty/FMT game archives (`.fbmod`, `.cas`, `.toc`) | not present in a usable keyed form | absent |

## Notes on honesty / scope

- The `FC Editor by decoruiz` asset library is a **third-party extracted art pack**. It is used
  strictly as a **read-only local image source**; nothing from it is modified or written back.
  Its licence is documented in `THIRD_PARTY_NOTICES.md`.
- Only **minifaces** ship as **DDS**; every other category is **PNG** (GDI+-readable).
- The texture preview service therefore must support **DXT5 DDS decode** (for minifaces) and
  **standard PNG** (for everything else). No TGA files were found anywhere.
- Asset **write / import** is **not** supported by the engine and is **not enabled** anywhere.

# Creation Master 26 — Current limitations

Status: Version 1.0.157 (MIT License), 2026-08-25.

## User interface

The v1.0.157 public launcher preserves the original Creation Master / CM16-style interface used by v1.0.148. The x64 Frostbite host runs behind that interface; the internal modern Studio is available only with `--studio`. Exotic DPI combinations or very small window sizes may still need refinement.

v1.0.124 adds embedded in-app 3D previews (HelixToolkit + Assimp FBX import) for kit, player face, stadium, ball and boot records. The external `CM26.3DViewer` remains available as a separate tool for standalone FBX inspection.

v1.0.132 can launch exported FBX meshes in an optional F3D installation. F3D
is not redistributed by CM26; when it is absent, the bundled CM26 viewer remains
the default external-viewer fallback.

v1.0.137 keeps FC26 minifaces in a bounded in-memory cache and loads the visible
Starting XI before the bench. A first visit can still take longer when Frostbite
assets must be extracted from the installed game, but the UI remains responsive
and subsequent visits to cached players are immediate. Formation cards are
visually reflowed when database-native centre points are too close; this does not
rewrite the stored FC26 position coordinates.

## Direct editing

CM26 writes validated database and staged legacy replacements directly into the
opened FC26 `Data`/`Patch`. The real installed containers have been exercised in
prepare/verify mode, including Oodle compression, collector offsets and signed
temporary TOCs. A live commit is deliberately performed only after the user
confirms Save and while FC26 is closed.

The writer updates resolved legacy chunks. It is not a general EBX schema editor
and cannot safely modify an arbitrary unknown Frostbite asset type.

## Backup boundary

`CmModData` must contain a complete original `Data` and `Patch`. Restore removes
files that are not present in that backup, so it is a full restoration rather
than a merge. If the backup itself was changed or deleted, CM26 cannot recreate
the earlier original state.

## Database fields

The public UI does not expose an **All FC26 Data** or raw-fields editor. Each
supported value is mapped into a named CM16-style control. A mapped value is
editable only when its type and storage are supported by the database engine.
Unknown encodings, unsafe identity changes and fields without a writable schema
remain locked.

Some FC26 display names use EA locale/runtime encodings that are not fully
recoverable from the supplied files. CM26 uses resolved names when available
and an honest ID fallback otherwise.

FC26's static `teams` table exposes `clubworth`, not the live Career transfer
budget. CM26 therefore keeps that squads value labelled **Club Worth**. Since
v1.0.138 the Team page also provides a separate Career-save editor for the real
`career_managerpref.transferbudget` and `startofseasontransferbudget` values.
It resolves the active club through `career_users.clubteamid`, performs the load
and save away from the UI thread, and creates a timestamped backup beside the
Career save before writing it.

## Structural edits

The current native engine supports the row operations used by roster transfers,
free-agent links and loans. Structural undo is not available after an insert or
delete has been staged; use Revert/close without Save, or Restore after a saved
live-game change.

## Visual assets

Legacy UI textures can be previewed and targeted replacements can be staged.
Availability depends on the corresponding installed FC26 legacy path. Remove
means “remove the staged replacement and show the original”; CM26 does not
delete the original asset from the game.

## Audio

Player commentary mappings, nation/stadium audio regions, and the custom
team-name, anthem and chant/goal-song catalog records are editable. The bundled
CallName TTS tool can be opened from the Player Callname tab. The Audio section
parses, lists and exports installed Harmony/NewWave RES banks using the FC26
bank structure. Local WAV/MP3/WMA/M4A files can be previewed. Importing newly encoded media remains deliberately unavailable:
FET's reference workflow proves this operation requires a coordinated RES bank,
SPS/SEK chunk and sometimes EBX update. CM26 does not present a misleading
single-field import button; catalog entries must reference an item already
present in the installed FC26 banks.

## 3D

The packaged renderer displays exported FBX models and associated textures.
Player face folders are auto-detected by `headclasscode`/`playerid` when an
export exists. CM26 does not yet convert an installed Frostbite skinned mesh to
FBX itself, so a face with no extracted FBX cannot be rendered in 3D.

## Transfermarkt and local scraper

The CM26 Scraper is **not included** in the CM26 package. Its data set contains
EA-derived database content (database tables, schema and name lists) that this
project does not redistribute, so it must be downloaded separately. Data Sync is
fully functional once you point CM26 at your own copy using **Set folder...**, or
keep a `CM26 Scraper` folder beside CM26. When no copy is installed, Data Sync
explains how to add one instead of failing silently; every other section works
without it.

With a scraper present, the Data Sync page opens it, discovers its latest
`squad_*.xlsx` output, previews it and imports a confirmed squad to the selected
team. The import creates players and team-player links, but it does not overwrite
existing CM26 records or silently write data without confirmation.

The optional Transfermarkt URL preview/CSV view needs no scraper and is a
research aid only. Transfermarkt may change its HTML or block automated requests;
in that case the page can load with zero recognised rows.

## Compdata

The integrated editor works on `.xlsx` Compdata workbooks and saves to a new
copy. It validates documented object/parent links and the most important linked
worksheets, but it does not compile the workbook into game archives; use the
established Compdata build/export step after editing. Complex formulas or macros
are outside the supported workbook format.

The League/Cup builder creates a safe starting structure. Teams, custom task
logic, standings rules, settings and detailed scheduling still need to be set in
their named worksheets before the external Compdata build/export step.

The local scraper import creates database records from its supplied squad
fields and links them to the selected team. It does not download or assign
third-party club/nation logos or face assets, and it does not overwrite an
existing CM26 player.

## Platform

Windows 10/11 x64 only. The Lite package requires .NET 8 Desktop Runtime x64.

## Code signing and security tooling

CM26 is distributed unsigned: Windows SmartScreen and some antivirus products may
show an “unknown publisher” warning on first run. Because the app validates and
rewrites parts of the FC26 database and patch containers, it ships helper
libraries that share algorithms with the retail console packaging (EA's TOC and
locale-string ciphers are implemented as reference keys inside the engine
bridge; they only decrypt and re-encrypt the same formats the game itself
reads). This is required to write edited files back without leaving them
corrupted; CM26 never extracts or redistributes those original files. Reviewing
the source or disabling encryption is unsupported.

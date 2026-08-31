# Creation Master 26 — Current limitations

Status: Version 1.0.190 (MIT License), 2026-09-01.

## User interface

The v1.0.190 public launcher is named `Creation Master 26.exe` and opens only the familiar Creation Master / CM16-style interface. There is no separate Studio executable or Studio launch switch. The x64 host remains behind the classic interface for Frostbite database, asset and preview work. Exotic DPI combinations or very small window sizes may still need refinement.

Both **Full Portable** and **Lite Portable** are assembled with checksums. Lite
requires the Microsoft .NET 8 Desktop Runtime; Full includes its own runtime.

Team Generic's Transfer Budget is a Deco-compatible estimate when a base squads
database is loaded. FC26 does not store an editable per-team budget on the
static `teams` row; the actual amount is read and written from a loaded Career
save. Club Worth remains the static team value, formatted with Deco's scale.

New league structures are loaded by Career initialization. Create at least two
teams, use the normal CM26 **Save**, then start a new Career; an existing Career
save is not expected to rebuild its competition graph automatically.

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

v1.0.185 removes the main synchronous preview hot paths from Player Info and
Team Roster. A first uncached Frostbite extraction can still take time, but it
runs outside the UI thread and stale navigation requests are discarded.

v1.0.188 also moves shared Frostbite texture decoding off the UI thread. The
first construction of a large editor can still take longer than a cached return,
so CM26 shows a dedicated loading surface; every warm section switch is guarded
at 100 ms in the release navigation audit.

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
budget. The Team page therefore shows **Club Worth** and the Deco-compatible
**Transfer Budget** reference as read-only currency values directly below it.
The second figure is calculated from club worth and profitability for display;
it is not presented as an editable database field and does not overwrite a live
Career save.

FC26 `playernames` rows are shared by design. When a player name is edited,
CM26 allocates a free dictionary ID and leaves the original shared row intact.
The Save Preflight and Diagnostics reports show the available ID ranges; an
existing Career must be restarted after database or Compdata changes.

## Structural edits

The current native engine supports the row operations used by roster transfers,
free-agent links and loans. **Revert All Unsaved FC26 Database Changes** reloads
the original session snapshot and discards the complete unsaved database/detail/
structural transaction. It is intentionally a whole-transaction rollback rather
than an unsafe guessed inverse for only the latest inserted row. Staged visual
assets are managed separately.

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

The integrated Competition editor loads the 11 installed FC26 Compdata TXT
assets, validates object/parent/database/team links, row limits, schedules and
advancement paths, then stages the complete set into CM26's normal direct Save
transaction. No FIFA Editing Tool build/import step is required. Workbook/TXT
copy commands remain optional authoring and backup tools only; complex formulas
or macros are outside the supported workbook format.

The League builder creates the country/league/stage/group graph, initial-team
links, standings and a complete double round-robin calendar automatically. The
generic Tournament Wizard creates a conservative starting structure; custom
task formulas and unusual competition rules still require explicit user choices.

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

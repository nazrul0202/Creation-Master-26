# Creation Master 26

**Version 1.0.24** · Windows x64 · by Rizco98

Creation Master 26 is a direct EA SPORTS FC 26 database and legacy-asset editor.
It does not build a separate mod package. Saving writes the selected, validated
changes into the installed game's `Data` and `Patch` containers.

> **License**: this is commercial-licensed software, not open source. By
> installing or using it you agree to the [`EULA`](EULA.md) and
> [`LICENSE`](LICENSE). It is an **unofficial, independent community tool** and is
> **not affiliated with or endorsed by EA SPORTS**.

## Safety model

- The first successful **File > Open Game** creates
  `<FC26>\CmModData\Data` and `<FC26>\CmModData\Patch`.
- `CmModData` is the immutable copy of the original game state.
- Editing happens in CM26's isolated database workspace until **Save** is confirmed.
- Direct asset/database commit is transactional: new CAS data is append-only, TOCs
  are prepared and verified before replacement, and a failed commit restores the
  previous TOCs and CAS lengths.
- **File > Restore Original Data** replaces the live `Data` and `Patch`
  trees from `CmModData`.
- FC26 must be closed for Save and Restore.

Do not edit, rename or delete `CmModData` unless you have another complete copy
of the original FC26 installation.

## Quick start

1. Run `CM26_by_Rizco98.exe`.
2. Choose **File > Open Game**.
3. Select the FC26 installation only if automatic detection does not find it.
4. Edit database fields or import a supported legacy image.
5. Press **Validate**, then **Save** and confirm the live-game warning.
6. Use **File > Restore Original Data** if the original game state is needed.

The startup workspace is intentionally blank. Database and legacy data load
together when FC26 is opened; selecting individual database files is not needed.

## Editor coverage

The application contains 23 navigable public modules. The requested editors include:

- Countries, leagues, teams, national teams and players with safe **New** commands and automatic unused IDs.
- Teams, roster, formation with minifaces, player contract/record details,
  loans and transfers directly inside **Teams > Roster**.
- Players, complete skills/traits, face values and the packaged 3D FBX viewer.
- Managers, stadiums, stadium audio, kits, competitions and formations.
- Integrated Compdata workbook editor with workbook-native column names,
  capacity checks, League/Cup builder, promotion/relegation links, automatic
  object IDs, linked-row validation, safe Save Copy and UTF-8 TXT export.
- Balls, boots and goalkeeper gloves with installed-game texture preview.
- Sponsors, adboards, flags, audio and presentation data.
- Data Sync workspace that opens the local CM26 Scraper, detects and previews
  its latest squad output, then imports the confirmed squad directly to a
  selected team with generated player IDs and team-player links. The scraper
  ships inside the package under `Tools\CM26 Scraper\`; an existing copy next
  to CM26, at a drive root, or configured in Settings is used automatically.
  Transfermarkt URL preview/CSV remains available as a non-writing research tool.
- Database Browser, Diagnostics and Settings.

The public editor uses named CM16-style controls. It does not expose **All FC26
Data**, raw-field pages or old placeholder data. Only explicitly mapped fields
are shown; writable mappings are staged and validated before Save.

The Audio section can browse, inspect and export installed NewWave banks and
preview local WAV/MP3/WMA/M4A audio.
Player callnames, national-team audio, team callnames, anthems and chant/goal-
song catalog mappings are available in their relevant Player, Country and Team
sections.

## Legacy image import/remove

Supported preview panels expose **Import** and **Remove**. Import stages a
replacement for the exact installed FC26 legacy path. Remove discards the staged
replacement and returns to the installed original; it does not delete the
original game asset. The replacement is committed to `Data`/`Patch` only on Save.

## 3D viewer

The packaged viewer opens exported FC26 FBX face/model folders. For Players,
CM26 searches configured/local FC26 asset folders using the selected player's
`headclasscode` and `playerid`, then opens a matching head/hair/mouthbag set
automatically when found. If no extracted mesh exists, CM26 asks for the folder.
Native Frostbite mesh-to-FBX export is not yet performed inside CM26.

## Local files

- Settings/log/cache/workspaces:
  `%LOCALAPPDATA%\Creation Master 26\`
- Original game backup:
  `<FC26>\CmModData\`
- Full Portable includes .NET 8, the separate asset bridge and the CM26 Scraper
  under `Tools\CM26 Scraper\`.
- Lite requires Microsoft .NET 8 Desktop Runtime x64.

See `KNOWN_LIMITATIONS.md`, `ASSET_SUPPORT_MATRIX.md` and
`FROSTBITE_ASSET_BRIDGE_STATUS.md` for exact boundaries.

Creation Master 26 is a community tool and is not affiliated with or endorsed
by EA SPORTS.

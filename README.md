# Creation Master 26

<p align="center">
  <img src="https://img.shields.io/badge/version-1.0.94-blue" alt="Version">
  <img src="https://img.shields.io/badge/license-MIT-green" alt="License">
  <img src="https://img.shields.io/badge/platform-Windows%20x64-lightgrey" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4" alt=".NET">
  <img src="https://img.shields.io/badge/FC-26-brightgreen" alt="FC 26">
</p>

**Direct EA SPORTS FC 26 database and legacy-asset editor for Windows.**

Edit players, teams, leagues, countries, kits, formations, compdata, and legacy assets
directly inside the installed FC26 game files. No external mod packages — saving writes
validated changes into the Frostbite `Data` and `Patch` CAS archives.

> **License**: MIT — see [`LICENSE`](LICENSE).  
> **Unofficial community tool** — not affiliated with or endorsed by EA SPORTS.

---

## Features

### Core editors
- **Players** — Full player card with OVR/POT, PAC/SHO/PAS/DRI/DEF/PHY ratings, attributes, playstyles, roles, face preview, and 3D FBX viewer
- **Teams** — Club profile with OVR/ATT/MID/DEF bars, kits, squad roster, formation, stadium, manager
- **Leagues** — League profile, clubs grid, settings flags, country assignment
- **Countries** — Nation profile, flag viewers, national team linking, audio settings
- **Managers, Stadiums, Kits, Competitions, Formations** — Full FC26 schema coverage

### Data & assets
- **Compdata workbook editor** — League/Cup builder, promotion/relegation links, TXT export
- **Legacy asset import/remove** — Crests, flags, logos, minifaces, kit textures
- **Frostbite asset browser** — NewWave audio banks, RES/Ebx asset search
- **Data Sync** — Import squads from CM26 Scraper (optional separate download) or Transfermarkt CSV

### Safety & workflow
- **Transactional saves** — CAS data is append-only, TOCs verified before replacement, failed commits roll back
- **CmModData backup** — Immutable copy of original game state created on first open, restorable anytime
- **CM26 Project files** — Export/import editable `.fifaproject` snapshots without touching live game files
- **FIFA Mod Manager export** — Generate `.fifamod` packages for the mod manager workflow

---

## Quick start

1. Download the [latest release](https://github.com/nazrul0202/Creation-Master-26/releases)
2. Run `CM26_by_Rizco98.exe`
3. **File > Open Game** — select your FC26 installation folder
4. Navigate to any section and edit fields
5. **Validate** then **Save** to commit changes

| Package | Size | Description |
|---------|------|-------------|
| **Full Portable** | ~76 MB | Self-contained, includes .NET 8 runtime |
| **Lite** | ~11 MB | Framework-dependent, requires .NET 8 Desktop Runtime x64 |

---

## Tech stack

| Component | Technology |
|-----------|-----------|
| UI Framework | C# WinForms (.NET 8) |
| Engine Bridge | C++/CLI native interop |
| Frostbite parser | C++ CAS/TOC/layout reader |
| Mesh processing | CM26.MeshKit (FBX export) |
| Asset bridge | CM26.AssetBridge (separate process) |
| 3D viewer | Bundled CM26.3DViewer (WebGL) |

---

## Building from source

```bash
# Requirements: Windows x64, .NET 8 SDK, Visual Studio 2022+ with C++ workload
build-managed.cmd                           # Bridge + solution + native engine smoke test
CM26_by_Rizco98.exe --release-selftest      # Release checks, no game needed
```

See [`docs/BUILDING.md`](docs/BUILDING.md) for full instructions.

---

## Editor coverage

**23 navigable modules** — all with real FC26 field mappings, validation, and staging:

`Dashboard` · `Countries` · `Leagues` · `Teams` · `Players` · `Managers` · `Stadiums`  
`Kits` · `Competitions` · `Formations` · `Balls` · `Boots` · `Gloves` · `Sponsors`  
`Adboards` · `Audio` · `Referees` · `Transfers` · `Mod Manager` · `Database Browser`  
`Diagnostics` · `Settings`

---

## No EA content

This project redistributes **zero EA game content** — no database tables, schema files, audio,
textures, meshes, or name lists. The release script verifies this automatically before packaging.

---

## License

MIT — see [`LICENSE`](LICENSE) for full terms.
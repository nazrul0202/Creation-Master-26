# ⚽ Creation Master 26

<p align="center">
  <img src="https://img.shields.io/github/stars/nazrul0202/Creation-Master-26?style=for-the-badge&color=74B922" alt="GitHub stars">
  <img src="https://img.shields.io/github/forks/nazrul0202/Creation-Master-26?style=for-the-badge&color=74B922" alt="GitHub forks">
  <img src="https://img.shields.io/github/v/release/nazrul0202/Creation-Master-26?style=for-the-badge&color=74B922" alt="Release">
  <img src="https://img.shields.io/github/actions/workflow/status/nazrul0202/Creation-Master-26/ci.yml?branch=main&style=for-the-badge" alt="CI">
  <img src="https://img.shields.io/badge/license-MIT-brightgreen?style=for-the-badge" alt="License">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge" alt=".NET 8">
  <img src="https://img.shields.io/badge/platform-Windows%20x64-lightgrey?style=for-the-badge" alt="Platform">
</p>

<p align="center">
  <b>The direct database & asset editor for EA SPORTS FC 26 career mode.</b><br>
  Edit players, teams, leagues, countries, kits, formations and compdata — then save
  <b>validated</b> changes straight into your installed game. No external mod packages needed.
</p>

> **Unofficial community tool** — not affiliated with or endorsed by EA SPORTS. Redistributes **zero** EA game content.
> License: MIT — see [`LICENSE`](LICENSE).

---

## 🚀 Quick start

1. Download the [latest release](https://github.com/nazrul0202/Creation-Master-26/releases) — **Full Portable** (self-contained) or **Lite** (needs .NET 8)
2. Run `CM26_by_Rizco98.exe`
3. **File > Open Game** → select your FC26 installation folder
4. Edit any section, press **Validate**, then **Save** to commit

| Package | Size | Description |
|---------|------|-------------|
| **Full Portable** | See release asset | Self-contained, includes .NET 8 runtime — works on any Windows x64 |
| **Lite** | See release asset | Framework-dependent, requires .NET 8 Desktop Runtime x64 |

Checksums: `SHA256SUMS_v1.0.155.txt` is included with the v1.0.155 release assets.

---

## ✨ Features

### Core editors and workflow hub

- **Comprehensive Tools** — one searchable launch point for all 38 accepted CM26 workflow areas, routed to the real Direct Frostbite editors and validators
- **Players** — Full FC-style player card: OVR/POT tiles, PAC/SHO/PAS/DRI/DEF/PHY rating bars, playstyles, roles, contract, face preview + **3D FBX viewer** with optional F3D launch
- **Teams** — FC Tools Hub-style club profile: crest, overall tile, ATT/MID/DEF bars, kits, squad roster with drag-drop lineups, formation board, FC26 build-up style, defensive line/height and three-context team traits
- **Leagues** — Profile header with logo/OVR bars, clubs grid, settings flags, country assignment
- **Countries** — Nation profile, flags, national-team linking, regional audio mappings
- **Managers · Stadiums · Kits · Competitions · Formations · Balls · Boots · Gloves · Referees · Transfers** — full FC26 schema coverage
- **Sponsors · Adboards · Audio** — FC26-only tables with artwork preview & NewWave bank browser
- **Advanced Database Workspace** — browse/edit every table; all-row text/numeric filters and saved searches; transactional copy/paste and bulk replace; file comparison; row templates; pending/history views; dependency and asset-usage reports; safe linked-ID replacement; and TSV/CSV import/export for one or all tables
- **Roster integrity tools** — repair broken/duplicate club links and missing team-sheet players, assign unique shirt numbers, and run the engine's full key/relationship validator
- **Restored specialist tabs** — Players includes Info, Skills, Face, Details and Callname; Teams includes Sponsors, Adboards, Flags and Audio

### Data & assets
- **Compdata editor** — Simple structure tree, visual calendar/conflict view, league/cup wizard, advancement links, raw XLSX/TXT editing and validated TXT export
- **Direct native asset import/remove** — images plus format-compatible RX3/DDS/BIG payloads for verified player, kit, stadium, ball, boot, glove and presentation paths
- **Deco asset-pack discovery** — automatically uses a local `FC Editor by decoruiz Alpha v21` folder (including the standard Downloads location) for supported visual previews
- **Frostbite asset search** — RES/Ebx search, NewWave audio bank metadata inspection
- **Data Sync** — import squads from CM26 Scraper or Transfermarkt CSV

### Safety & workflow
- **Transactional saves** — CAS data is append-only; TOCs verified before replacement; failed commits roll back
- **CmModData backup** — immutable copy of your original game state on first open, restorable anytime
- **Direct FC26 Save** — validates, backs up and commits database and staged native assets without a FIFA Mod Manager package
- **Lightweight CM26ModData overlay** — NTFS symlinks mirror unchanged FC26 archives; copy-on-write materialises only modified CAS/TOC files
- **Automated quality gate** — xUnit regression suite, all-section multi-resolution layout smoke test and GitHub Actions CI

---

## 🖼️ Screenshots

The responsive workspace is verified automatically at common laptop and desktop sizes.
Run `CM26_by_Rizco98.exe --release-selftest` for the offline release checks.

---

## 🧱 Tech stack

| Component | Technology |
|-----------|-----------|
| UI | C# WinForms (.NET 8), custom light theme with FC26 accent palette |
| Engine bridge | C++/CLI native interop (CAS/TOC/layout parsing) |
| Frostbite parser | C++ — layout descriptors, locale decoders, chunk writer |
| Mesh processing | CM26.MeshKit (FBX export) |
| Asset bridge | CM26.AssetBridge (separate process) |
| 3D viewer | Bundled CM26.3DViewer (WebGL) + optional F3D desktop integration |

---

## 🛠️ Building from source

```bash
# Requirements: Windows x64, .NET 8 SDK, Visual Studio 2022+ with C++ workload
build-managed.cmd                           # Bridge + solution + native engine smoke test
CM26_by_Rizco98.exe --release-selftest      # Release checks, no game needed
```

See [`docs/BUILDING.md`](docs/BUILDING.md) for the full build & packaging pipeline.

---

## 📁 Repository layout

```
src/CM26.App/            # WinForms app (UI, sections, validation, staging)
src-native/CM26.EngineBridge/ # C++/CLI bridge to the Frostbite engine parser
src/CM26.AssetBridge/    # Separate asset-process bridge
src/CM26.MeshKit/        # FBX mesh export
tests/CM26.Tests/        # Offline xUnit regression suite and fixtures
docs/BUILDING.md         # Build & release pipeline
docs/reports/            # Reverse-engineering research reports (30+ documents)
Release/                 # Local-only assembled release packages + SHA256SUMS
```

---

## ❓ FAQ

**Does this replace FIFA Editor Tool / Creation Master 16?**
CM26 is a from-scratch FC26 editor — it does not reuse CM16 code or content. It targets
the FC26 Frostbite database (`fifa_ng_db`) and legacy UI asset pipeline directly.

**Is this safe for my game save?**
Every write goes through validation + transactional CAS commits with automatic rollback,
and the first open creates a restorable backup of the original game state.

**Can I contribute?**
Yes! Open issues for bugs/requests and submit PRs — see [Contributing](#-contributing).

---

## 🤝 Contributing

- **Report bugs** — open an issue with steps to reproduce and a log snippet
- **Request features** — open an issue tagged `enhancement`
- **Submit code** — fork, branch, PR against `main`. Keep the MIT license header and run
  `CM26_by_Rizco98.exe --release-selftest` before submitting
- **Share knowledge** — the `docs/reports/` research documents are open for review and correction

## 💬 Support

- **Issues**: [github.com/nazrul0202/Creation-Master-26/issues](https://github.com/nazrul0202/Creation-Master-26/issues)
- **Releases**: [github.com/nazrul0202/Creation-Master-26/releases](https://github.com/nazrul0202/Creation-Master-26/releases)
- **Star us ⭐** — stars tell the community the tool works and keep development going

---

## ⚖️ License

MIT — see [`LICENSE`](LICENSE) for full terms. This project contains **no EA game content**:
the release pipeline fails the build if any game-derived database, audio, texture, or mesh
is found in a package.

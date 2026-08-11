# Contributing to Creation Master 26

Thanks for helping make the first open-source FC26 database editor better!

## Ground rules

- **No EA game content.** This project redistributes zero EA-derived content
  (database tables, schemas, audio, textures, meshes, name lists). The release
  pipeline fails the build if any slips in — keep it that way in PRs.
- **MIT license.** Contributions are accepted under the project's MIT license.
- **Keep it honest.** Tools that claim to write the game database must fail loudly
  rather than guess. If a write path is unverifiable, mark it read-only.

## Ways to contribute

### Report a bug
Open an issue with:
- Steps to reproduce (which section, which record, what you changed)
- What you expected vs. what happened
- The log file if available (`%APPDATA%\CreationMaster26\logs`)

### Request a feature
Open an issue tagged `enhancement`. Include why it matters for editing FC26
career-mode data — not just what the UI should look like.

### Submit code
1. Fork the repo and create a branch from `main`.
2. Follow the existing conventions: WinForms sections live in `src/CM26.App/Sections`,
   palette constants in `src/CM26.App/Theming/Theme.cs` + `Controls/CardLayout.cs`.
3. Build and run the release self-test before submitting:
   ```bash
   build-managed.cmd
   CM26_by_Rizco98.exe --release-selftest
   ```
4. Open a PR describing the change and how you verified it.

### Share research
The `docs/reports/` folder documents reverse-engineering of the FC26 Frostbite
formats (layout descriptors, locale decoders, CAS/TOC writes). Corrections and
new findings are very welcome — a well-written report is a first-class contribution.

## Code of conduct

Be respectful. This is a volunteer project around a commercial game — keep
discussions about the tool and the data formats, not about people.

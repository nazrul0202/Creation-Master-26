# Building Creation Master 26

Windows x64 only. Every step below works from a fresh clone.

## Prerequisites

| Requirement | Notes |
|---|---|
| Windows 10/11 x64 | The app, the native engine and the C++/CLI bridge are Windows-only. |
| **.NET 8 SDK** | https://dotnet.microsoft.com/download/dotnet/8.0 |
| **Visual Studio 2022 or newer** with the **Desktop development with C++** workload | Any edition: Community, Professional, Enterprise, Preview, or the standalone **Build Tools**. The scripts locate it with `vswhere`, so no edition or path is hardcoded. |
| CMake 3.24+ | *Optional.* Only for the alternative native-only build (`CMakeLists.txt`). Not needed for a normal build. |

The C++ workload must include the **MSVC v143 x64 toolset**; the build scripts
verify `cl.exe` is reachable and stop with a clear message if it is not.

## Build

```bat
build-managed.cmd
```

That single command:

1. resolves the Visual Studio environment (`vsenv.cmd`, via `vswhere`),
2. builds the native C++/CLI bridge `CM26.EngineBridge.dll`,
3. builds the full solution `CM26.slnx` (Release | x64),
4. builds the native engine and `EngineSmokeTest.exe`,
5. runs the engine smoke test and fails the build if it does not pass.

**Build the bridge first if you build the solution by hand.** `CM26.App` and
`CM26.Application` reference `CM26.EngineBridge.dll` by `HintPath` (a C++/CLI
assembly cannot be a `ProjectReference`), so the DLL must exist before the
managed projects compile. `build-managed.cmd` already does this in the right
order.

Individual scripts:

| Script | Purpose |
|---|---|
| `vsenv.cmd` | Shared VS environment resolver. Called by all others. |
| `build-bridge.cmd` | Native C++/CLI engine bridge only. |
| `build.cmd` | Native engine app + `EngineSmokeTest.exe`. |
| `build-cascade-smoke.cmd` | `CascadeSmokeTest.exe` regression probe. |
| `build-structural-read.cmd` | `StructuralReadTest.exe` regression probe. |
| `build-structural-smoke.cmd` | `StructuralSmokeTest.exe` regression probe. |

## Tests

### Release self-test — no game installation needed

```bat
& 'src\CM26.App\bin\Release\net8.0-windows\Creation Master 26.exe' --release-selftest
```

Verifies portable tool detection (no developer paths), that the build output
redistributes no EA game content, and that the version strings agree. This is the
gate CI runs; it must exit `0`.

### Studio shell smoke — no game installation needed

```bat
& 'src\CM26.App\bin\Release\net8.0-windows\Creation Master 26.exe' --ui-shell-smoke
```

Constructs and closes the public x64 Direct Frostbite Studio. This catches missing
resources and public-startup regressions without indexing FC26. The separate
`--cm16-studio` switch remains available for compatibility-shell diagnostics.

### Native engine smoke test

```bat
EngineSmokeTest.exe
```

Run automatically by `build-managed.cmd`. Exercises table loading, a locale-string
round trip, a save round trip and a structural add/delete.

### Tests that require your own FC26 installation

These read a real database and are therefore not part of CI. Each is a headless
mode of the app and prints its result to the console:

```bat
Creation Master 26.exe --smoke              <dbFolder>
Creation Master 26.exe --nav-test           <dbFolder> <assetRoot-or-empty>
Creation Master 26.exe --layout-test        <dbFolder> <assetRoot-or-empty>
Creation Master 26.exe --squad-probe        <dbFolder>
Creation Master 26.exe --formation-test     <dbFolder>
Creation Master 26.exe --compdata-test      <workbook.xlsx>
Creation Master 26.exe --frostbite-test     [gameRoot]
Creation Master 26.exe --workspace-test     [gameRoot]
Creation Master 26.exe --backup-audit       [gameRoot]
```

`--live-save-roundtrip` writes to a real installation and requires an explicit
confirmation token. Do not run it against an installation you care about.

## Producing the release packages

The release is assembled from `dotnet publish` output. Both runtime models are
published, plus a matching asset bridge for each:

> **Run the four publish commands sequentially, never in parallel.** Two of
> them publish the same projects into the same `obj\` intermediates; parallel
> invocations race and can produce a corrupted `Creation Master 26.deps.json`
> that omits the bundled runtime packs. The resulting package fails to launch
> with "Could not resolve CoreCLR path". `assemble_packages.ps1` now fails on
> that exact corruption, but the corrupted output would still need a clean
> sequential re-publish.

```bat
:: version folder suffix = version.json with dots replaced by underscores
:: e.g. 1.0.145 -> v1_0_145

:: Full Portable (self-contained, carries .NET 8)
dotnet publish src\CM26.App\CM26.App.csproj -c Release -r win-x64 --self-contained true ^
  -o publish_sc_v1_0_145
dotnet publish src\CM26.AssetBridge\CM26.AssetBridge.csproj -c Release -r win-x64 --self-contained true ^
  -o publish_assetbridge_sc_v1_0_145

:: Lite (framework-dependent, needs .NET 8 Desktop Runtime x64)
dotnet publish src\CM26.App\CM26.App.csproj -c Release -r win-x64 --self-contained false ^
  -o publish_lite_v1_0_145
dotnet publish src\CM26.AssetBridge\CM26.AssetBridge.csproj -c Release -r win-x64 --self-contained false ^
  -o publish_assetbridge_lite_v1_0_145
```

Then assemble, verify, zip and checksum:

```powershell
powershell -ExecutionPolicy Bypass -File Release\assemble_packages.ps1
```

To keep generated files in a designated external release folder, pass an
absolute output path:

```powershell
pwsh -File Release\assemble_packages.ps1 -ReleaseDirectory 'D:\CM 26 Final\Release'
```

The script reads the version from `version.json` only, and **fails** (rather than
warning) if any of these is true:

* `Directory.Build.props` disagrees with `version.json`
* release notes, installation instructions, limitations or checksum references name another version
* a publish folder is missing, or is older than the newest source file (stale)
* a required payload file is missing, or the bundled 3D viewer is absent
* the shipped `.exe` file version does not match the release version
* either packaged Full/Lite `--release-selftest` or `--ui-shell-smoke` does not pass
* any PDB debug symbol reached the package
* **any EA-derived game content reached the package** — see below

Output: `Release\Creation_Master_26_v<version>_{Full_Portable,Lite}` plus `.zip`
files and `Release\SHA256SUMS_v<version>.txt`.

## Versioning

`Directory.Build.props` (`CM26Version`) is the single source of truth for all
assembly versions. `version.json` carries the same number for the in-app update
check and the release script. **Bump both**, then update `README.md`,
`RELEASE_NOTES.md`, `KNOWN_LIMITATIONS.md` and `EULA.md`. The release script and
`--release-selftest` both fail on a mismatch.

## No EA content

Neither the repository nor the release packages may contain EA game content:
database tables, schema files, audio, textures, meshes or name lists. This is what
`EULA.md` promises users, so it is enforced in three places:

1. `.gitignore` blocks the relevant extensions (`*.db`, `*.fcepatch`, `*.dbc`,
   `*.dbp`, `*.big`, `*.ebx`, `*.res`, `*.chunk`, `*.sqlite`) and data folders.
2. `--release-selftest` scans the build output and fails if any appear.
3. `Release\assemble_packages.ps1` scans both packages — including inside archives,
   since a ZIP renamed to `.db` is how such data slipped into an earlier release —
   and refuses to produce a package.

The **CM26 Scraper is deliberately not bundled** for this reason. It is an optional
separate download that users point CM26 at from Settings or Data Sync.

## Repository layout

| Path | Contents |
|---|---|
| `src/CM26.App/` | WinForms UI, sections, headless diagnostic modes |
| `src/CM26.Application/` | Services and models over the engine |
| `src/CM26.AssetBridge/` | Separate-process Frostbite layout/TOC/CAS reader |
| `src/CM26.MeshKit/` | FC26 MeshSet parsing and FBX export |
| `src-native/CM26.EngineBridge/` | C++/CLI bridge to the native database engine |
| `src/database_engine.cpp` | Native t3db database engine |
| `tests/` | Native C++ regression probes |
| `Release/` | Packaging script and generated packages (git-ignored) |
| `docs/reports/` | Format research and audit reports |

`publish_3d_viewer/` is a prebuilt payload copied into `Tools\CM26.3DViewer`. A
publish fails loudly if it is missing, so the package can never ship without it.

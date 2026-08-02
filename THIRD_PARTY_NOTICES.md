# Third-Party Notices — Creation Master 26

## 2026-07-28 Frostbite bridge clarification

`CM26.AssetBridge` is an independently written layout/TOC/CAS reader and direct
legacy-chunk transaction writer. The build does not reference, bundle or redistribute FMT, FET,
FrostyToolsuite, DirectXTexNet or CSharpImageLibrary. Those repositories were
evaluated or consulted as format/workflow references only. `FMT.Core` was not
adopted because its published licence is unsuitable for the intended public
distribution model.

For Oodle-compressed FC26 payloads, CM26 dynamically calls
`oo2core_9_win64.dll` from the user's existing FC26 installation. CM26 does not
copy, bundle or redistribute that EA-shipped binary; extraction is unavailable
when the legitimate game installation does not provide it.

This application includes or references third-party components. Each is listed with its
licence and how it is used. Creation Master 26 itself does **not** copy any third-party source
code into its own codebase; dependencies are used as compiled libraries or read-only data.

## 1. Bundled runtime libraries (redistributed with the app)

| Component | Source | Licence | Use | Redistribution |
|-----------|--------|---------|-----|----------------|
| **Microsoft Visual C++ Runtime** (`msvcp140.dll`, `vcruntime140.dll`, `vcruntime140_1.dll`) | Microsoft Visual Studio 2022 | [Microsoft Visual Studio Redistributable Licence](https://learn.microsoft.com/visualstudio/releases/2022/redistribution) | Required by the native C++ engine bridge. Deployed **app-locally** so no separate install is needed. | Permitted under the VS redist terms for x64 CRT DLLs. |
| **.NET 8 runtime / SDK reference assemblies** | Microsoft | [MIT + .NET Library Licence](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT) | Target framework. The **framework-dependent** build loads the shared .NET 8 Desktop Runtime, which the user installs separately (not bundled). | Framework-dependent publish; runtime not redistributed. |
| **System.Drawing.Common** | Microsoft / .NET | MIT | Read-only image loading for standard formats (PNG/JPEG/BMP). Windows-only GDI+ wrapper. | NuGet package; MIT. |
| **System.Text.Encoding.CodePages** | Microsoft / .NET | MIT | CP1252 decoding for locale text. | NuGet package; MIT. |
| **Open XML SDK / DocumentFormat.OpenXml** | .NET Foundation / Microsoft | MIT | Reads and writes Compdata `.xlsx` workbooks. | NuGet package; MIT. |
| **AssimpNet / assimp** | Assimp project | BSD-3-Clause | Imported FBX scene loading in the separately packaged CM26 3D viewer. | Bundled with the 3D viewer under its licence. |
| **HelixToolkit.SharpDX / SharpDX** | Helix Toolkit / SharpDX projects | MIT | WPF/Direct3D rendering in the separately packaged CM26 3D viewer. | Bundled with the 3D viewer under their licences. |

## 2. DDS texture decoding — self-contained (no external library)

The miniface preview decodes **BC3/DXT5** DDS files using a **small self-contained decoder written
for this project** (`DdsDecoder.cs`). It implements the public BC1/BC3 block-compression format
directly in managed C#.

- **DirectXTexNet** (https://github.com/deng0/DirectXTexNet, MIT) was **evaluated but not bundled**.
  Rationale: the only DDS files present in the verified local asset set are BC3/DXT5 (and BC1/DXT1),
  which the self-contained decoder covers without taking on an external native dependency. If other
  BC formats appear in future, the texture service reports them as *unsupported* rather than
  mis-decoding.
- **CSharpImageLibrary** (https://github.com/KFreon/CSharpImageLibrary) was treated as a reference
  only (the project is archived) and is **not** bundled.

## 3. Read-only reference material (NOT distributed, NOT linked)

The following were consulted for **workflow and file-format understanding only**. None of their
code or assets is compiled into, linked by, or shipped with Creation Master 26.

| Reference | Use |
|-----------|-----|
| **CM16 source/decompile references** | Workflow/section-arrangement reference only. No code copied. |
| **DBM Studio** | Entity-ID and Compdata workflow research only. No source copied or bundled; no repository licence was present during review. |
| **FMT / FET / FrostyToolsuite / FMT.Releases** | Understanding Frostbite container, chunk and editor workflows; no source or binary is incorporated into CM26. |
| **vgmstream** | Considered for audio preview; not integrated. |

## 4. Read-only local asset data (user-supplied, never modified or redistributed by CM26)

The preview feature reads image files that already exist on the user's machine. Creation Master 26
**does not ship, modify, or write** these files; it only locates and displays them read-only.

| Data | Source | Status |
|------|--------|--------|
| Player minifaces (`p{id}.dds/.png`) | Local `miniface` pack | Read-only display |
| Ball / stadium / boot / glove / flag PNGs | Local `FC Editor by decoruiz Alpha v21` extracted art pack | Read-only display |

These are third-party game-art extractions owned by their respective authors/EA. The end user is
responsible for their own right to possess them. CM26 treats them strictly as read-only input and
never presents a placeholder as a real asset.

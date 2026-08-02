# CM26 asset support matrix

Status: Official Release 1.00, 2026-07-30.

| Category | Database/data view | Installed preview | Import/revert | 3D |
|---|---:|---:|---:|---:|
| Country flags/maps | Yes | Yes when mapped | Yes | N/A |
| League logos/banners | Yes | Yes when mapped | Yes | N/A |
| Team crests/flags | Yes | Yes when mapped | Yes | N/A |
| Sponsors/adboards | Yes | Yes when mapped | Yes | N/A |
| Player minifaces | Yes | Yes when mapped | Supported legacy target | N/A |
| Player face | Yes | 2D miniface/legacy preview | Legacy target where mapped | Exported FBX viewer |
| Manager face | Yes | Yes when mapped | Yes | N/A |
| Kits | Yes | Installed texture | Yes | Exported FBX viewer |
| Competition logos | Yes | Yes when mapped | Yes | N/A |
| Balls | Yes | Installed/fallback texture | Yes | Exported FBX viewer |
| Boots | Yes | Installed/fallback texture | Yes | Exported FBX viewer |
| Goalkeeper gloves | Yes | Installed/fallback texture | Yes | N/A |
| NewWave audio banks | Mapped catalogs | Bank/dataset inspector | Raw bank export only | N/A |

“Import/revert” stages a replacement for an exact legacy path, then Save commits
it through the direct writer. Revert removes the staged replacement and retains
the installed original. A missing or unresolved installed legacy path is shown
as unavailable and is not guessed during commit.

All public record editors use explicit CM16-style mappings. There is no **All
FC26 Data** or raw-field tab.

# Creation Master 26 — Public Release Matrix

Version: 1.0.183
Date: 2026-08-31
Public package: Full Portable (Windows 10/11 x64)

This matrix separates checks that were actually executed from checks requiring
another physical machine or an EA title update that is not available locally.
`NOT TESTED` never means pass.

## Automated release gates

| Gate | Result | Evidence |
|---|---|---|
| Release solution build | PASS | Release build completes with 0 warnings and 0 errors. |
| Regression suite | PASS | 155 tests, including Compdata leagues with 2/4/12/20/24 teams. |
| Public executable identity | PASS | `Creation Master 26.exe`, assembly identity and package manifest agree. |
| Compdata integrity | PASS | Hierarchy, database mapping, teams, standings, schedule, conflicts and TXT round-trip are validated. |
| Direct-save failure recovery | PASS (code gate) | Atomic journal covers prepare/write/replace/complete/rollback and preserves failed transaction evidence. |
| Friendly diagnostics | PASS (code gate) | Startup and supported save workflows issue diagnostic IDs instead of raw exception dialogs. |
| Package self-test | PASS | Full Portable and internal Lite validation both returned `RELEASE SELF-TEST OK`. |
| UI shell smoke | PASS | Full Portable and internal Lite shell smoke both passed. |
| Package integrity | PASS | 573-file Full Portable payload; Large Address Aware, version, dependencies, no-PDB and no-EA-content gates passed. |
| ZIP checksum | PASS | Final Full Portable SHA-256 is recorded in `Release/SHA256SUMS_v1.0.183.txt`. |

## Machine-local integration evidence

| Area | Result | Boundary |
|---|---|---|
| Installed FC26 asset indexing and Compdata extraction | PASS on development machine | Does not prove every future EA title update. |
| Real 11-file FC26 Compdata snapshot validation | PASS | Validated snapshot contains 90 competitions; no database content is redistributed. |
| Direct Frostbite prepare/verify | PASS | Live commit only occurs after an explicit user Save while FC26 is closed. |
| Career container budget write/reopen verification | PASS on verified sample workflow | Other unknown Career structures remain unsupported. |

## External certification still required

| Check | Status | Release meaning |
|---|---|---|
| Clean Windows 10 physical PC | NOT TESTED | Full Portable should be verified on a machine without the SDK. |
| Clean Windows 11 physical PC | NOT TESTED | Full Portable should be verified on a machine without the SDK. |
| 100% / 125% / 150% DPI on different GPUs | NOT TESTED | Automated layout smoke does not replace physical display checks. |
| Every FC26 EA title update | NOT TESTED | Save Preflight validates mapped schema, but a changed unknown format may need an update. |
| Brand-new created league through a full played Career season | NOT TESTED externally | Structural generation is automated and round-tripped; long-form gameplay still needs community validation. |
| Authenticode signature / trusted publisher | NOT AVAILABLE | Build is unsigned; verify the published SHA-256 checksum. |

## Public verdict

The build passed its automated release gates and is suitable for a public,
checksum-verified release. It must not be advertised as universally bug-free or
certified for unknown future FC26 updates. Users should close FC26, keep an
independent backup and start a new Career after database or Compdata structure
changes.

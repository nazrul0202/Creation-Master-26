# Assembles the Full Portable and Lite release packages from the publish outputs.
# Run from the project root. Strips PDBs, refreshes VC++ CRT from System32, copies docs.
$ErrorActionPreference = 'Stop'
$root   = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$sys32  = Join-Path $env:SystemRoot 'System32'
$version = '1.0.22'
$publishVersion = $version.Replace('.', '_')

# VC++ CRT DLLs to bundle app-locally (refresh from this machine's System32).
$vcrt = @('msvcp140.dll', 'vcruntime140.dll', 'vcruntime140_1.dll')

# Shared documentation set (main docs at repo root, reports under docs/reports).
$rootDocs = @('README.md', 'INSTALLATION.md', 'RELEASE_NOTES.md',
              'KNOWN_LIMITATIONS.md', 'THIRD_PARTY_NOTICES.md',
              'docs\reports\ASSET_INVENTORY.md', 'docs\reports\ASSET_SUPPORT_MATRIX.md',
              'docs\reports\FROSTBITE_ASSET_BRIDGE_STATUS.md',
              'docs\reports\RELEASE_READINESS_REPORT.md')

function Assemble-Package {
    param(
        [string]$PublishDir,   # source publish output
        [string]$PackageDir,   # destination package folder
        [string]$BridgeDir     # matching self-contained/framework-dependent bridge publish
    )
    Write-Host "=== Assembling $PackageDir from $PublishDir ==="
    if (Test-Path $PackageDir) { Remove-Item $PackageDir -Recurse -Force }
    New-Item -ItemType Directory -Path $PackageDir | Out-Null

    # Copy app payload (exclude PDB debug symbols).
    Get-ChildItem -Path $PublishDir -Recurse -File |
        Where-Object { $_.Extension -ne '.pdb' } |
        ForEach-Object {
            $rel = $_.FullName.Substring($PublishDir.Length).TrimStart('\')
            $dest = Join-Path $PackageDir $rel
            New-Item -ItemType Directory -Path (Split-Path $dest) -Force | Out-Null
            Copy-Item $_.FullName $dest -Force
        }

    # The asset bridge is a separate process. Full Portable receives its own
    # self-contained bridge; Lite receives the framework-dependent bridge.
    Get-ChildItem -Path $BridgeDir -File |
        Where-Object { $_.Extension -ne '.pdb' } |
        ForEach-Object { Copy-Item $_.FullName (Join-Path $PackageDir $_.Name) -Force }

    # Refresh VC++ CRT app-locally from System32 (matches the toolchain that built the bridge).
    foreach ($d in $vcrt) {
        $src = Join-Path $sys32 $d
        if (Test-Path $src) { Copy-Item $src (Join-Path $PackageDir $d) -Force }
        else { Write-Warning "VC++ CRT not found in System32: $d" }
    }

    # Documentation (create destination subfolders for docs copied from docs\reports).
    foreach ($d in $rootDocs) {
        $dest = Join-Path $PackageDir $d
        New-Item -ItemType Directory -Path (Split-Path $dest) -Force | Out-Null
        Copy-Item (Join-Path $root $d) $dest -Force
    }

    # Sanity report.
    $files = Get-ChildItem $PackageDir -Recurse -File
    $mb = ($files | Measure-Object Length -Sum).Sum / 1MB
    Write-Host ("    files={0}  size={1:N1} MB" -f $files.Count, $mb)
    $must = @('CM26_by_Rizco98.exe','CM26_by_Rizco98.dll','CM26.Application.dll',
              'CM26.EngineBridge.dll','CM26.AssetBridge.exe','CM26.AssetBridge.dll',
              'Ijwhost.dll','msvcp140.dll','vcruntime140.dll','vcruntime140_1.dll',
              'README.md','KNOWN_LIMITATIONS.md','THIRD_PARTY_NOTICES.md','RELEASE_NOTES.md','INSTALLATION.md')
    foreach ($m in $must) {
        if (-not (Test-Path (Join-Path $PackageDir $m))) { Write-Warning "MISSING: $m" }
    }
    if (Get-ChildItem $PackageDir -Recurse -Filter '*.pdb' -ErrorAction SilentlyContinue) {
        Write-Warning 'PDB files still present!'
    }
    return $true
}

Assemble-Package (Join-Path $root "publish_sc_v$publishVersion") `
    (Join-Path $root "Release\Creation_Master_26_v$version`_Full_Portable") `
    (Join-Path $root "publish_assetbridge_sc_v$publishVersion") | Out-Null
Assemble-Package (Join-Path $root "publish_lite_v$publishVersion") `
    (Join-Path $root "Release\Creation_Master_26_v$version`_Lite") `
    (Join-Path $root "publish_assetbridge_lite_v$publishVersion") | Out-Null
Write-Host '=== DONE ==='

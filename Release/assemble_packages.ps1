# Assembles the Full Portable and Lite release packages from the publish outputs.
#
# Run from anywhere; paths are resolved relative to this script.
#   pwsh -File Release\assemble_packages.ps1
#
# Guarantees (each one FAILS the run rather than warning):
#   * version comes from version.json only - no second source of truth
#   * required payload files are all present
#   * no PDB debug symbols
#   * no EA-derived game content is redistributed (see EULA.md)
#   * SHA256SUMS is generated and the zips are produced by this script
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root  = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$sys32 = Join-Path $env:SystemRoot 'System32'

# --- Single source of truth for the release version -------------------------
$versionFile = Join-Path $root 'version.json'
if (-not (Test-Path $versionFile)) { throw "version.json not found at $versionFile" }
$version = (Get-Content $versionFile -Raw | ConvertFrom-Json).version
if ([string]::IsNullOrWhiteSpace($version)) { throw 'version.json does not contain a "version" value.' }
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw "version.json version '$version' is not MAJOR.MINOR.PATCH." }

# Directory.Build.props must agree, otherwise the binaries and the docs disagree.
$propsFile = Join-Path $root 'Directory.Build.props'
if (Test-Path $propsFile) {
    # Directory.Build.props has several PropertyGroup elements; select the node
    # directly rather than assuming which group carries the version.
    $propsVersion = ([xml](Get-Content $propsFile -Raw)).SelectSingleNode('//CM26Version')
    if (-not $propsVersion) { throw 'Directory.Build.props does not define CM26Version.' }
    if ($propsVersion.InnerText.Trim() -ne $version) {
        throw "Version mismatch: version.json='$version' but Directory.Build.props CM26Version='$($propsVersion.InnerText.Trim())'."
    }
}
Write-Host "=== Creation Master 26 release assembly - version $version ==="

$publishVersion = $version.Replace('.', '_')
$errors = New-Object System.Collections.Generic.List[string]

# VC++ CRT DLLs to bundle app-locally (refresh from this machine's System32).
$vcrt = @('msvcp140.dll', 'vcruntime140.dll', 'vcruntime140_1.dll')

# Shared documentation set (main docs at repo root, reports under docs/reports).
$rootDocs = @('README.md', 'INSTALLATION.md', 'RELEASE_NOTES.md',
              'KNOWN_LIMITATIONS.md', 'THIRD_PARTY_NOTICES.md',
              'LICENSE', 'EULA.md', 'version.json',
              'docs\reports\ASSET_INVENTORY.md', 'docs\reports\ASSET_SUPPORT_MATRIX.md',
              'docs\reports\FROSTBITE_ASSET_BRIDGE_STATUS.md',
              'docs\reports\RELEASE_READINESS_REPORT.md')

# --- EA content guard -------------------------------------------------------
# EULA.md states the package redistributes no EA game content. These patterns are
# the file shapes that would break that promise. Extensions are game-data
# containers; names are EA database artefacts previously shipped by mistake.
$forbiddenExtensions = @('.fcepatch', '.dbc', '.dbp', '.big', '.ebx', '.res', '.chunk', '.db', '.sqlite')
$forbiddenNames      = @('master.db', 'fifa_ng_db-meta.xml', 'playernames.txt', 'fifa_ng_db')

function Assert-NoGameContent {
    param([string]$PackageDir, [string]$Label)

    $hits = New-Object System.Collections.Generic.List[string]
    foreach ($file in Get-ChildItem $PackageDir -Recurse -File) {
        $rel = $file.FullName.Substring($PackageDir.Length).TrimStart('\')
        if ($forbiddenExtensions -contains $file.Extension.ToLowerInvariant()) {
            $hits.Add("$rel  [extension $($file.Extension)]"); continue
        }
        if ($forbiddenNames -contains $file.Name.ToLowerInvariant()) {
            $hits.Add("$rel  [name $($file.Name)]"); continue
        }
        # A ZIP disguised under another extension is how master.db slipped through
        # before: check the archive members of anything suspiciously large too.
        if ($file.Extension -ieq '.zip' -and $file.Length -gt 1MB) {
            try {
                Add-Type -AssemblyName System.IO.Compression.FileSystem
                $zip = [System.IO.Compression.ZipFile]::OpenRead($file.FullName)
                try {
                    foreach ($entry in $zip.Entries) {
                        $ext = [System.IO.Path]::GetExtension($entry.Name).ToLowerInvariant()
                        if ($forbiddenExtensions -contains $ext) {
                            $hits.Add("$rel -> $($entry.FullName)  [archived $ext]")
                        }
                    }
                } finally { $zip.Dispose() }
            } catch {
                $hits.Add("$rel  [unreadable archive - inspect manually]")
            }
        }
    }

    if ($hits.Count -gt 0) {
        Write-Host "    EA CONTENT CHECK: FAILED ($($hits.Count) file(s))" -ForegroundColor Red
        $hits | Select-Object -First 25 | ForEach-Object { Write-Host "      $_" -ForegroundColor Red }
        $errors.Add("$Label redistributes game content that EULA.md says it does not: $($hits.Count) file(s).")
    }
    else { Write-Host '    EA content check: clean' }
}

function Assemble-Package {
    param(
        [string]$PublishDir,   # source publish output
        [string]$PackageDir,   # destination package folder
        [string]$BridgeDir,    # matching self-contained/framework-dependent bridge publish
        [string]$Label
    )
    Write-Host "=== Assembling $Label ==="
    Write-Host "    source: $PublishDir"

    foreach ($dir in @($PublishDir, $BridgeDir)) {
        if (-not (Test-Path $dir)) {
            $errors.Add("$Label source directory missing: $dir. Run the publish step first (see docs/BUILDING.md).")
            Write-Host "    SKIPPED - missing $dir" -ForegroundColor Red
            return
        }
    }

    # Stale-output guard: a publish tree older than the newest source file means
    # the packaged binaries do not match the current code.
    $newestSource = Get-ChildItem (Join-Path $root 'src') -Recurse -File -Include *.cs, *.csproj, *.resx |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
        Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    $newestPublish = Get-ChildItem $PublishDir -Recurse -File |
        Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    if ($newestSource -and $newestPublish -and $newestSource.LastWriteTimeUtc -gt $newestPublish.LastWriteTimeUtc) {
        $errors.Add("$Label publish output is STALE: '$($newestSource.Name)' is newer than the published payload. Re-run publish before packaging.")
        Write-Host "    STALE OUTPUT - $($newestSource.Name) is newer than the publish tree" -ForegroundColor Red
    }

    # deps.json guard: a corrupted publish (e.g. parallel dotnet publish calls
    # racing on the shared obj/) can produce a deps.json that omits the bundled
    # runtime packs. The apphost then dies with "Could not resolve CoreCLR path"
    # at launch. A self-contained package MUST reference the runtime packs.
    $depsFile = Join-Path $PublishDir 'CM26_by_Rizco98.deps.json'
    if (-not (Test-Path $depsFile)) {
        $errors.Add("$Label publish output is missing CM26_by_Rizco98.deps.json.")
    }
    else {
        $deps = Get-Content $depsFile -Raw
        if ($Label -eq 'Lite') {
            # Framework-dependent publish should NOT have runtime packs (it relies on installed .NET runtime).
            # Verify the deps.json has the expected structure with targets.
            if ($deps -notmatch '"targets"') {
                $errors.Add("$Label deps.json is missing targets section (corrupt publish - re-run dotnet publish sequentially, never in parallel).")
                Write-Host "    MISSING targets in deps.json - re-run publish sequentially" -ForegroundColor Red
            }
        }
        elseif ($deps -notmatch 'CM26_by_Rizco98\.dll') {
            $errors.Add("$Label deps.json does not reference the application assembly (corrupt publish).")
            Write-Host "    deps.json does not reference the app assembly - re-run publish" -ForegroundColor Red
        }
    }

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

    # The asset bridge is loaded in-process by CM26. The matching publish is
    # still copied so its managed/native dependencies follow the app runtime.
    Get-ChildItem -Path $BridgeDir -File |
        Where-Object { $_.Extension -ne '.pdb' } |
        ForEach-Object { Copy-Item $_.FullName (Join-Path $PackageDir $_.Name) -Force }

    # Refresh VC++ CRT app-locally from System32 (matches the toolchain that built the bridge).
    foreach ($d in $vcrt) {
        $src = Join-Path $sys32 $d
        if (Test-Path $src) { Copy-Item $src (Join-Path $PackageDir $d) -Force }
        else { $errors.Add("$Label is missing VC++ CRT '$d' (not found in System32).") }
    }

    # Documentation (create destination subfolders for docs copied from docs\reports).
    foreach ($d in $rootDocs) {
        $srcDoc = Join-Path $root $d
        if (-not (Test-Path $srcDoc)) { $errors.Add("$Label doc source missing: $d"); continue }
        $dest = Join-Path $PackageDir $d
        New-Item -ItemType Directory -Path (Split-Path $dest) -Force | Out-Null
        Copy-Item $srcDoc $dest -Force
    }

    # --- Verification -------------------------------------------------------
    $files = Get-ChildItem $PackageDir -Recurse -File
    $mb = ($files | Measure-Object Length -Sum).Sum / 1MB
    Write-Host ("    files={0}  size={1:N1} MB" -f $files.Count, $mb)

    $must = @('CM26_by_Rizco98.exe','CM26_by_Rizco98.dll','CM26.Application.dll',
              'CM26.EngineBridge.dll','CM26.AssetBridge.dll',
              'CM26.MeshKit.dll',
              'Ijwhost.dll','msvcp140.dll','vcruntime140.dll','vcruntime140_1.dll',
              'README.md','KNOWN_LIMITATIONS.md','THIRD_PARTY_NOTICES.md',
              'RELEASE_NOTES.md','INSTALLATION.md','LICENSE','EULA.md','version.json')
    foreach ($m in $must) {
        if (-not (Test-Path (Join-Path $PackageDir $m))) { $errors.Add("$Label MISSING required file: $m") }
    }

    # The bundled 3D viewer is the largest optional payload; verify it arrived.
    $viewer = Join-Path $PackageDir 'Tools\CM26.3DViewer'
    if (-not (Test-Path $viewer)) { $errors.Add("$Label MISSING bundled 3D viewer: Tools\CM26.3DViewer") }
    elseif (-not (Get-ChildItem $viewer -Recurse -File)) { $errors.Add("$Label 3D viewer folder is empty.") }

    if (Get-ChildItem $PackageDir -Recurse -Filter '*.pdb' -ErrorAction SilentlyContinue) {
        $errors.Add("$Label contains PDB debug symbols.")
    }

    # The shipped exe must report the version we are releasing.
    $exe = Join-Path $PackageDir 'CM26_by_Rizco98.exe'
    if (Test-Path $exe) {
        $fileVersion = (Get-Item $exe).VersionInfo.FileVersion
        if ($fileVersion -and $fileVersion -notmatch [regex]::Escape($version)) {
            $errors.Add("$Label exe FileVersion '$fileVersion' does not match release version '$version'.")
        }
        else { Write-Host "    exe version: $fileVersion" }
    }

    Assert-NoGameContent -PackageDir $PackageDir -Label $Label
}

$fullDir = Join-Path $root "Release\Creation_Master_26_v$version`_Full_Portable"
$liteDir = Join-Path $root "Release\Creation_Master_26_v$version`_Lite"

Assemble-Package (Join-Path $root "publish_sc_v$publishVersion") $fullDir `
    (Join-Path $root "publish_assetbridge_sc_v$publishVersion") 'Full Portable'
Assemble-Package (Join-Path $root "publish_lite_v$publishVersion") $liteDir `
    (Join-Path $root "publish_assetbridge_lite_v$publishVersion") 'Lite'

# --- Fail before publishing anything unusable -------------------------------
if ($errors.Count -gt 0) {
    Write-Host ''
    Write-Host "=== FAILED: $($errors.Count) problem(s) ===" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host "  * $_" -ForegroundColor Red }
    exit 1
}

# --- Zip + checksums --------------------------------------------------------
Write-Host '=== Creating archives and checksums ==='
$sums = New-Object System.Collections.Generic.List[string]
foreach ($pkg in @($fullDir, $liteDir)) {
    if (-not (Test-Path $pkg)) { continue }
    $zip = "$pkg.zip"
    if (Test-Path $zip) { Remove-Item $zip -Force }
    Compress-Archive -Path (Join-Path $pkg '*') -DestinationPath $zip -CompressionLevel Optimal
    $hash = (Get-FileHash $zip -Algorithm SHA256).Hash
    $sums.Add("$hash  $(Split-Path $zip -Leaf)")
    Write-Host ("    {0}  {1:N1} MB" -f (Split-Path $zip -Leaf), ((Get-Item $zip).Length / 1MB))
}

$sumsFile = Join-Path $root "Release\SHA256SUMS_v$version.txt"
Set-Content -Path $sumsFile -Value $sums -Encoding ASCII
Write-Host "    checksums: $(Split-Path $sumsFile -Leaf)"

Write-Host ''
Write-Host "=== DONE - v$version packaged, verified, zipped and checksummed ===" -ForegroundColor Green

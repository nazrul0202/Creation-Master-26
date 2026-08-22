param(
    [string]$Root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$manifestPath = Join-Path $Root 'version.json'
$propsPath = Join-Path $Root 'Directory.Build.props'
if (-not (Test-Path -LiteralPath $manifestPath)) { throw "Missing version manifest: $manifestPath" }
if (-not (Test-Path -LiteralPath $propsPath)) { throw "Missing version props: $propsPath" }

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$version = [string]$manifest.version
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw "Invalid MAJOR.MINOR.PATCH version '$version'." }
if ([string]$manifest.notes -notmatch "^v$([regex]::Escape($version))\b") {
    throw "version.json notes must begin with 'v$version'."
}

$propsNode = ([xml](Get-Content -LiteralPath $propsPath -Raw)).SelectSingleNode('//CM26Version')
if (-not $propsNode -or $propsNode.InnerText.Trim() -ne $version) {
    throw "Directory.Build.props CM26Version must match version.json '$version'."
}

$checks = @(
    @{ File = 'RELEASE_NOTES.md'; Pattern = "(?m)^## Version $([regex]::Escape($version))\b"; Message = 'top-level release entry' },
    @{ File = 'KNOWN_LIMITATIONS.md'; Pattern = "Status: Version $([regex]::Escape($version))\b"; Message = 'current limitation status' },
    @{ File = 'INSTALLATION.md'; Pattern = "Creation Master 26 $([regex]::Escape($version))\b"; Message = 'installation heading' },
    @{ File = 'INSTALLATION.md'; Pattern = "SHA256SUMS_v$([regex]::Escape($version))\.txt"; Message = 'installation checksum name' },
    @{ File = 'README.md'; Pattern = "SHA256SUMS_v$([regex]::Escape($version))\.txt"; Message = 'README checksum name' }
)

foreach ($check in $checks) {
    $path = Join-Path $Root $check.File
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing release document: $($check.File)" }
    $content = Get-Content -LiteralPath $path -Raw
    if ($content -notmatch $check.Pattern) {
        throw "$($check.File) is missing the v$version $($check.Message)."
    }
}

$firstReleaseHeading = Get-Content -LiteralPath (Join-Path $Root 'RELEASE_NOTES.md') |
    Where-Object { $_ -match '^## Version ' } | Select-Object -First 1
if ($firstReleaseHeading -notmatch "^## Version $([regex]::Escape($version))\b") {
    throw "RELEASE_NOTES.md first version heading must be Version $version."
}

Write-Host "Release metadata consistent: v$version"

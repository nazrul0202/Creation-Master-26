param(
    [Parameter(Mandatory = $true)]
    [string]$Path
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$resolved = (Resolve-Path -LiteralPath $Path).Path
$bytes = [System.IO.File]::ReadAllBytes($resolved)
if ($bytes.Length -lt 64 -or $bytes[0] -ne 0x4D -or $bytes[1] -ne 0x5A) {
    throw "'$resolved' is not a valid Windows PE image."
}

$peOffset = [System.BitConverter]::ToInt32($bytes, 0x3C)
$characteristicsOffset = $peOffset + 22
if ($peOffset -lt 0 -or $characteristicsOffset + 1 -ge $bytes.Length -or
    $bytes[$peOffset] -ne 0x50 -or $bytes[$peOffset + 1] -ne 0x45 -or
    $bytes[$peOffset + 2] -ne 0 -or $bytes[$peOffset + 3] -ne 0) {
    throw "'$resolved' has an invalid PE header."
}

$largeAddressAware = 0x20
$characteristics = [System.BitConverter]::ToUInt16($bytes, $characteristicsOffset)
if (($characteristics -band $largeAddressAware) -eq 0) {
    $updated = [uint16]($characteristics -bor $largeAddressAware)
    $encoded = [System.BitConverter]::GetBytes($updated)
    $bytes[$characteristicsOffset] = $encoded[0]
    $bytes[$characteristicsOffset + 1] = $encoded[1]
    [System.IO.File]::WriteAllBytes($resolved, $bytes)
}

$verified = [System.BitConverter]::ToUInt16(
    [System.IO.File]::ReadAllBytes($resolved), $characteristicsOffset)
if (($verified -band $largeAddressAware) -eq 0) {
    throw "Failed to enable Large Address Aware on '$resolved'."
}

Write-Host "Large Address Aware: $resolved"

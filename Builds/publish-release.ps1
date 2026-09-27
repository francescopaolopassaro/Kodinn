# Publishes a Kodinn release on GitHub with the installers attached.
#
#   powershell -ExecutionPolicy Bypass -File Builds\publish-release.ps1 -Version 1.2.0
#   powershell -ExecutionPolicy Bypass -File Builds\publish-release.ps1 -Version 1.2.0 -Files setup1.exe,setup2.pkg
#
# Without -Files it takes every installer in Builds\ (windows\, windows\msix\, macos\).
# The checksums are rewritten first (update-checksums.ps1) and attached too.
# Needs the GitHub CLI (gh) logged in.

param(
    [Parameter(Mandatory = $true)][string]$Version,
    [string[]]$Files = @(),
    [string]$Notes = ""
)

$ErrorActionPreference = "Stop"
$here = $PSScriptRoot

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw "GitHub CLI (gh) not found: https://cli.github.com/" }

if ($Files.Count -eq 0) {
    $extensions = @(".exe", ".msi", ".msix", ".msixbundle", ".cer", ".pkg", ".dmg", ".zip")
    $Files = Get-ChildItem -Path $here -Recurse -File |
        Where-Object { $extensions -contains $_.Extension.ToLowerInvariant() -and $_.FullName -notlike "*\nuget\*" } |
        ForEach-Object { $_.FullName }
    if ($Files.Count -eq 0) { throw "No installers in $here" }
}
foreach ($f in $Files) { if (-not (Test-Path $f)) { throw "File not found: $f" } }

& (Join-Path $here "update-checksums.ps1")
$sumsFile = Join-Path $here "SHA256SUMS.txt"

$tag = "v$Version"
if ([string]::IsNullOrWhiteSpace($Notes)) { $Notes = "Kodinn $Version" }
gh release create $tag @Files $sumsFile --title "Kodinn $Version" --notes $Notes
if ($LASTEXITCODE -ne 0) { throw "gh release create failed" }
Write-Host "Release $tag published."

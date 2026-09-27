# Rewrites Builds\SHA256SUMS.txt with the SHA-256 of every installer in Builds\
# (windows\, windows\msix\, macos\). Run after adding or replacing a setup;
# Kodinn's installer build runs it automatically.
#
#   powershell -ExecutionPolicy Bypass -File Builds\update-checksums.ps1

$ErrorActionPreference = "Stop"
$here = $PSScriptRoot
$extensions = @(".exe", ".msi", ".msix", ".msixbundle", ".cer", ".pkg", ".dmg", ".zip")

$files = Get-ChildItem -Path $here -Recurse -File |
    Where-Object { $extensions -contains $_.Extension.ToLowerInvariant() -and $_.FullName -notlike "*\nuget\*" } |
    Sort-Object FullName

$lines = foreach ($f in $files) {
    $h = (Get-FileHash $f.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    $rel = $f.FullName.Substring($here.Length + 1).Replace('\', '/')
    "$h  $rel"
}

$sumsFile = Join-Path $here "SHA256SUMS.txt"
if ($lines) { $lines | Set-Content -Path $sumsFile -Encoding utf8 } else { Set-Content -Path $sumsFile -Value "" -Encoding utf8 }
Write-Host "SHA256SUMS.txt: $(@($lines).Count) file(s)"

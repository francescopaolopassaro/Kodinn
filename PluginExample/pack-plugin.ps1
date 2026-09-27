# Builds the plugin and packs it as PluginExample.kodinn-plugin (a zip with
# plugin.json at its root), ready for Kodinn: Settings > Plugins > Install.
#
#   powershell -ExecutionPolicy Bypass -File pack-plugin.ps1

$ErrorActionPreference = "Stop"
$here = $PSScriptRoot
$out = Join-Path $here "bin\Release\net10.0"

dotnet build (Join-Path $here "PluginExample.csproj") -c Release
if ($LASTEXITCODE -ne 0) { throw "build failed" }

$stage = Join-Path $here "bin\package"
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory $stage | Out-Null

# The plugin's own files only: Kodinn provides Kodinn.Sdk and the Blazor runtime.
Get-ChildItem $out -File |
    Where-Object { $_.Name -notlike "Kodinn.Sdk.*" -and $_.Name -notlike "Microsoft.AspNetCore.*" -and $_.Extension -ne ".pdb" } |
    Copy-Item -Destination $stage

$package = Join-Path $here "PluginExample.kodinn-plugin"
if (Test-Path $package) { Remove-Item $package -Force }
Compress-Archive -Path (Join-Path $stage "*") -DestinationPath ($package + ".zip")
Move-Item ($package + ".zip") $package

Write-Host "Package: $package"

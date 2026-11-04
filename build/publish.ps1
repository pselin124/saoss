#Requires -Version 5.1
<#
.SYNOPSIS
    Tests and publishes the self-contained Windows x64 release, zipped to artifacts\Saoss-<version>-win-x64.zip.
#>
[CmdletBinding()]
param(
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\Saoss.Game\Saoss.Game.csproj'
$solution = Join-Path $root 'Saoss.slnx'
$artifacts = Join-Path $root 'artifacts'

function Invoke-Dotnet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($args -join ' ') failed with exit code $LASTEXITCODE"
    }
}

$version = (& dotnet msbuild $project -nologo -getProperty:Version).Trim()
if ($LASTEXITCODE -ne 0 -or -not $version) {
    throw 'Could not read the version from the project.'
}

$name = "Saoss-$version-win-x64"
$publishDir = Join-Path $artifacts $name
$zipPath = Join-Path $artifacts "$name.zip"

if (-not $SkipTests) {
    Write-Host 'Running tests...'
    Invoke-Dotnet test --solution $solution -c Release
}

foreach ($path in @($publishDir, $zipPath)) {
    if (Test-Path $path) {
        Remove-Item $path -Recurse -Force
    }
}

Write-Host "Publishing $name..."
Invoke-Dotnet publish $project -c Release -p:PublishProfile=win-x64 -o $publishDir

Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($publishDir, $zipPath, [IO.Compression.CompressionLevel]::Optimal, $true)

$files = Get-ChildItem $publishDir -Recurse -File
$folderMb = ($files | Measure-Object Length -Sum).Sum / 1MB
$zipMb = (Get-Item $zipPath).Length / 1MB
Write-Host ('Published {0} files ({1:N1} MB) to {2}' -f $files.Count, $folderMb, $publishDir)
Write-Host ('Zip: {0} ({1:N1} MB)' -f $zipPath, $zipMb)

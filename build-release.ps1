param (
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

# Configuration
$ProjectDir = "$PSScriptRoot\src\WindowMux"
$ProjectFile = "$ProjectDir\WindowMux.csproj"
$OutputDir = "$ProjectDir\bin\Release\net8.0-windows\win-x64\publish"
$ZipName = "WindowMux-$Version-win-x64.zip"
$ZipPath = "$PSScriptRoot\$ZipName"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " Building WindowMux Release: $Version" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Clean previous build
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
}
if (Test-Path $ZipPath) {
    Remove-Item -Path $ZipPath -Force
}

# 2. Publish
Write-Host "Publishing project..." -ForegroundColor Yellow

# Strip "V" prefix for distinct numeric version (e.g. "V1.0" -> "1.0")
$NumericVersion = $Version -replace "^[vV]", ""

# Note: Project is already configured for SelfContained/SingleFile in .csproj
dotnet publish $ProjectFile -c Release -r win-x64 /p:DebugType=None /p:DebugSymbols=false /p:Version=$NumericVersion /p:AssemblyVersion=$NumericVersion /p:FileVersion=$NumericVersion

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
}

# 3. Verify Output
if (-not (Test-Path "$OutputDir\WindowMux.exe")) {
    Write-Error "Output executable not found at $OutputDir\WindowMux.exe"
}

# 4. Package
Write-Host "Creating archive: $ZipName" -ForegroundColor Yellow
Compress-Archive -Path "$OutputDir\*" -DestinationPath $ZipPath

Write-Host "==========================================" -ForegroundColor Green
Write-Host " Success!" -ForegroundColor Green
Write-Host " Archive created at: $ZipPath" -ForegroundColor Gray
Write-Host "==========================================" -ForegroundColor Green

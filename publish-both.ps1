# =======================================================================
# PharmacistRecommendation - Publish Both Versions
# This script publishes both Server and Client versions
# =======================================================================

Write-Host ""
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "   PharmacistRecommendation - Publishing Server and Client Versions" -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

# Save current directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Check if we're in the right directory
if (-not (Test-Path "PharmacistRecommendation\PharmacistRecommendation.csproj")) {
    Write-Host "ERROR: PharmacistRecommendation.csproj not found!" -ForegroundColor Red
    Write-Host "Please run this script from the solution root directory." -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

# Function to handle errors
function Handle-Error {
    param([string]$Message)
    Write-Host ""
    Write-Host "========================================================================" -ForegroundColor Red
    Write-Host "ERROR: $Message" -ForegroundColor Red
    Write-Host "========================================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "  - .NET 8 SDK not installed" -ForegroundColor Yellow
    Write-Host "  - Project files corrupted" -ForegroundColor Yellow
    Write-Host "  - Insufficient disk space" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

# Optional: Clean previous builds
# Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
# dotnet clean PharmacistRecommendation\PharmacistRecommendation.csproj
# if ($LASTEXITCODE -ne 0) { Handle-Error "Clean failed" }
# Write-Host ""

# Publish Server version
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "[1/2] Publishing SERVER version..." -ForegroundColor Green
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

Set-Location PharmacistRecommendation

dotnet publish -c Release -p:PublishProfile=Properties\PublishProfiles\Server.pubxml

if ($LASTEXITCODE -ne 0) {
    Set-Location $ScriptDir
    Handle-Error "Server publish failed"
}

Write-Host ""
Write-Host "Server version published successfully!" -ForegroundColor Green
Write-Host "Output: bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\" -ForegroundColor Yellow
Write-Host ""

# Publish Client version
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "[2/2] Publishing CLIENT version..." -ForegroundColor Green
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

dotnet publish -c Release -p:PublishProfile=Properties\PublishProfiles\Client.pubxml

if ($LASTEXITCODE -ne 0) {
    Set-Location $ScriptDir
    Handle-Error "Client publish failed"
}

Write-Host ""
Write-Host "Client version published successfully!" -ForegroundColor Green
Write-Host "Output: bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\" -ForegroundColor Yellow
Write-Host ""

# Return to original directory
Set-Location $ScriptDir

# Success message
Write-Host "========================================================================" -ForegroundColor Green
Write-Host "SUCCESS! Both versions published successfully" -ForegroundColor Green
Write-Host "========================================================================" -ForegroundColor Green
Write-Host ""

$ServerPath = Join-Path $ScriptDir "PharmacistRecommendation\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server"
$ClientPath = Join-Path $ScriptDir "PharmacistRecommendation\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client"

Write-Host "Server output folder:" -ForegroundColor Cyan
Write-Host "  $ServerPath" -ForegroundColor Yellow
Write-Host ""
Write-Host "Client output folder:" -ForegroundColor Cyan
Write-Host "  $ClientPath" -ForegroundColor Yellow
Write-Host ""

Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. For SERVER: Copy Server folder to the server computer" -ForegroundColor White
Write-Host "  2. For CLIENTS: Copy Client folder to each client computer" -ForegroundColor White
Write-Host ""
Write-Host "See COMPLETE_DEPLOYMENT_GUIDE.md for detailed installation instructions." -ForegroundColor Yellow
Write-Host ""

# Optional: Open output folders
$OpenFolders = Read-Host "Would you like to open the output folders? (Y/N)"
if ($OpenFolders -eq 'Y' -or $OpenFolders -eq 'y') {
    if (Test-Path $ServerPath) {
        Start-Process explorer.exe $ServerPath
    }
    if (Test-Path $ClientPath) {
        Start-Process explorer.exe $ClientPath
    }
}

Write-Host ""
Read-Host "Press Enter to exit"

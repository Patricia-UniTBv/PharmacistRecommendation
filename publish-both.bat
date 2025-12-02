@echo off
REM =======================================================================
REM PharmacistRecommendation - Publish Both Versions
REM This script publishes both Server and Client versions
REM =======================================================================

echo.
echo ========================================================================
echo    PharmacistRecommendation - Publishing Server and Client Versions
echo ========================================================================
echo.

REM Save current directory
set "SCRIPT_DIR=%~dp0"

REM Check if we're in the right directory
if not exist "PharmacistRecommendation\PharmacistRecommendation.csproj" (
    echo ERROR: PharmacistRecommendation.csproj not found!
    echo Please run this script from the solution root directory.
    echo.
    pause
    exit /b 1
)

REM Clean previous builds (optional - uncomment if needed)
REM echo Cleaning previous builds...
REM dotnet clean PharmacistRecommendation\PharmacistRecommendation.csproj
REM if %errorlevel% neq 0 goto :error
REM echo.

REM Publish Server version
echo ========================================================================
echo [1/2] Publishing SERVER version...
echo ========================================================================
echo.
cd PharmacistRecommendation
dotnet publish -c Release -p:PublishProfile=Properties\PublishProfiles\Server.pubxml
if %errorlevel% neq 0 (
    cd "%SCRIPT_DIR%"
    goto :error
)
echo.
echo Server version published successfully!
echo Output: bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\
echo.

REM Publish Client version
echo ========================================================================
echo [2/2] Publishing CLIENT version...
echo ========================================================================
echo.
dotnet publish -c Release -p:PublishProfile=Properties\PublishProfiles\Client.pubxml
if %errorlevel% neq 0 (
    cd "%SCRIPT_DIR%"
    goto :error
)
echo.
echo Client version published successfully!
echo Output: bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\
echo.

REM Success
cd "%SCRIPT_DIR%"
echo ========================================================================
echo SUCCESS! Both versions published successfully
echo ========================================================================
echo.
echo Server output folder:
echo   %~dp0PharmacistRecommendation\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\
echo.
echo Client output folder:
echo   %~dp0PharmacistRecommendation\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\
echo.
echo Next steps:
echo   1. For SERVER: Copy Server folder to the server computer
echo   2. For CLIENTS: Copy Client folder to each client computer
echo.
echo See COMPLETE_DEPLOYMENT_GUIDE.md for detailed installation instructions.
echo.
pause
exit /b 0

:error
echo.
echo ========================================================================
echo ERROR: Publishing failed!
echo ========================================================================
echo.
echo Please check the error messages above and try again.
echo.
echo Common issues:
echo   - .NET 8 SDK not installed
echo   - Project files corrupted
echo   - Insufficient disk space
echo.
pause
exit /b 1

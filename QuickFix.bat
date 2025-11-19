@echo off
echo ========================================
echo Quick Fix for CidGen DLL Crash
echo ========================================
echo.

echo Step 1: Killing any running PharmacistRecommendation processes...
taskkill /F /IM PharmacistRecommendation.exe 2>nul
timeout /t 2 /nobreak >nul

echo Step 2: Clearing output directories...
rd /s /q "PharmacistRecommendation\bin\Debug" 2>nul
timeout /t 1 /nobreak >nul

echo Step 3: Clearing NuGet package cache...
dotnet nuget locals all --clear

echo Step 4: Rebuilding solution...
dotnet clean
dotnet build PharmacistRecommendation\PharmacistRecommendation.csproj --no-incremental

echo.
echo ========================================
echo Done! Try running the app again.
echo ========================================
pause

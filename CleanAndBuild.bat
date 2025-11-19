@echo off
echo Cleaning up build artifacts and cache...

REM Clean bin and obj folders
rd /s /q "PharmacistRecommendation\bin" 2>nul
rd /s /q "PharmacistRecommendation\obj" 2>nul
rd /s /q "Entities\bin" 2>nul
rd /s /q "Entities\obj" 2>nul
rd /s /q "DTO\bin" 2>nul
rd /s /q "DTO\obj" 2>nul

REM Clear NuGet cache
dotnet nuget locals all --clear

echo.
echo Cleanup complete! Now building...
echo.

REM Restore and build
dotnet restore
dotnet build PharmacistRecommendation\PharmacistRecommendation.csproj

echo.
echo Build complete! You can now run the application.
pause

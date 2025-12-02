# Quick Publishing Guide

This guide shows you how to quickly publish Server and Client versions of the PharmacistRecommendation application.

---

## Publishing Methods

You can publish using either **Visual Studio** or **Command Line**.

---

## Method 1: Using Visual Studio 2022

### Publishing Server Version

1. Open the solution in Visual Studio 2022
2. In **Solution Explorer**, right-click on the **PharmacistRecommendation** project
3. Select **Publish**
4. In the publish dialog, select the **Server** profile from the dropdown
5. Click **Publish** button
6. Wait for the build and publish to complete
7. The output will be in:
   ```
   PharmacistRecommendation\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\
   ```

### Publishing Client Version

1. In the publish dialog (same as above)
2. Select the **Client** profile from the dropdown
3. Click **Publish** button
4. Wait for the build and publish to complete
5. The output will be in:
   ```
   PharmacistRecommendation\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\
   ```

---

## Method 2: Using Command Line

### Publishing Server Version

Open a command prompt or PowerShell in the solution directory and run:

```cmd
cd PharmacistRecommendation
dotnet publish -c Release -p:PublishProfile=Properties\PublishProfiles\Server.pubxml
```

Or more simply (if the profile is configured correctly):

```cmd
cd PharmacistRecommendation
dotnet publish -p:PublishProfile=Server
```

**Output location**: `bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\`

### Publishing Client Version

```cmd
cd PharmacistRecommendation
dotnet publish -c Release -p:PublishProfile=Properties\PublishProfiles\Client.pubxml
```

Or simply:

```cmd
cd PharmacistRecommendation
dotnet publish -p:PublishProfile=Client
```

**Output location**: `bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\`

---

## Publishing Both Versions at Once

### PowerShell Script

Create a file called `publish-all.ps1`:

```powershell
# Publish Server version
Write-Host "Publishing Server version..." -ForegroundColor Green
cd PharmacistRecommendation
dotnet publish -p:PublishProfile=Server
if ($LASTEXITCODE -ne 0) {
    Write-Host "Server publish failed!" -ForegroundColor Red
    exit 1
}

# Publish Client version
Write-Host "Publishing Client version..." -ForegroundColor Green
dotnet publish -p:PublishProfile=Client
if ($LASTEXITCODE -ne 0) {
    Write-Host "Client publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nPublishing complete!" -ForegroundColor Green
Write-Host "Server output: bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\" -ForegroundColor Yellow
Write-Host "Client output: bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\" -ForegroundColor Yellow
```

Run it:
```powershell
.\publish-all.ps1
```

### Batch Script

Create a file called `publish-all.bat`:

```batch
@echo off
echo Publishing Server version...
cd PharmacistRecommendation
dotnet publish -p:PublishProfile=Server
if %errorlevel% neq 0 (
    echo Server publish failed!
    exit /b %errorlevel%
)

echo.
echo Publishing Client version...
dotnet publish -p:PublishProfile=Client
if %errorlevel% neq 0 (
    echo Client publish failed!
    exit /b %errorlevel%
)

echo.
echo Publishing complete!
echo Server output: bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\
echo Client output: bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\
pause
```

Run it:
```cmd
publish-all.bat
```

---

## What Gets Published?

### Server Version
- ? `appsettings.json` (from `appsettings.Server.json`)
- ? All application DLLs and dependencies
- ? Resources (images, fonts, etc.)
- ? `appsettings.Client.json` (excluded)

**Configuration in appsettings.json**:
```json
{
  "DeploymentMode": "Server",
  "DatabaseSettings": {
    "Server": "localhost\\SQLEXPRESS",
    "Database": "PharmacistRecommendationDB",
    "UseWindowsAuthentication": true
  }
}
```

### Client Version
- ? `appsettings.json` (from `appsettings.Client.json`)
- ? All application DLLs and dependencies
- ? Resources (images, fonts, etc.)
- ? `appsettings.Server.json` (excluded)

**Configuration in appsettings.json**:
```json
{
  "DeploymentMode": "Client",
  "DatabaseSettings": {
    "Server": "",
    "Database": "PharmacistRecommendationDB",
    "UseSqlAuthentication": true,
    "Username": "appuser",
    "Password": ""
  }
}
```

---

## After Publishing

### For Server Deployment

1. Go to the output folder:
   ```
   PharmacistRecommendation\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\
   ```

2. Copy the entire `Server` folder to the server computer

3. Recommended installation location:
   ```
   C:\Program Files\PharmacistRecommendation\
   ```

4. Create a desktop shortcut to:
   ```
   C:\Program Files\PharmacistRecommendation\PharmacistRecommendation.exe
   ```

5. Run the application - it will automatically connect to `localhost\SQLEXPRESS`

### For Client Deployment

1. Go to the output folder:
   ```
   PharmacistRecommendation\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\
   ```

2. Copy the entire `Client` folder to each client computer

3. Recommended installation location:
   ```
   C:\Program Files\PharmacistRecommendation\
   ```

4. Create a desktop shortcut to:
   ```
   C:\Program Files\PharmacistRecommendation\PharmacistRecommendation.exe
   ```

5. Run the application - it will prompt for server configuration

6. Enter server details:
   - Server IP: `192.168.1.100` (your server's IP)
   - Database: `PharmacistRecommendationDB`
   - Username: `appuser`
   - Password: (the SQL password you set)

7. Click **Test Connection**, then **Save**

8. Restart the application

---

## Creating Installers (Optional)

To create actual installer packages (.msi or .exe), you can use:

### Option 1: WiX Toolset (Recommended)

1. Install WiX Toolset: https://wixtoolset.org/
2. Create installer projects for Server and Client
3. This creates proper Windows installers with start menu entries

### Option 2: Inno Setup (Easier)

1. Download Inno Setup: https://jrsoftware.org/isinfo.php
2. Create scripts for Server and Client versions
3. Generates .exe installers

### Example Inno Setup Script for Server

Create `ServerInstaller.iss`:

```inno
[Setup]
AppName=PharmacistRecommendation Server
AppVersion=1.0
DefaultDirName={pf}\PharmacistRecommendation
DefaultGroupName=PharmacistRecommendation
OutputBaseFilename=PharmacistRecommendation_Server_Setup
Compression=lzma2
SolidCompression=yes

[Files]
Source: "bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\PharmacistRecommendation Server"; Filename: "{app}\PharmacistRecommendation.exe"
Name: "{commondesktop}\PharmacistRecommendation Server"; Filename: "{app}\PharmacistRecommendation.exe"

[Run]
Filename: "{app}\PharmacistRecommendation.exe"; Description: "Launch PharmacistRecommendation"; Flags: postinstall nowait skipifsilent
```

### Example Inno Setup Script for Client

Create `ClientInstaller.iss`:

```inno
[Setup]
AppName=PharmacistRecommendation Client
AppVersion=1.0
DefaultDirName={pf}\PharmacistRecommendation
DefaultGroupName=PharmacistRecommendation
OutputBaseFilename=PharmacistRecommendation_Client_Setup
Compression=lzma2
SolidCompression=yes

[Files]
Source: "bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\PharmacistRecommendation Client"; Filename: "{app}\PharmacistRecommendation.exe"
Name: "{commondesktop}\PharmacistRecommendation Client"; Filename: "{app}\PharmacistRecommendation.exe"

[Run]
Filename: "{app}\PharmacistRecommendation.exe"; Description: "Launch PharmacistRecommendation"; Flags: postinstall nowait skipifsilent
```

Compile with Inno Setup to create installers.

---

## Verifying the Build

### Check Configuration Files

**Server version** - check that `appsettings.json` contains:
```json
"DeploymentMode": "Server"
```

**Client version** - check that `appsettings.json` contains:
```json
"DeploymentMode": "Client"
```

### Test Locally

Before deploying:

1. **Test Server version**:
   - Copy to a test folder
   - Run `PharmacistRecommendation.exe`
   - Should connect to localhost automatically

2. **Test Client version**:
   - Copy to a test folder
   - Run `PharmacistRecommendation.exe`
   - Should prompt for server configuration

---

## Troubleshooting Publishing Issues

### Issue: "Project targets framework that is not installed"

**Solution**: Install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0

### Issue: "Unable to find publish profile"

**Solution**: Verify the profile files exist:
- `PharmacistRecommendation\Properties\PublishProfiles\Server.pubxml`
- `PharmacistRecommendation\Properties\PublishProfiles\Client.pubxml`

### Issue: Published app won't run

**Solution**: 
1. Check if .NET 8.0 Desktop Runtime is installed on target machine
2. Run in command prompt to see error messages:
   ```cmd
   PharmacistRecommendation.exe
   ```

### Issue: Wrong appsettings.json in output

**Solution**: 
1. Clean the solution: `dotnet clean`
2. Delete bin and obj folders
3. Publish again

---

## Quick Reference

### File Structure After Publishing

```
Server\
??? PharmacistRecommendation.exe
??? PharmacistRecommendation.dll
??? appsettings.json (from appsettings.Server.json)
??? Entities.dll
??? DTO.dll
??? [other dependencies...]

Client\
??? PharmacistRecommendation.exe
??? PharmacistRecommendation.dll
??? appsettings.json (from appsettings.Client.json)
??? Entities.dll
??? DTO.dll
??? [other dependencies...]
```

### Deployment Checklist

**Server**:
- [ ] SQL Server Express installed
- [ ] Database created
- [ ] SQL user created (appuser)
- [ ] Firewall configured
- [ ] Server app published
- [ ] Server app installed
- [ ] Server app tested

**Each Client**:
- [ ] .NET Runtime installed
- [ ] Network connectivity verified
- [ ] Client app published
- [ ] Client app installed
- [ ] Server IP configured
- [ ] Connection tested
- [ ] Client app works

---

**For detailed setup and configuration instructions, see `COMPLETE_DEPLOYMENT_GUIDE.md`**

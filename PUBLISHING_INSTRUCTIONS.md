# PharmacistRecommendation - Server/Client Deployment Guide

## Overview

Your application now supports two deployment modes:
- **Server Mode**: For the main server computer with SQL Server Express installed locally
- **Client Mode**: For laptops/PCs on the network that connect to the server

## Quick Start

### Prerequisites

**On Server Computer:**
- Windows 10/11 with .NET 8 Runtime installed
- SQL Server 2022 Express with instance name `SQLEXPRESS`
- Database: `PharmacistRecommendationDB` must exist

**On Client Computers:**
- Windows 10/11 with .NET 8 Runtime installed  
- Network access to the server computer
- SQL Authentication user: `appuser` (created on server)

## Publishing the Application

### Using Visual Studio

#### For Server Deployment:
1. Right-click **PharmacistRecommendation** project ? **Publish...**
2. Select **Server** publish profile
3. Click **Publish**
4. Output: `bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\`

#### For Client Deployment:
1. Right-click **PharmacistRecommendation** project ? **Publish...**
2. Select **Client** publish profile
3. Click **Publish**
4. Output: `bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\`

### Using Command Line

From the solution directory:

```powershell
# Server
dotnet publish PharmacistRecommendation\PharmacistRecommendation.csproj /p:PublishProfile=Server

# Client
dotnet publish PharmacistRecommendation\PharmacistRecommendation.csproj /p:PublishProfile=Client
```

## Installation

### Server Installation

1. Copy the entire `Server` folder to: `C:\Program Files\PharmacistRecommendation\`
2. Run `PharmacistRecommendation.exe`
3. The application automatically connects to `localhost\SQLEXPRESS` using Windows Authentication

**Configuration File Location:** `C:\ProgramData\PharmacistRecommendation\config.json` (auto-created)

### Client Installation

1. Copy the entire `Client` folder to: `C:\Program Files\PharmacistRecommendation\`
2. Run `PharmacistRecommendation.exe`
3. Navigate to **Configur?ri** ? **Configurare conexiune server**
4. Enter:
   - **Server IP Address**: e.g., `192.168.1.100`
   - **SQL Server Instance**: `SQLEXPRESS` (default)
   - **Database Name**: `PharmacistRecommendationDB` (default)
   - **Username**: `appuser`
   - **Password**: (password for appuser)
5. Click **Test Connection**
6. Click **Save Configuration**
7. Restart the application

## SQL Server Setup (One-Time)

### 1. Create SQL User for Clients

On the server, run in SQL Server Management Studio (SSMS):

```sql
USE master;
GO

CREATE LOGIN appuser WITH PASSWORD = 'YourSecurePassword123!';
GO

USE PharmacistRecommendationDB;
GO

CREATE USER appuser FOR LOGIN appuser;
GO

ALTER ROLE db_datareader ADD MEMBER appuser;
ALTER ROLE db_datawriter ADD MEMBER appuser;
ALTER ROLE db_ddladmin ADD MEMBER appuser;
GO
```

### 2. Enable Remote Connections

1. Open **SQL Server Configuration Manager**
2. Expand **SQL Server Network Configuration**
3. Click **Protocols for SQLEXPRESS**
4. Enable **TCP/IP** protocol
5. Right-click **TCP/IP** ? **Properties**
6. Go to **IP Addresses** tab ? **IPALL** section
7. Set **TCP Port** to `1433`
8. Restart **SQL Server (SQLEXPRESS)** service

### 3. Configure Firewall

Open PowerShell as Administrator on the server:

```powershell
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

### 4. Find Server IP Address

On the server:

```cmd
ipconfig
```

Look for **IPv4 Address** (e.g., `192.168.1.100`) - clients will need this.

## Configuration Files

### Server Mode (appsettings.json in Server deployment)

```json
{
  "DeploymentMode": "Server",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PharmacistRecommendationDB;Integrated Security=true;TrustServerCertificate=true;"
  },
  "DatabaseSettings": {
    "Server": "localhost\\SQLEXPRESS",
    "Database": "PharmacistRecommendationDB",
    "UseWindowsAuthentication": true,
    "TrustServerCertificate": true
  }
}
```

### Client Mode (appsettings.json in Client deployment)

```json
{
  "DeploymentMode": "Client",
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "DatabaseSettings": {
    "Server": "",
    "Database": "PharmacistRecommendationDB",
    "UseWindowsAuthentication": false,
    "UseSqlAuthentication": true,
    "Username": "appuser",
    "Password": "",
    "TrustServerCertificate": true
  }
}
```

### User Configuration (Runtime)

Created at: `C:\ProgramData\PharmacistRecommendation\config.json`

Example for client mode:
```json
{
  "SqlServer": "192.168.1.100\\SQLEXPRESS",
  "Database": "PharmacistRecommendationDB",
  "ServerIP": "192.168.1.100",
  "Username": "appuser",
  "Password": "YourPassword",
  "TrustServerCertificate": true
}
```

## Troubleshooting

### Client Cannot Connect

**Check:**
1. Server IP is correct
2. SQL Server is running on server
3. TCP/IP is enabled in SQL Server Configuration Manager
4. Firewall allows port 1433
5. `appuser` exists with correct password

**Test port from client:**
```powershell
Test-NetConnection -ComputerName 192.168.1.100 -Port 1433
```

### Authentication Failed

**Solution:**
1. Verify password is correct
2. In SSMS: Right-click server ? Properties ? Security
3. Select "SQL Server and Windows Authentication mode"
4. Restart SQL Server service

### Configuration Not Saved

**Solution:**
1. Ensure application has write permissions to `C:\ProgramData\PharmacistRecommendation\`
2. Try running as administrator once to create the folder
3. Delete `config.json` and reconfigure

### Change Configuration After Setup

**Option 1 - From Application:**
1. **Configur?ri** ? **Configurare conexiune server**
2. Update settings
3. Test connection
4. Save and restart

**Option 2 - Manual:**
Edit: `C:\ProgramData\PharmacistRecommendation\config.json`

## Technical Details

### How It Works

1. **Build Time**: Two publish profiles (`Server.pubxml` and `Client.pubxml`) copy the appropriate `appsettings.json` file
2. **Runtime**: 
   - `ConfigurationManager` loads `appsettings.json` from application directory
   - Detects deployment mode (`Server` or `Client`)
   - For Client mode, loads/creates user configuration from `C:\ProgramData\PharmacistRecommendation\config.json`
   - `MauiProgram.cs` builds connection string and registers DbContext

### Files Structure After Publishing

**Server:**
```
PharmacistRecommendation.exe
appsettings.json (Server mode)
[other DLLs and dependencies]
```

**Client:**
```
PharmacistRecommendation.exe
appsettings.json (Client mode)
[other DLLs and dependencies]
```

### Key Classes

- `ConfigurationManager` (`Helpers/ConfigurationManager.cs`): Loads configuration
- `ServerConfigurationViewModel`: UI for configuration
- `ServerConfigurationView`: Configuration screen
- `MauiProgram.cs`: Application startup and dependency injection

## Security Recommendations

1. Use strong passwords for `appuser`
2. Restrict network access with firewall rules
3. Consider VPN for internet connections
4. Regular SQL Server security updates
5. Regular database backups

## Creating MSI Installers (Optional)

You can use tools like:
- **WiX Toolset**: https://wixtoolset.org/
- **Inno Setup**: https://jrsoftware.org/isinfo.php

Example Inno Setup script:

```iss
[Setup]
AppName=PharmacistRecommendation Server
AppVersion=1.0
DefaultDirName={pf}\PharmacistRecommendation
DefaultGroupName=PharmacistRecommendation

[Files]
Source: "publish\Server\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\PharmacistRecommendation"; Filename: "{app}\PharmacistRecommendation.exe"
```

---

**Version**: 2.0  
**Last Updated**: January 2025
**Framework**: .NET 8 / .NET MAUI

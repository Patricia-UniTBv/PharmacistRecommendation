# PharmacistRecommendation - Deployment Guide

## Overview

This application supports two deployment modes:
- **Server Mode**: For the main server computer with SQL Server Express installed locally
- **Client Mode**: For laptops/PCs on the network that connect to the server

## Prerequisites

### Server Computer
1. Windows 10/11 with .NET 8 Runtime installed
2. SQL Server 2022 Express installed with instance name `SQLEXPRESS`
3. Database: `PharmacistRecommendationDB` must be created and configured
4. SQL Server configured to accept remote connections (for client computers to connect)
5. Windows Firewall configured to allow SQL Server connections on port 1433

### Client Computers
1. Windows 10/11 with .NET 8 Runtime installed
2. Network access to the server computer
3. SQL Authentication user created: `appuser` with appropriate password

## SQL Server Setup (One-Time Server Configuration)

### 1. Enable SQL Server Authentication and Remote Connections

On the server computer, open SQL Server Management Studio (SSMS) and run:

```sql
-- Enable SQL Server and Windows Authentication Mode
USE master;
GO

-- Create the application user for client connections
CREATE LOGIN appuser WITH PASSWORD = 'YourSecurePassword123!';
GO

USE PharmacistRecommendationDB;
GO

-- Grant permissions to the application user
CREATE USER appuser FOR LOGIN appuser;
GO

ALTER ROLE db_datareader ADD MEMBER appuser;
ALTER ROLE db_datawriter ADD MEMBER appuser;
ALTER ROLE db_ddladmin ADD MEMBER appuser;
GO

-- Verify the user was created
SELECT name FROM sys.database_principals WHERE name = 'appuser';
GO
```

### 2. Configure SQL Server for Remote Connections

1. Open **SQL Server Configuration Manager**
2. Expand **SQL Server Network Configuration**
3. Click **Protocols for SQLEXPRESS**
4. Enable **TCP/IP** protocol
5. Right-click **TCP/IP** ? **Properties**
6. Go to **IP Addresses** tab
7. Scroll to **IPALL** section
8. Set **TCP Port** to `1433`
9. Click **OK**
10. Restart **SQL Server (SQLEXPRESS)** service

### 3. Configure Windows Firewall

On the server computer, open PowerShell as Administrator and run:

```powershell
# Allow SQL Server through Windows Firewall
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow

# Verify the rule was created
Get-NetFirewallRule -DisplayName "SQL Server"
```

### 4. Find Server IP Address

On the server computer, open Command Prompt and run:

```cmd
ipconfig
```

Look for the **IPv4 Address** under your active network adapter (usually starts with 192.168.x.x or 10.x.x.x).
**Write down this IP address** - clients will need it.

## Publishing the Application

### Option A: Using Visual Studio

#### For Server Deployment:

1. Right-click the **PharmacistRecommendation** project in Solution Explorer
2. Select **Publish...**
3. Choose the **Server** publish profile
4. Click **Publish**
5. The output will be in: `bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\`

#### For Client Deployment:

1. Right-click the **PharmacistRecommendation** project in Solution Explorer
2. Select **Publish...**
3. Choose the **Client** publish profile
4. Click **Publish**
5. The output will be in: `bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\`

### Option B: Using Command Line

Open a terminal in the project root directory and run:

#### For Server:
```powershell
dotnet publish PharmacistRecommendation\PharmacistRecommendation.csproj /p:PublishProfile=Server
```

#### For Client:
```powershell
dotnet publish PharmacistRecommendation\PharmacistRecommendation.csproj /p:PublishProfile=Client
```

## Installation Instructions

### Server Installation

1. Copy the entire contents of the Server publish folder to the server computer:
   - Recommended location: `C:\Program Files\PharmacistRecommendation\`

2. Run `PharmacistRecommendation.exe`

3. The application will automatically use Windows Authentication to connect to:
   - Server: `localhost\SQLEXPRESS`
   - Database: `PharmacistRecommendationDB`

4. If needed, you can manually configure the connection by:
   - Navigate to **Settings** ? **Server Configuration**
   - The server mode settings will be displayed
   - Verify the connection with **Test Connection** button

### Client Installation

1. Copy the entire contents of the Client publish folder to each client computer:
   - Recommended location: `C:\Program Files\PharmacistRecommendation\`

2. Run `PharmacistRecommendation.exe` for the first time

3. You will be prompted to configure the server connection

4. Enter the following information:
   - **Server IP Address**: The IP address of the server computer (e.g., `192.168.1.100`)
   - **SQL Server Instance**: `SQLEXPRESS` (default)
   - **Database Name**: `PharmacistRecommendationDB` (default)
   - **Username**: `appuser`
   - **Password**: The password you set for the `appuser` SQL login

5. Click **Test Connection** to verify the connection works

6. Click **Save Configuration**

7. Restart the application

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

### User Configuration File (Created at Runtime)

For both modes, a user-specific configuration file is created at:
- **Location**: `C:\ProgramData\PharmacistRecommendation\config.json`

This file stores the actual connection settings and can be edited if needed.

## Troubleshooting

### Issue: Client cannot connect to server

**Check:**
1. Server IP address is correct
2. SQL Server service is running on server
3. TCP/IP protocol is enabled in SQL Server Configuration Manager
4. Firewall allows connections on port 1433
5. `appuser` login exists and has proper permissions

**Test Connection from Client:**
```powershell
# Test if port 1433 is open
Test-NetConnection -ComputerName 192.168.1.100 -Port 1433
```

### Issue: Authentication failed for user 'appuser'

**Solution:**
1. Verify the password is correct
2. Check that SQL Server is set to **SQL Server and Windows Authentication mode**:
   - In SSMS, right-click server ? Properties ? Security
   - Select "SQL Server and Windows Authentication mode"
   - Restart SQL Server service

### Issue: Database not found

**Solution:**
1. Verify the database exists: `PharmacistRecommendationDB`
2. Ensure the user has access to the database
3. Check the database name in the configuration

### Issue: Configuration window doesn't appear

**Solution:**
1. Delete the config file: `C:\ProgramData\PharmacistRecommendation\config.json`
2. Restart the application
3. The configuration screen will appear automatically

## Updating Configuration

To reconfigure the application after initial setup:

### From Application Menu:
1. Navigate to **Settings** ? **Server Configuration**
2. Modify the connection settings
3. Test the connection
4. Save the changes
5. Restart the application

### Manual Configuration:
Edit the file: `C:\ProgramData\PharmacistRecommendation\config.json`

Example for Client mode:
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

## Creating Installers (Optional)

To create proper installers, you can use tools like:

### For MSI Installers:
- **WiX Toolset**: https://wixtoolset.org/
- **Advanced Installer**: https://www.advancedinstaller.com/

### For Single-File Installers:
- **Inno Setup**: https://jrsoftware.org/isinfo.php

Example Inno Setup script structure:
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

## Security Recommendations

1. **Use strong passwords** for the `appuser` SQL login
2. **Restrict network access** to SQL Server using firewall rules (only allow specific client IPs)
3. **Regularly update** the application and SQL Server with security patches
4. **Backup the database** regularly
5. **Use VPN** if clients need to connect over the internet

## Support

For issues or questions, contact your system administrator or refer to the application documentation.

---

**Version**: 1.0  
**Last Updated**: 2025

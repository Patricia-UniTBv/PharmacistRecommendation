# Complete Deployment Guide - PharmacistRecommendation App

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Prerequisites](#prerequisites)
4. [Server Deployment](#server-deployment)
5. [Client Deployment](#client-deployment)
6. [SQL Server Configuration](#sql-server-configuration)
7. [Troubleshooting](#troubleshooting)

---

## Overview

This application supports two deployment modes:
- **Server Mode**: Runs on the main server with SQL Server Express installed locally
- **Client Mode**: Runs on multiple workstations connecting to the central server

---

## Architecture

### Server Mode
```
???????????????????????????????????
?     Server Computer             ?
?                                 ?
?  ????????????????????????????  ?
?  ?  PharmacistRecommendation?  ?
?  ?  (Server Mode)           ?  ?
?  ????????????????????????????  ?
?             ?                   ?
?  ????????????????????????????  ?
?  ?  SQL Server Express      ?  ?
?  ?  localhost\SQLEXPRESS    ?  ?
?  ?  Windows Authentication  ?  ?
?  ????????????????????????????  ?
???????????????????????????????????
```

### Client Mode
```
????????????????????         ????????????????????
?  Client PC #1    ?         ?  Client PC #2    ?
?                  ?         ?                  ?
?  ??????????????  ?         ?  ??????????????  ?
?  ?    App     ?  ?         ?  ?    App     ?  ?
?  ?(Client Mode)  ?         ?  ?(Client Mode) ?
?  ??????????????  ?         ?  ??????????????  ?
????????????????????         ????????????????????
         ?                            ?
         ?     Network Connection     ?
         ?                            ?
         ??????????????????????????????
                      ?
         ???????????????????????????????
         ?    Server Computer          ?
         ?                             ?
         ?  ????????????????????????  ?
         ?  ?  SQL Server Express  ?  ?
         ?  ?  IP: 192.168.1.100   ?  ?
         ?  ?  SQL Authentication  ?  ?
         ?  ????????????????????????  ?
         ???????????????????????????????
```

---

## Prerequisites

### For Server Computer
1. **Windows 10/11 Professional or Enterprise** (required for SQL Server)
2. **SQL Server 2022 Express** or higher
   - Instance name: `SQLEXPRESS`
   - Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
3. **.NET 8.0 Runtime** (Desktop Runtime)
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
4. **Minimum 4 GB RAM**, 8 GB recommended
5. **Static IP address** on the local network (for client connections)

### For Client Computers
1. **Windows 10/11** (any edition)
2. **.NET 8.0 Runtime** (Desktop Runtime)
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
3. **Minimum 2 GB RAM**
4. **Network connectivity** to the server computer

---

## Server Deployment

### Step 1: Install SQL Server Express

1. Download SQL Server 2022 Express from Microsoft
2. During installation:
   - Choose "Basic" or "Custom" installation
   - Use instance name: `SQLEXPRESS`
   - Enable "Mixed Mode" authentication
   - Set a strong SA password (write it down!)
   - Add your Windows user as SQL Server administrator

### Step 2: Configure SQL Server for Network Access

#### 2.1 Enable TCP/IP Protocol

1. Open **SQL Server Configuration Manager**
   - Press Windows Key + R
   - Type: `SQLServerManager16.msc` (for SQL Server 2022)
   - Press Enter

2. Navigate to: **SQL Server Network Configuration** ? **Protocols for SQLEXPRESS**

3. Enable the following protocols:
   - **TCP/IP**: Right-click ? Enable
   - **Named Pipes**: Right-click ? Enable

4. Double-click **TCP/IP** ? Go to **IP Addresses** tab
   - Scroll to **IPAII**
   - Set **TCP Port** to: `1433`
   - Click OK

5. Restart SQL Server:
   - Go to **SQL Server Services**
   - Right-click **SQL Server (SQLEXPRESS)**
   - Select **Restart**

#### 2.2 Configure Windows Firewall

Run these commands in **Command Prompt (Administrator)**:

```cmd
netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433

netsh advfirewall firewall add rule name="SQL Browser" dir=in action=allow protocol=UDP localport=1434
```

### Step 3: Create Database and User Account

1. Open **SQL Server Management Studio (SSMS)** or **Azure Data Studio**
   - Connect to: `localhost\SQLEXPRESS`
   - Use Windows Authentication

2. Create the database:
```sql
CREATE DATABASE PharmacistRecommendationDB;
GO
```

3. Create SQL login for client computers:
```sql
USE [master];
GO

-- Create login
CREATE LOGIN [appuser] WITH PASSWORD = 'YourStrongPassword123!';
GO

-- Switch to your database
USE [PharmacistRecommendationDB];
GO

-- Create user and grant permissions
CREATE USER [appuser] FOR LOGIN [appuser];
GO

-- Grant necessary permissions
ALTER ROLE db_datareader ADD MEMBER [appuser];
ALTER ROLE db_datawriter ADD MEMBER [appuser];
ALTER ROLE db_ddladmin ADD MEMBER [appuser];
GO
```

**IMPORTANT**: Replace `'YourStrongPassword123!'` with a strong password and save it securely!

### Step 4: Find Server IP Address

1. Open **Command Prompt**
2. Run: `ipconfig`
3. Look for **IPv4 Address** under your network adapter (e.g., `192.168.1.100`)
4. Write this down - you'll need it for client configuration

### Step 5: Publish Server Application

#### Using Visual Studio:

1. Open the solution in Visual Studio 2022
2. Right-click on **PharmacistRecommendation** project ? **Publish**
3. Select the **Server** publish profile
4. Click **Publish**
5. The application will be published to:
   ```
   bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\
   ```

#### Using Command Line:

```cmd
cd PharmacistRecommendation
dotnet publish -p:PublishProfile=Server
```

### Step 6: Install Server Application

1. Copy the entire `Server` folder to the server computer
2. Recommended location: `C:\Program Files\PharmacistRecommendation\`
3. Create a desktop shortcut to `PharmacistRecommendation.exe`
4. Run the application
5. The application will connect to `localhost\SQLEXPRESS` automatically

### Step 7: Run Database Migrations

1. Launch the application on the server
2. If this is the first run, the application will create database tables automatically
3. Alternatively, run migrations manually:

```cmd
cd PharmacistRecommendation
dotnet ef database update --project ..\Entities\Entities.csproj --startup-project .
```

---

## Client Deployment

### Step 1: Verify Server Connectivity

Before deploying clients, verify network connectivity:

1. On a client computer, open **Command Prompt**
2. Ping the server:
   ```cmd
   ping 192.168.1.100
   ```
   (Replace with your server's IP address)

3. Test SQL Server connection:
   ```cmd
   telnet 192.168.1.100 1433
   ```
   - If telnet is not enabled, enable it in Windows Features
   - A blank screen means connection successful
   - Error means port is blocked

### Step 2: Publish Client Application

#### Using Visual Studio:

1. Open the solution in Visual Studio 2022
2. Right-click on **PharmacistRecommendation** project ? **Publish**
3. Select the **Client** publish profile
4. Click **Publish**
5. The application will be published to:
   ```
   bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\
   ```

#### Using Command Line:

```cmd
cd PharmacistRecommendation
dotnet publish -p:PublishProfile=Client
```

### Step 3: Install Client Application

1. Copy the entire `Client` folder to each client computer
2. Recommended location: `C:\Program Files\PharmacistRecommendation\`
3. Create a desktop shortcut to `PharmacistRecommendation.exe`

### Step 4: Configure Client Connection

When you first run the client application:

1. The app will detect it's in Client mode and prompt for server configuration
2. Click **OK** to open the configuration screen
3. Enter the following information:

   - **Server IP Address**: `192.168.1.100` (your server's IP)
   - **Instance Name**: `SQLEXPRESS` (leave as default)
   - **Database Name**: `PharmacistRecommendationDB` (leave as default)
   - **SQL Username**: `appuser`
   - **SQL Password**: The password you set in Step 3 of Server Deployment

4. Click **Test Connection** to verify
5. If successful, click **Save**
6. **Restart the application** to apply changes

### Alternative: Manual Configuration

If you prefer to configure manually, create this file on each client:

**Location**: `C:\ProgramData\PharmacistRecommendation\config.json`

```json
{
  "SqlServer": "192.168.1.100\\SQLEXPRESS",
  "Database": "PharmacistRecommendationDB",
  "ServerIP": "192.168.1.100",
  "Username": "appuser",
  "Password": "YourStrongPassword123!",
  "TrustServerCertificate": true
}
```

---

## SQL Server Configuration

### Database Name Verification

To verify your database name:

1. Open SSMS and connect to your server
2. Expand **Databases** in Object Explorer
3. Look for the database name (default: `PharmacistRecommendationDB`)

If the database has a different name, update the configuration accordingly.

### Checking SQL Server Instance Name

```sql
SELECT @@SERVERNAME AS ServerName, 
       SERVERPROPERTY('InstanceName') AS InstanceName
```

### Verify User Permissions

```sql
USE PharmacistRecommendationDB;
GO

-- Check user permissions
EXEC sp_helpuser 'appuser';
GO

-- Check role memberships
SELECT 
    USER_NAME(member_principal_id) AS UserName,
    USER_NAME(role_principal_id) AS RoleName
FROM sys.database_role_members
WHERE USER_NAME(member_principal_id) = 'appuser';
GO
```

---

## Troubleshooting

### Issue: "Server not found or not accessible"

**Solution:**
1. Verify SQL Server is running:
   - Open **Services** (services.msc)
   - Look for **SQL Server (SQLEXPRESS)**
   - Ensure it's Running
   
2. Check SQL Server Browser service:
   - In Services, find **SQL Server Browser**
   - Set Startup Type to **Automatic**
   - Start the service

3. Verify TCP/IP is enabled (see Step 2.1)

4. Check firewall rules (see Step 2.2)

### Issue: "Login failed for user 'appuser'"

**Solution:**
1. Verify the user exists:
```sql
USE master;
GO
SELECT name FROM sys.sql_logins WHERE name = 'appuser';
GO
```

2. Reset password:
```sql
ALTER LOGIN appuser WITH PASSWORD = 'NewStrongPassword123!';
GO
```

3. Verify user has database access:
```sql
USE PharmacistRecommendationDB;
GO
CREATE USER [appuser] FOR LOGIN [appuser];
GO
```

### Issue: "Cannot open database"

**Solution:**
1. Verify database exists:
```sql
SELECT name FROM sys.databases WHERE name = 'PharmacistRecommendationDB';
GO
```

2. If database doesn't exist, create it:
```sql
CREATE DATABASE PharmacistRecommendationDB;
GO
```

3. Run migrations from the server installation

### Issue: Client can ping server but can't connect to SQL Server

**Solution:**
1. Verify port 1433 is open:
```cmd
netstat -ano | findstr :1433
```

2. Test with SQL Server Management Studio from the client computer:
   - Server name: `192.168.1.100\SQLEXPRESS`
   - Authentication: SQL Server Authentication
   - Login: appuser
   - Password: (your password)

3. If SSMS can't connect, the issue is with SQL Server configuration, not the app

### Issue: "Trust Server Certificate" error

**Solution:**
The connection string includes `TrustServerCertificate=true` which should handle this. If you still see errors:

1. Ensure SQL Server has a valid certificate
2. Or continue using `TrustServerCertificate=true` (suitable for internal networks)

### Issue: Application won't start - missing .NET Runtime

**Solution:**
1. Download and install .NET 8.0 Desktop Runtime
2. Link: https://dotnet.microsoft.com/download/dotnet/8.0
3. Choose: **Windows Desktop Runtime x64**

### Issue: Configuration changes don't apply

**Solution:**
1. Close the application completely
2. Delete config cache:
   ```
   C:\ProgramData\PharmacistRecommendation\config.json
   ```
3. Restart the application
4. Reconfigure

---

## Quick Reference

### Connection Strings

**Server Mode:**
```
Server=localhost\SQLEXPRESS;Database=PharmacistRecommendationDB;Integrated Security=true;TrustServerCertificate=true;
```

**Client Mode:**
```
Server=192.168.1.100\SQLEXPRESS;Database=PharmacistRecommendationDB;User Id=appuser;Password=YourPassword;TrustServerCertificate=true;
```

### Important File Locations

- **Configuration File**: `C:\ProgramData\PharmacistRecommendation\config.json`
- **Application Files**: `C:\Program Files\PharmacistRecommendation\`
- **appsettings.json** (embedded): In application folder

### Useful SQL Commands

```sql
-- Check database size
EXEC sp_spaceused;

-- List all tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Check active connections
SELECT 
    session_id,
    login_name,
    host_name,
    program_name,
    login_time
FROM sys.dm_exec_sessions
WHERE database_id = DB_ID('PharmacistRecommendationDB');

-- Backup database
BACKUP DATABASE PharmacistRecommendationDB 
TO DISK = 'C:\Backup\PharmacistRecommendationDB.bak'
WITH FORMAT;
```

---

## Support and Maintenance

### Regular Maintenance Tasks

1. **Database Backup** (Weekly):
   - Use SQL Server Management Studio
   - Backup to secure location
   - Test restore procedure

2. **Application Updates**:
   - Republish with appropriate profile
   - Deploy to server first, then clients
   - Test on one client before mass deployment

3. **Monitor Logs**:
   - Check Windows Event Viewer for application errors
   - Monitor SQL Server error log

### Performance Optimization

1. **SQL Server**:
   - Ensure sufficient RAM allocated
   - Regular index maintenance
   - Keep statistics updated

2. **Network**:
   - Use wired connections when possible
   - Ensure sufficient bandwidth for client count
   - Monitor network latency

---

## Security Best Practices

1. **Passwords**:
   - Use strong passwords (minimum 12 characters)
   - Mix uppercase, lowercase, numbers, and symbols
   - Change passwords regularly

2. **Network**:
   - Use isolated network segment if possible
   - Keep firewall rules restrictive
   - Disable SQL Server remote access when not needed

3. **Database**:
   - Regular backups
   - Test restore procedures
   - Audit user access

4. **Application**:
   - Keep .NET Runtime updated
   - Monitor for security updates
   - Regular application updates

---

## Additional Resources

- [SQL Server Express Documentation](https://docs.microsoft.com/sql/sql-server/)
- [.NET 8.0 Documentation](https://docs.microsoft.com/dotnet/)
- [SQL Server Configuration Manager](https://docs.microsoft.com/sql/relational-databases/sql-server-configuration-manager)

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**For**: PharmacistRecommendation Application

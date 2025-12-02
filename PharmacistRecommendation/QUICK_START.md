# Quick Start Guide - PharmacistRecommendation Deployment

## Quick Publishing Commands

### Using Visual Studio:
1. Right-click project ? Publish
2. Select **Server** or **Client** profile
3. Click **Publish**

### Using Command Line:

```powershell
# Server deployment
dotnet publish PharmacistRecommendation\PharmacistRecommendation.csproj /p:PublishProfile=Server

# Client deployment
dotnet publish PharmacistRecommendation\PharmacistRecommendation.csproj /p:PublishProfile=Client
```

## Output Locations

- **Server**: `PharmacistRecommendation\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\`
- **Client**: `PharmacistRecommendation\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\`

## Connection Strings

### Server Mode (Windows Authentication):
```
Server=localhost\SQLEXPRESS;Database=PharmacistRecommendationDB;Integrated Security=true;TrustServerCertificate=true;
```

### Client Mode (SQL Authentication):
```
Server=[SERVER_IP]\SQLEXPRESS;Database=PharmacistRecommendationDB;User Id=appuser;Password=[password];TrustServerCertificate=true;
```

## SQL Server Setup (Run on Server)

```sql
-- Create SQL login for client connections
CREATE LOGIN appuser WITH PASSWORD = 'YourSecurePassword123!';
GO

USE PharmacistRecommendationDB;
GO

-- Create user and grant permissions
CREATE USER appuser FOR LOGIN appuser;
GO

ALTER ROLE db_datareader ADD MEMBER appuser;
ALTER ROLE db_datawriter ADD MEMBER appuser;
ALTER ROLE db_ddladmin ADD MEMBER appuser;
GO
```

## Firewall Configuration (Run on Server as Admin)

```powershell
# Allow SQL Server through firewall
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

## Find Server IP (Run on Server)

```cmd
ipconfig
```

Look for IPv4 Address (e.g., 192.168.1.100)

## First-Time Client Setup

1. Install application on client computer
2. Run `PharmacistRecommendation.exe`
3. Configuration screen appears automatically
4. Enter:
   - **Server IP**: (e.g., 192.168.1.100)
   - **Instance**: SQLEXPRESS
   - **Database**: PharmacistRecommendationDB
   - **Username**: appuser
   - **Password**: [your password]
5. Click **Test Connection**
6. Click **Save**
7. Restart application

## Configuration File Location

User settings are stored at:
```
C:\ProgramData\PharmacistRecommendation\config.json
```

## Troubleshooting

### Cannot connect from client?
```powershell
# Test connection from client PC
Test-NetConnection -ComputerName [SERVER_IP] -Port 1433
```

If fails:
1. Check SQL Server service is running
2. Enable TCP/IP in SQL Server Configuration Manager
3. Check firewall rules
4. Restart SQL Server service

### Reset configuration?
Delete: `C:\ProgramData\PharmacistRecommendation\config.json`

## Key Differences Between Deployments

| Feature | Server | Client |
|---------|--------|--------|
| Configuration File | appsettings.Server.json ? appsettings.json | appsettings.Client.json ? appsettings.json |
| Connection | localhost\SQLEXPRESS | [SERVER_IP]\SQLEXPRESS |
| Authentication | Windows (Integrated) | SQL (username/password) |
| First Run | Works immediately | Requires configuration |

## File Structure

### Server Deployment Contains:
- PharmacistRecommendation.exe
- appsettings.json (Server mode)
- All dependencies
- Database: Uses local SQL Server

### Client Deployment Contains:
- PharmacistRecommendation.exe
- appsettings.json (Client mode - unconfigured)
- All dependencies
- Database: Connects to remote server

## What Gets Published?

### Server Profile Publishes:
- All application files
- **appsettings.Server.json** renamed to **appsettings.json**
- Sets DeploymentMode = "Server"

### Client Profile Publishes:
- All application files
- **appsettings.Client.json** renamed to **appsettings.json**
- Sets DeploymentMode = "Client"

---

For detailed instructions, see `DEPLOYMENT_GUIDE.md`

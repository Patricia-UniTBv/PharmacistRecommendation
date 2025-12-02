# Quick Reference: Publishing Server and Client Versions

## Publishing Commands

### Using Visual Studio

**Server Version:**
1. Right-click `PharmacistRecommendation` project
2. Click **Publish...**
3. Select **Server** profile
4. Click **Publish**
5. Find output in: `bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Server\`

**Client Version:**
1. Right-click `PharmacistRecommendation` project
2. Click **Publish...**
3. Select **Client** profile
4. Click **Publish**
5. Find output in: `bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\Client\`

### Using Command Line

Open terminal in solution directory:

```powershell
# Publish Server
dotnet publish PharmacistRecommendation\PharmacistRecommendation.csproj /p:PublishProfile=Server

# Publish Client
dotnet publish PharmacistRecommendation\PharmacistRecommendation.csproj /p:PublishProfile=Client
```

## Quick Installation

### Server Computer

1. Copy `Server` folder ? `C:\Program Files\PharmacistRecommendation\`
2. Run `PharmacistRecommendation.exe`
3. Done! (Auto-connects to localhost\SQLEXPRESS)

### Client Computer

1. Copy `Client` folder ? `C:\Program Files\PharmacistRecommendation\`
2. Run `PharmacistRecommendation.exe`
3. Menu: **Configur?ri** ? **Configurare conexiune server**
4. Enter server IP (e.g., `192.168.1.100`)
5. Enter credentials:
   - Username: `appuser`
   - Password: (ask server admin)
6. Click **Test Connection**
7. Click **Save**
8. Restart app

## SQL Server Setup (Server Computer - One Time Only)

Open SQL Server Management Studio (SSMS) and run:

```sql
-- 1. Create login and user
USE master;
CREATE LOGIN appuser WITH PASSWORD = 'YourSecurePassword123!';

USE PharmacistRecommendationDB;
CREATE USER appuser FOR LOGIN appuser;

-- 2. Grant permissions
ALTER ROLE db_datareader ADD MEMBER appuser;
ALTER ROLE db_datawriter ADD MEMBER appuser;
ALTER ROLE db_ddladmin ADD MEMBER appuser;
```

Then:
1. Open **SQL Server Configuration Manager**
2. Enable **TCP/IP** for SQLEXPRESS
3. Set port to **1433**
4. Restart SQL Server service

Firewall (PowerShell as Admin):
```powershell
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Client can't connect | Check server IP, firewall, SQL Server running |
| Authentication failed | Verify password, check SQL auth enabled |
| Configuration not saved | Run as admin once, check folder permissions |
| Wrong deployment mode | Re-publish with correct profile |

## File Locations

| What | Where |
|------|-------|
| Published files | `bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\[Server or Client]\` |
| User config | `C:\ProgramData\PharmacistRecommendation\config.json` |
| App settings | In application folder: `appsettings.json` |

## Key Differences

| Feature | Server Mode | Client Mode |
|---------|-------------|-------------|
| Connection | localhost\SQLEXPRESS | [Server IP]\SQLEXPRESS |
| Authentication | Windows | SQL (username/password) |
| Configuration | Automatic | User must configure |
| appsettings.json | DeploymentMode: "Server" | DeploymentMode: "Client" |

## Important Notes

1. **Always** publish the correct profile (Server or Client)
2. **Never** mix Server and Client executables
3. **Each computer** needs only ONE installation (Server OR Client)
4. **Server IP** must be reachable from client computers
5. **Password** for appuser must be strong and secure

---

For detailed instructions, see:
- `PUBLISHING_INSTRUCTIONS.md` - Complete publishing and deployment guide
- `DEPLOYMENT_GUIDE.md` - Original deployment documentation
- `IMPLEMENTATION_SUMMARY_DEPLOYMENT.md` - Technical implementation details

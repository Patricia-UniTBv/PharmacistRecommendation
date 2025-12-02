# PharmacistRecommendation - Server/Client Deployment

This repository contains the PharmacistRecommendation application configured for two deployment modes: **Server** and **Client**.

## ?? Quick Start

### For Developers/Publishers

**To publish both Server and Client versions:**

**Option 1: Using the batch script (Windows)**
```cmd
publish-both.bat
```

**Option 2: Using PowerShell**
```powershell
.\publish-both.ps1
```

**Option 3: Manual publishing**
- For Server: `dotnet publish -p:PublishProfile=Server`
- For Client: `dotnet publish -p:PublishProfile=Client`

### For System Administrators

**Server Installation:**
1. Install SQL Server 2022 Express on the server computer
2. Run `SQL_Setup_Script.sql` to create database and user
3. Deploy the Server version of the application
4. See `COMPLETE_DEPLOYMENT_GUIDE.md` for details

**Client Installation:**
1. Deploy the Client version to each workstation
2. Configure server IP address on first run
3. See `COMPLETE_DEPLOYMENT_GUIDE.md` for details

---

## ?? Documentation

| Document | Purpose |
|----------|---------|
| **[COMPLETE_DEPLOYMENT_GUIDE.md](COMPLETE_DEPLOYMENT_GUIDE.md)** | Comprehensive guide covering SQL Server setup, network configuration, and detailed installation steps |
| **[QUICK_PUBLISHING_GUIDE.md](QUICK_PUBLISHING_GUIDE.md)** | Quick reference for publishing Server and Client versions |
| **[SQL_Setup_Script.sql](SQL_Setup_Script.sql)** | SQL script to create database, user account, and permissions |

---

## ??? Architecture

### Server Mode
- Runs on the main server computer
- SQL Server Express installed locally
- Uses Windows Authentication
- Connection: `localhost\SQLEXPRESS`

### Client Mode
- Runs on workstation computers
- Connects to remote SQL Server
- Uses SQL Authentication
- Connection: `[ServerIP]\SQLEXPRESS`

---

## ?? Project Structure

```
PharmacistRecommendation/
??? PharmacistRecommendation/          # Main MAUI application
?   ??? Properties/
?   ?   ??? PublishProfiles/
?   ?       ??? Server.pubxml          # Server publish profile
?   ?       ??? Client.pubxml          # Client publish profile
?   ??? appsettings.Server.json        # Server configuration
?   ??? appsettings.Client.json        # Client configuration
?   ??? Helpers/
?   ?   ??? ConfigurationManager.cs    # Configuration management
?   ??? Views/
?   ?   ??? ServerConfigurationView.xaml  # Server config UI
?   ??? ViewModels/
?       ??? ServerConfigurationViewModel.cs
??? Entities/                          # Entity Framework models
??? DTO/                               # Data transfer objects
??? publish-both.bat                   # Publishing script (Batch)
??? publish-both.ps1                   # Publishing script (PowerShell)
??? SQL_Setup_Script.sql              # Database setup script
??? COMPLETE_DEPLOYMENT_GUIDE.md      # Full deployment guide
??? QUICK_PUBLISHING_GUIDE.md         # Quick publishing reference
??? README_DEPLOYMENT.md              # This file
```

---

## ?? Configuration Files

### Server Configuration (`appsettings.Server.json`)

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

### Client Configuration (`appsettings.Client.json`)

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

The Client configuration is completed by users on first run through the UI.

---

## ?? Publishing

### Publish Profiles

**Server.pubxml**
- Target: Server computer with SQL Server
- Configuration: Windows Authentication
- Output: `bin\Release\...\publish\Server\`

**Client.pubxml**
- Target: Client workstations
- Configuration: SQL Authentication
- Output: `bin\Release\...\publish\Client\`

### Publishing Commands

**Visual Studio:**
1. Right-click project ? Publish
2. Select Server or Client profile
3. Click Publish

**Command Line:**
```cmd
# Publish Server
cd PharmacistRecommendation
dotnet publish -p:PublishProfile=Server

# Publish Client
cd PharmacistRecommendation
dotnet publish -p:PublishProfile=Client
```

---

## ?? Configuration Management

The application uses a smart configuration system:

1. **Embedded Configuration** (`appsettings.json`)
   - Deployed with the application
   - Sets the deployment mode (Server/Client)
   - Contains default settings

2. **User Configuration** (`C:\ProgramData\PharmacistRecommendation\config.json`)
   - Created on first run
   - Stores user-specific settings (server IP, passwords)
   - Can be edited manually or through UI

3. **Configuration Priority**
   - User configuration overrides embedded configuration
   - Allows easy reconfiguration without redeployment

---

## ?? Security

### SQL Server Authentication

**Server Mode:**
- Uses Windows Authentication (Integrated Security)
- No passwords stored
- Secure by default

**Client Mode:**
- Uses SQL Authentication
- Password stored in user config file
- File permissions restrict access to administrators

### SQL User Setup

The `SQL_Setup_Script.sql` creates a SQL user `appuser` with these permissions:
- `db_datareader` - Read data
- `db_datawriter` - Write data
- `db_ddladmin` - Schema changes (for Entity Framework migrations)

**Change the default password immediately after running the script!**

---

## ?? Network Requirements

### Ports
- **SQL Server**: TCP 1433
- **SQL Browser**: UDP 1434 (optional)

### Firewall Rules

**On the server computer**, allow incoming connections:

```cmd
netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433
netsh advfirewall firewall add rule name="SQL Browser" dir=in action=allow protocol=UDP localport=1434
```

### Server IP Address

Find your server's IP address:
```cmd
ipconfig
```
Look for **IPv4 Address** (e.g., `192.168.1.100`)

---

## ?? Testing

### Test Server Installation

1. Run the application
2. Should connect to `localhost\SQLEXPRESS` automatically
3. No configuration prompts

### Test Client Installation

1. Run the application
2. Should prompt for server configuration
3. Enter server details:
   - Server IP: `192.168.1.100`
   - Username: `appuser`
   - Password: (from SQL setup)
4. Click "Test Connection"
5. Should show success message

### Test Network Connectivity

**From a client computer:**

```cmd
# Ping server
ping 192.168.1.100

# Test SQL port
telnet 192.168.1.100 1433
```

---

## ??? Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| "Server not found" | Check SQL Server is running, TCP/IP enabled, firewall configured |
| "Login failed" | Verify SQL user exists, password is correct |
| "Cannot open database" | Run SQL setup script, verify database exists |
| "Missing .NET Runtime" | Install .NET 8.0 Desktop Runtime on target computer |

**For detailed troubleshooting, see `COMPLETE_DEPLOYMENT_GUIDE.md`**

---

## ?? Deployment Checklist

### Server Computer
- [ ] Windows 10/11 Pro or Enterprise
- [ ] SQL Server 2022 Express installed
- [ ] Instance name: SQLEXPRESS
- [ ] TCP/IP protocol enabled
- [ ] Port 1433 configured
- [ ] Firewall rules added
- [ ] Database created (`SQL_Setup_Script.sql` run)
- [ ] SQL user created
- [ ] Server app deployed and tested

### Each Client Computer
- [ ] Windows 10/11 (any edition)
- [ ] .NET 8.0 Desktop Runtime installed
- [ ] Network connectivity to server verified
- [ ] Client app deployed
- [ ] Server configuration completed
- [ ] Connection tested

---

## ?? Updates and Maintenance

### Application Updates

1. Make changes to code
2. Re-publish Server and/or Client versions
3. Deploy Server version first
4. Then deploy to clients
5. Test on one client before mass deployment

### Database Updates

Entity Framework migrations are applied automatically on application startup.

To manually run migrations:
```cmd
cd PharmacistRecommendation
dotnet ef database update --project ..\Entities\Entities.csproj
```

### Configuration Updates

**Server Mode:**
- Configuration is automatic, no changes needed

**Client Mode:**
- Users can update server settings through the UI
- Navigate to: Settings ? Server Configuration
- Or manually edit: `C:\ProgramData\PharmacistRecommendation\config.json`

---

## ?? Tips

1. **Use Static IP for Server**: Set a static IP address on the server to avoid reconfiguring clients

2. **Test Before Mass Deployment**: Always test the client version on one computer before deploying to all

3. **Document Your Setup**: Write down:
   - Server IP address
   - SQL password
   - Network configuration

4. **Regular Backups**: Set up automated SQL Server backups

5. **Monitor Logs**: Check Windows Event Viewer for application errors

---

## ?? Support

For issues or questions:

1. Check `COMPLETE_DEPLOYMENT_GUIDE.md` troubleshooting section
2. Review logs in Windows Event Viewer
3. Check SQL Server error logs
4. Verify network connectivity

---

## ?? License

[Your License Information]

---

## ?? Contributors

[Your Team Information]

---

**Last Updated**: 2024
**Version**: 1.0

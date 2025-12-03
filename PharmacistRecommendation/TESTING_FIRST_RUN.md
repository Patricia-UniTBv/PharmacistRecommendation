# First Run Setup - Testing Guide

## How to Test First Run Detection

### Initial State
When the app starts for the first time:
- **Server Mode**: Database doesn't exist ? Shows `ServerSetupWizard`
- **Client Mode**: Server IP is empty ? Shows `ClientSetupWizard`

### Test Server Setup Wizard

1. **Ensure Server Mode**:
   - Current `appsettings.json` should have `"DeploymentMode": "Server"`
   - Or publish using Server.pubxml profile

2. **Trigger First Run**:
   ```powershell
   # Option A: Delete database
   # In SQL Server Management Studio:
   DROP DATABASE PharmacistRecommendationDB;
   
   # Option B: Reset first run flag programmatically
   # Add this code temporarily in App.xaml.cs constructor:
   FirstRunHelper.ResetFirstRunFlag();
   ```

3. **Expected Behavior**:
   - App starts
   - `ServerSetupWizard` appears
   - User clicks "Creare Baza de Date"
   - Progress shows 10% ? 100%
   - Success message appears
   - Click "Finalizare" ? Navigate to AppShell

4. **Verify Success**:
   - Check Debug Output: "Application already configured. Starting normally..."
   - Next launch should show AppShell directly
   - Database should exist in SQL Server

### Test Client Setup Wizard

1. **Ensure Client Mode**:
   - Current `appsettings.json` should have `"DeploymentMode": "Client"`
   - Or publish using Client.pubxml profile

2. **Trigger First Run**:
   ```powershell
   # Option A: Clear server config
   # Delete: %ProgramData%\PharmacistRecommendation\config.json

   # Option B: Set server to empty in appsettings.Client.json
   {
     "DatabaseSettings": {
     "Server": "",
       "Password": ""
   }
   }
   
   # Option C: Reset first run flag
   FirstRunHelper.ResetFirstRunFlag();
 ```

3. **Expected Behavior**:
   - App starts
   - `ClientSetupWizard` appears
   - User enters:
  - Server Address: (e.g., `192.168.1.100` or `PHARMACY-SERVER`)
     - Password: (SQL password for `appuser`)
   - User clicks "Testeaza Conexiunea"
   - Success message appears (green)
   - "Finalizare" button becomes visible
   - Click "Finalizare" ? Navigate to AppShell

4. **Verify Success**:
   - Check Debug Output: "Application already configured. Starting normally..."
   - Next launch should show AppShell directly
   - Config file should exist: `%ProgramData%\PharmacistRecommendation\config.json`
   - Config file should contain server address and password

### Debug Output Examples

**First Run (Server Mode)**:
```
Starting application initialization...
First run detected. Showing setup wizard...
Deployment mode: Server
Showing Server Setup Wizard
```

**First Run (Client Mode)**:
```
Starting application initialization...
First run detected. Showing setup wizard...
Deployment mode: Client
Showing Client Setup Wizard
```

**Subsequent Runs (Already Configured)**:
```
Starting application initialization...
Application already configured. Starting normally...
```

### Testing the Complete Flow

#### Server Mode - Full Flow Test:
1. Start app ? ServerSetupWizard appears
2. Click "Creare Baza de Date"
3. Wait for database creation (2-3 minutes)
4. See success message
5. Click "Finalizare"
6. AppShell appears
7. Close app
8. Restart app ? AppShell appears directly (no wizard)

#### Client Mode - Full Flow Test:
1. Start app ? ClientSetupWizard appears
2. Enter server address: `192.168.1.100`
3. Enter password for `appuser`
4. Click "Testeaza Conexiunea"
5. See "Conexiune reusita!" message
6. Click "Finalizare"
7. AppShell appears
8. Close app
9. Restart app ? AppShell appears directly (no wizard)

### Reset First Run for Testing

To test the wizards again without reinstalling:

**Method 1: Delete Configuration Files**
```powershell
# Server mode - delete database
DROP DATABASE PharmacistRecommendationDB;

# Client mode - delete config file
Remove-Item "$env:ProgramData\PharmacistRecommendation\config.json" -Force

# Both modes - clear MAUI preferences
# This is stored in: %LOCALAPPDATA%\Packages\<PackageId>\LocalState
# Easiest way: Uninstall and reinstall the app
```

**Method 2: Add Reset Button (for Development)**
Add this to a debug/admin page:
```csharp
[RelayCommand]
private void ResetFirstRun()
{
    try
    {
        FirstRunHelper.ResetFirstRunFlag();
        Application.Current.MainPage.DisplayAlert(
          "Reset Complete", 
   "First run flag has been reset. Please restart the app.", 
            "OK");
    }
    catch (Exception ex)
    {
   Application.Current.MainPage.DisplayAlert(
            "Error", 
 $"Failed to reset: {ex.Message}", 
      "OK");
    }
}
```

### Common Issues and Solutions

#### Issue: "SqlPackage.exe not found" (Server)
**Solution**: Install SQL Server Data Tools (SSDT) or SQL Server Management Studio (SSMS)

#### Issue: "Cannot connect to SQL Server" (Server)
**Solution**: 
- Check SQL Server Express is installed
- Verify SQL Server service is running (Services.msc ? SQL Server (SQLEXPRESS))
- Ensure Windows Authentication is enabled

#### Issue: "Connection failed" (Client)
**Solution**:
- Verify server address is correct (ping the server)
- Check SQL Server allows remote connections
- Verify firewall allows SQL Server port (1433)
- Ensure `appuser` exists on server with correct password

#### Issue: Wizard appears every time
**Solution**:
- Check if `FirstRunHelper.MarkAsConfigured()` is being called
- Verify MAUI Preferences are working (permissions issue?)
- Check Debug Output for errors

### SQL Server Setup for Client Testing

On the **Server Machine**, create the SQL user:
```sql
-- Create login for client connections
CREATE LOGIN appuser WITH PASSWORD = 'YourSecurePassword123!';
GO

-- Switch to your database
USE PharmacistRecommendationDB;
GO

-- Create user and grant permissions
CREATE USER appuser FOR LOGIN appuser;
GO

EXEC sp_addrolemember 'db_datareader', 'appuser';
EXEC sp_addrolemember 'db_datawriter', 'appuser';
GO

-- Enable SQL Server Authentication and remote connections
-- (Use SQL Server Configuration Manager)
```

### Configuration Files Reference

**Server Mode (appsettings.Server.json)**:
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

**Client Mode (appsettings.Client.json)**:
```json
{
  "DeploymentMode": "Client",
  "DatabaseSettings": {
    "Server": "",
    "Database": "PharmacistRecommendationDB",
    "Username": "appuser",
    "Password": "",
    "UseSqlAuthentication": true
  }
}
```

**User Config (saved after Client wizard completes)**:
```json
{
  "SqlServer": "192.168.1.100",
  "Database": "PharmacistRecommendationDB",
  "ServerIP": "192.168.1.100",
  "Username": "appuser",
  "Password": "YourPassword",
  "TrustServerCertificate": true
}
```

### Deployment Testing

#### Server Deployment:
1. Publish using Server.pubxml
2. Copy to target machine
3. Run app ? ServerSetupWizard appears
4. Complete setup
5. Verify database created in SQL Server

#### Client Deployment:
1. Publish using Client.pubxml
2. Copy to target machine
3. Run app ? ClientSetupWizard appears
4. Enter server details
5. Test connection
6. Complete setup
7. Verify can connect to server database

### Integration with Authentication

After first run completes, the app will:
1. Navigate to AppShell
2. AppShell checks authentication (if configured)
3. If not authenticated ? Show LoginView
4. If authenticated ? Show main application

The first-run setup is independent of authentication and happens before login.

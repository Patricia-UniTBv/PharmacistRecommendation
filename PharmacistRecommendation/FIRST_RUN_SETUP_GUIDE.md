# First Run Setup Integration Guide

## Overview
This guide shows how to integrate the Server and Client Setup Wizards into your application startup flow.

## Files Created

### Server Setup Wizard
- `PharmacistRecommendation/ViewModels/ServerSetupWizardViewModel.cs`
- `PharmacistRecommendation/Views/ServerSetupWizard.xaml`
- `PharmacistRecommendation/Views/ServerSetupWizard.xaml.cs`

### Client Setup Wizard
- `PharmacistRecommendation/ViewModels/ClientSetupWizardViewModel.cs`
- `PharmacistRecommendation/Views/ClientSetupWizard.xaml`
- `PharmacistRecommendation/Views/ClientSetupWizard.xaml.cs`

### Helper Classes
- `PharmacistRecommendation/Helpers/FirstRunHelper.cs`
- `PharmacistRecommendation/Helpers/ValueConverters.cs`

## Integration in App.xaml.cs

Add this code to your `App.xaml.cs` file in the constructor or `OnStart` method:

```csharp
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Views;

namespace PharmacistRecommendation
{
 public partial class App : Application
    {
        public App()
        {
    InitializeComponent();
            Current!.UserAppTheme = AppTheme.Light;
      
       // Check first run and show appropriate wizard
  InitializeApplication();
        }

 private async void InitializeApplication()
        {
try
            {
                // Check if this is first run
       bool isFirstRun = await FirstRunHelper.IsFirstRunAsync();
         
         if (isFirstRun)
        {
            // Get deployment mode
                string deploymentMode = FirstRunHelper.GetDeploymentMode();
        
         if (deploymentMode == "Server")
      {
          // Show Server Setup Wizard
       var serverWizard = ServiceHelper.GetService<ServerSetupWizard>();
        MainPage = new NavigationPage(serverWizard);
         }
 else // Client mode
              {
              // Show Client Setup Wizard
       var clientWizard = ServiceHelper.GetService<ClientSetupWizard>();
            MainPage = new NavigationPage(clientWizard);
}
  }
         else
       {
             // Normal startup - configuration is complete
       MainPage = new AppShell();
        }
}
         catch (Exception ex)
            {
   System.Diagnostics.Debug.WriteLine($"Initialization error: {ex.Message}");
      // Fallback to normal shell
              MainPage = new AppShell();
            }
  }

        // ...existing code...
    }
}
```

## Alternative: Check on AppShell Load

If you prefer to check in `AppShell.xaml.cs`:

```csharp
public partial class AppShell : Shell
{
    private readonly IAuthenticationService _authService;

    public AppShell()
    {
        InitializeComponent();
     
     // Get authentication service
        _authService = ServiceHelper.GetService<IAuthenticationService>();
        
  // Check first run before loading
        CheckFirstRunAsync();
        
        // Register routes
        // ...existing code...
}

    private async void CheckFirstRunAsync()
    {
        try
        {
            bool isFirstRun = await FirstRunHelper.IsFirstRunAsync();
     
         if (isFirstRun)
            {
         string deploymentMode = FirstRunHelper.GetDeploymentMode();
                
        if (deploymentMode == "Server")
            {
            var serverWizard = ServiceHelper.GetService<ServerSetupWizard>();
       await Navigation.PushModalAsync(serverWizard);
       }
    else
         {
         var clientWizard = ServiceHelper.GetService<ClientSetupWizard>();
  await Navigation.PushModalAsync(clientWizard);
         }
            }
  }
     catch (Exception ex)
        {
   System.Diagnostics.Debug.WriteLine($"First run check error: {ex.Message}");
        }
    }
}
```

## Server Setup Wizard Features

### What it does:
1. ? Verifies .bacpac file exists
2. ? Checks SQL Server connectivity
3. ? Drops existing database if present
4. ? Restores database from .bacpac using SqlPackage.exe
5. ? Verifies database was created successfully
6. ? Calls `FirstRunHelper.MarkAsConfigured()`

### User Flow:
1. User sees welcome message and instructions
2. User clicks "Creare Baza de Date"
3. Progress indicator shows 6 steps (10% ? 100%)
4. On success: Green message + "Finalizare" button
5. On error: Red message with troubleshooting tips
6. Click "Finalizare" ? Navigate to main app

## Client Setup Wizard Features

### What it does:
1. ? Collects server address (IP or hostname)
2. ? Collects password for SQL user 'appuser'
3. ? Tests database connection
4. ? Saves configuration to user config file
5. ? Calls `FirstRunHelper.MarkAsConfigured()`

### User Flow:
1. User sees welcome message
2. User enters:
   - Server address (e.g., 192.168.1.100 or PHARMACY-SERVER)
   - Password for appuser
3. User clicks "Testeaza Conexiunea"
4. Progress indicator shows while testing
5. On success: Green message + "Finalizare" button enabled
6. On error: Red message with troubleshooting checklist
7. Click "Finalizare" ? Navigate to main app

## Testing

### Test First Run Detection:
```csharp
// In debug or test code
FirstRunHelper.ResetFirstRunFlag();
// Restart app - wizards will appear again
```

### Test Server Mode:
1. Set `appsettings.json` ? `DeploymentMode: "Server"`
2. Delete or rename database
3. Run app ? Server wizard appears

### Test Client Mode:
1. Set `appsettings.json` ? `DeploymentMode: "Client"`
2. Clear server IP in config
3. Run app ? Client wizard appears

## Configuration Files

### Server Mode (appsettings.Server.json):
```json
{
  "DeploymentMode": "Server",
  "DatabaseSettings": {
    "Server": "localhost\\SQLEXPRESS",
    "Database": "PharmacistRecommendationDB",
    "UseWindowsAuthentication": true,
    "TrustServerCertificate": true
  }
}
```

### Client Mode (appsettings.Client.json):
```json
{
  "DeploymentMode": "Client",
  "DatabaseSettings": {
    "Server": "",
    "Database": "PharmacistRecommendationDB",
    "UseSqlAuthentication": true,
  "Username": "appuser",
    "Password": "",
    "TrustServerCertificate": true
  }
}
```

## Troubleshooting

### Server Wizard Issues:

**"SqlPackage.exe not found"**
- Install SQL Server Data Tools (SSDT) or SQL Server Management Studio (SSMS)
- Wizard searches common locations automatically

**"Cannot connect to SQL Server"**
- Verify SQL Server Express is installed
- Check SQL Server service is running
- Ensure Windows Authentication is enabled

**"Database already exists"**
- Wizard automatically drops and recreates
- If manual cleanup needed: DROP DATABASE PharmacistRecommendationDB

### Client Wizard Issues:

**"Connection failed"**
- Verify server address is correct
- Check network connectivity (ping server)
- Ensure SQL Server allows remote connections
- Verify 'appuser' exists on server with correct password
- Check Windows Firewall allows SQL Server port (1433)

**"Database not found"**
- Ensure Server wizard was run first on the server machine
- Verify database name is correct (case-sensitive)

## Security Notes

1. **Password Storage**: Client passwords are stored in user config file
   - Location: `%ProgramData%\PharmacistRecommendation\config.json`
   - Consider encrypting in production

2. **SQL User**: The 'appuser' must be created on server:
   ```sql
   CREATE LOGIN appuser WITH PASSWORD = 'YourSecurePassword';
   CREATE USER appuser FOR LOGIN appuser;
   EXEC sp_addrolemember 'db_datareader', 'appuser';
   EXEC sp_addrolemember 'db_datawriter', 'appuser';
```

3. **Network Security**: Client-server communication uses SQL Server encryption
   - TrustServerCertificate is enabled for simplicity
- For production, use proper SSL certificates

## Next Steps

After successful setup:
1. User is redirected to main application (AppShell)
2. Login screen appears (if authentication is required)
3. Application connects to database using saved configuration
4. FirstRunHelper.IsFirstRunAsync() returns false on subsequent launches

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace PharmacistRecommendation.Helpers
{
    /// <summary>
    /// Helper class to detect if the application is running for the first time
    /// and needs initial configuration based on deployment mode (Server/Client)
  /// </summary>
    public static class FirstRunHelper
    {
        private const string FIRST_RUN_FLAG_KEY = "app_first_run_completed";
        private const string SERVER_CONNECTION_FORMAT = "Server={0};Database={1};Integrated Security=true;TrustServerCertificate=true;Connection Timeout=5";
     private const string CLIENT_CONNECTION_FORMAT = "Server={0};Database={1};User Id={2};Password={3};TrustServerCertificate=true;Connection Timeout=5";

        /// <summary>
        /// Checks if this is the first run of the application based on deployment mode
        /// </summary>
        /// <returns>True if first run, false if already configured</returns>
        public static async Task<bool> IsFirstRunAsync()
        {
 try
            {
              var config = ConfigurationManager.LoadConfiguration();
  
        if (config.DeploymentMode == "Server")
                {
        return await IsFirstRunServerModeAsync(config);
         }
      else // Client mode
       {
        return IsFirstRunClientMode(config);
 }
            }
     catch (Exception ex)
     {
 Debug.WriteLine($"Error checking first run status: {ex.Message}");
                // If we can't determine, assume it's first run to be safe
     return true;
  }
        }

        /// <summary>
   /// Checks first run for Server mode by verifying database existence
        /// </summary>
        private static async Task<bool> IsFirstRunServerModeAsync(AppConfiguration config)
        {
    try
            {
     // Server mode: Check if database exists at localhost\SQLEXPRESS
      string server = config.DatabaseSettings.Server;
          string database = config.DatabaseSettings.Database;

                // Build connection string to master database to check if our DB exists
      string masterConnectionString = $"Server={server};Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=5";

             using var connection = new SqlConnection(masterConnectionString);
    await connection.OpenAsync();

            // Check if the database exists
     string checkDbQuery = $"SELECT database_id FROM sys.databases WHERE name = '{database}'";
      using var command = new SqlCommand(checkDbQuery, connection);
            var result = await command.ExecuteScalarAsync();

             if (result == null)
{
  // Database doesn't exist - first run
     Debug.WriteLine($"Database '{database}' not found on '{server}'. First run detected.");
          return true;
  }

     // Database exists - check if it's properly initialized
                // Try to connect to the actual database
string appConnectionString = string.Format(SERVER_CONNECTION_FORMAT, server, database);
   using var appConnection = new SqlConnection(appConnectionString);
                await appConnection.OpenAsync();

       // Check if essential tables exist (e.g., Pharmacy table)
     string checkTableQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Pharmacy'";
        using var tableCommand = new SqlCommand(checkTableQuery, appConnection);
     var tableCount = (int)await tableCommand.ExecuteScalarAsync();

     if (tableCount == 0)
      {
   // Database exists but tables are missing - first run
         Debug.WriteLine($"Database '{database}' exists but tables are missing. First run detected.");
                return true;
    }

 // Check first run flag
       string checkFlagQuery = "SELECT COUNT(*) FROM Pharmacy";
          using var flagCommand = new SqlCommand(checkFlagQuery, appConnection);
    var pharmacyCount = (int)await flagCommand.ExecuteScalarAsync();

            if (pharmacyCount == 0)
     {
         // No pharmacy configured - first run
  Debug.WriteLine("No pharmacy configured. First run detected.");
        return true;
         }

            // Everything is configured
          return false;
            }
         catch (SqlException sqlEx)
       {
          // Connection failed or database doesn't exist - first run
    Debug.WriteLine($"SQL Error checking server mode first run: {sqlEx.Message}");
        return true;
            }
      catch (Exception ex)
            {
    Debug.WriteLine($"Error checking server mode first run: {ex.Message}");
         // Assume first run if we can't determine
                return true;
         }
        }

    /// <summary>
        /// Checks first run for Client mode by verifying server IP configuration
    /// </summary>
        private static bool IsFirstRunClientMode(AppConfiguration config)
    {
        try
            {
    // Client mode: Check if server IP is configured in appsettings.json
                string serverIP = config.DatabaseSettings.Server;

    if (string.IsNullOrWhiteSpace(serverIP))
         {
       Debug.WriteLine("Client mode: Server IP is empty. First run detected.");
        return true;
        }

     // Check if the first run flag has been set
        bool firstRunCompleted = Preferences.Get(FIRST_RUN_FLAG_KEY, false);
      
                if (!firstRunCompleted)
          {
           Debug.WriteLine("Client mode: First run flag not set. First run detected.");
       return true;
                }

  // Server is configured and flag is set
Debug.WriteLine("Client mode: Configuration complete.");
         return false;
            }
  catch (Exception ex)
      {
       Debug.WriteLine($"Error checking client mode first run: {ex.Message}");
   // Assume first run if we can't determine
   return true;
          }
        }

        /// <summary>
 /// Marks the application as configured and setup complete
        /// </summary>
  public static void MarkAsConfigured()
        {
     try
            {
      Preferences.Set(FIRST_RUN_FLAG_KEY, true);
       Debug.WriteLine("Application marked as configured.");
      }
            catch (Exception ex)
            {
        Debug.WriteLine($"Error marking application as configured: {ex.Message}");
     throw;
            }
        }

        /// <summary>
        /// Resets the first run flag (useful for testing or reconfiguration)
     /// </summary>
        public static void ResetFirstRunFlag()
        {
            try
            {
 Preferences.Remove(FIRST_RUN_FLAG_KEY);
           Debug.WriteLine("First run flag has been reset.");
            }
    catch (Exception ex)
          {
 Debug.WriteLine($"Error resetting first run flag: {ex.Message}");
       throw;
      }
        }

        /// <summary>
    /// Tests database connectivity with current configuration
        /// </summary>
 /// <returns>True if connection successful, false otherwise</returns>
  public static async Task<bool> TestDatabaseConnectionAsync()
     {
     try
            {
                var config = ConfigurationManager.LoadConfiguration();
       string connectionString;

       if (config.DeploymentMode == "Server")
  {
         connectionString = string.Format(
        SERVER_CONNECTION_FORMAT,
      config.DatabaseSettings.Server,
   config.DatabaseSettings.Database);
     }
    else
   {
     if (string.IsNullOrWhiteSpace(config.DatabaseSettings.Server) ||
      string.IsNullOrWhiteSpace(config.DatabaseSettings.Password))
        {
          return false;
    }

        connectionString = string.Format(
     CLIENT_CONNECTION_FORMAT,
       config.DatabaseSettings.Server,
   config.DatabaseSettings.Database,
        config.DatabaseSettings.Username,
     config.DatabaseSettings.Password);
           }

              using var connection = new SqlConnection(connectionString);
  await connection.OpenAsync();
return true;
    }
            catch (Exception ex)
    {
       Debug.WriteLine($"Database connection test failed: {ex.Message}");
   return false;
          }
        }

        /// <summary>
 /// Gets the current deployment mode
        /// </summary>
        /// <returns>"Server" or "Client"</returns>
        public static string GetDeploymentMode()
      {
   try
            {
 var config = ConfigurationManager.LoadConfiguration();
    return config.DeploymentMode;
            }
       catch (Exception ex)
  {
         Debug.WriteLine($"Error getting deployment mode: {ex.Message}");
     return "Client"; // Default to Client mode
     }
        }

        /// <summary>
   /// Checks if the application needs database migration
        /// </summary>
        /// <returns>True if migrations are needed, false otherwise</returns>
        public static async Task<bool> NeedsDatabaseMigrationAsync()
 {
            try
      {
 // Only check if not first run
         if (await IsFirstRunAsync())
        {
       return true; // First run always needs migration
      }

 var config = ConfigurationManager.LoadConfiguration();
                string connectionString = ConfigurationManager.BuildConnectionString();

            using var connection = new SqlConnection(connectionString);
   await connection.OpenAsync();

     // Check if __EFMigrationsHistory table exists
                string checkMigrationTableQuery = @"
      SELECT COUNT(*) 
        FROM INFORMATION_SCHEMA.TABLES 
             WHERE TABLE_NAME = '__EFMigrationsHistory'";

     using var command = new SqlCommand(checkMigrationTableQuery, connection);
     var result = (int)await command.ExecuteScalarAsync();

         return result == 0; // Needs migration if table doesn't exist
}
        catch (Exception ex)
          {
     Debug.WriteLine($"Error checking migration status: {ex.Message}");
                return true; // Assume migration needed on error
            }
        }
    }
}

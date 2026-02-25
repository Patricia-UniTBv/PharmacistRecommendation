using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using PharmacistRecommendation.Helpers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PharmacistRecommendation.ViewModels
{
    public partial class ServerSetupWizardViewModel : ObservableObject
    {
        private static string BACPAC_FILE_PATH => Path.Combine(AppContext.BaseDirectory, "Database", "PharmacistRecommendationDB.bacpac");
        private static string SQLPACKAGE_PATH => Path.Combine(AppContext.BaseDirectory, "SqlPackage", "SqlPackage.exe");

        [ObservableProperty]
private bool isCreating;

        [ObservableProperty]
        private bool isCompleted;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
     private string progressMessage = string.Empty;

     [ObservableProperty]
        private double progressValue;

        public ServerSetupWizardViewModel()
        {
 }

        private async Task<string> GetServerIPAddressAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    string hostName = Dns.GetHostName();
                    IPAddress[] addresses = Dns.GetHostAddresses(hostName);

                    // Find first valid IPv4 address
                    foreach (var address in addresses)
                    {
                        // Check if it's IPv4
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            string ipString = address.ToString();

                            // Exclude loopback addresses (127.x.x.x)
                            if (ipString.StartsWith("127."))
                                continue;

                            // Exclude link-local addresses (169.254.x.x)
                            if (ipString.StartsWith("169.254."))
                                continue;

                            return ipString + "\\PHARMACYREC";
                        }
                    }
                    return "localhost\\PHARMACYREC";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting IP address: {ex.Message}");
                return "localhost\\PHARMACYREC";
            }
        }

        [RelayCommand]
        private async Task CreateDatabaseAsync()
     {
     IsCreating = true;
IsCompleted = false;
       HasError = false;
       StatusMessage = string.Empty;
         ProgressValue = 0;

            try
        {
      // Step 1: Verify .bacpac file exists
     ProgressMessage = "Verificare fisier bazs de date...";
     ProgressValue = 10;
        await Task.Delay(500);

   if (!File.Exists(BACPAC_FILE_PATH))
{
         throw new FileNotFoundException($"Fisierul bazei de date nu a fost gasit: {BACPAC_FILE_PATH}");
                }

      // Step 2: Check if SQL Server is accessible
      ProgressMessage = "Conectare la SQL Server...";
             ProgressValue = 20;
          await Task.Delay(500);

          var config = ConfigurationManager.LoadConfiguration();
          string server = config.DatabaseSettings.Server;
        string database = config.DatabaseSettings.Database;

                if (!await CheckSqlServerConnectionAsync(server))
   {
                 throw new Exception($"Nu se poate conecta la SQL Server: {server}\nVa rugam sa verificati daca SQL Server este pornit.");
                }

    // Step 3: Drop existing database if it exists
        ProgressMessage = "Pregatire baza de date...";
      ProgressValue = 30;
      await Task.Delay(500);

       await DropDatabaseIfExistsAsync(server, database);

     // Step 4: Restore database from .bacpac
     ProgressMessage = "Se creeaza baza de date... (acest proces poate dura cateva minute)";
       ProgressValue = 40;

       await RestoreDatabaseFromBacpacAsync(server, database);

          // Step 5: Verify database was created
                ProgressMessage = "Verificare baza de date...";
      ProgressValue = 90;
await Task.Delay(500);

      if (!await VerifyDatabaseExistsAsync(server, database))
          {
  throw new Exception("Baza de date a fost creata, dar nu poate fi gasita.");
                }

   // Step 5.5: Create appuser login for client connections
           ProgressMessage = "Creare utilizator pentru clienti...";
            ProgressValue = 95;
  await Task.Delay(500);

       await CreateAppUserLoginAsync(server, database);

   // Step 6: Mark as configured
                ProgressMessage = "Finalizare configurare...";
                ProgressValue = 100;
       await Task.Delay(500);

  FirstRunHelper.MarkAsConfigured();

   // Get server IP address for client connections
       string serverIP = await GetServerIPAddressAsync();

          IsCompleted = true;
   StatusMessage = "Baza de date a fost creata cu succes!\n\n" +
   "Serverul dumneavoastra este gata de utilizare.\n\n" +
          "========================================\n\n" +
          "INFORMATII PENTRU CALCULATOARELE CLIENT:\n\n" +
      $"Adresa IP Server: {serverIP}\n" +
   "Parola: Farmacie2025\n\n" +
      "Va rugam sa salvati aceste informatii!\n" +
"Acestea vor fi necesare pentru conectarea\n" +
         "calculatoarelor client la acest server.\n\n" +
    "========================================";
 }
        catch (Exception ex)
  {
          HasError = true;
     StatusMessage = $"A ap?rut o eroare:\n\n{ex.Message}\n\nVa rugam sa contactati asistenta tehnica.";
         Debug.WriteLine($"Database creation error: {ex}");
 }
            finally
      {
           IsCreating = false;
            }
      }

     private async Task<bool> CheckSqlServerConnectionAsync(string server)
   {
   try
      {
       // Increased timeout to 10 seconds for initial check
                string masterConnectionString = $"Server={server};Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=10";
                using var connection = new SqlConnection(masterConnectionString);
                await connection.OpenAsync();
                return true;
}
         catch (Exception ex)
            {
         Debug.WriteLine($"SQL Server connection check failed: {ex.Message}");
return false;
            }
     }

        private async Task DropDatabaseIfExistsAsync(string server, string database)
        {
try
 {
      string masterConnectionString = $"Server={server};Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=10";
  using var connection = new SqlConnection(masterConnectionString);
     await connection.OpenAsync();

      // Check if database exists
      string checkQuery = $"SELECT database_id FROM sys.databases WHERE name = '{database}'";
    using var checkCommand = new SqlCommand(checkQuery, connection);
       var result = await checkCommand.ExecuteScalarAsync();

      if (result != null)
        {
     Debug.WriteLine($"Database {database} exists, dropping it...");

             // Set database to single user mode to disconnect users
    string setSingleUserQuery = $@"
        IF EXISTS (SELECT name FROM sys.databases WHERE name = '{database}')
              BEGIN
           ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{database}];
   END";

     using var dropCommand = new SqlCommand(setSingleUserQuery, connection);
  dropCommand.CommandTimeout = 60;
   await dropCommand.ExecuteNonQueryAsync();

Debug.WriteLine($"Database {database} dropped successfully.");
        await Task.Delay(2000); // Give SQL Server time to clean up
     }
            }
  catch (Exception ex)
   {
     Debug.WriteLine($"Error dropping database: {ex.Message}");
    // Don't throw - we'll try to create anyway
    }
        }

        private async Task RestoreDatabaseFromBacpacAsync(string server, string database)
   {
            try
            {
 // Try to use SqlPackage.exe first (preferred method)
     if (File.Exists(SQLPACKAGE_PATH))
     {
     await RestoreUsingSqlPackageAsync(server, database);
   }
    else
          {
 // Fallback: Try to find SqlPackage in common locations
   string sqlPackagePath = FindSqlPackageExe();
 if (!string.IsNullOrEmpty(sqlPackagePath))
            {
  await RestoreUsingSqlPackageAsync(server, database, sqlPackagePath);
    }
       else
             {
                throw new FileNotFoundException(
  "SqlPackage.exe nu a fost gasit.\n\n" +
         "Va rugam sa instalati SQL Server Management Studio (SSMS)\n" +
          "pentru a continua.");
    }
           }
    }
      catch (Exception ex)
    {
     Debug.WriteLine($"Error restoring database: {ex}");
                throw;
         }
    }

        private async Task RestoreUsingSqlPackageAsync(string server, string database, string? sqlPackagePath = null)
        {
            // Use the bundled SqlPackage if no path provided
            sqlPackagePath ??= SQLPACKAGE_PATH;

            await Task.Run(() =>
            {
                try
                {
                    // Build SqlPackage command
                    string arguments = $"/Action:Import " +
                        $"/SourceFile:\"{BACPAC_FILE_PATH}\" " +
                        $"/TargetServerName:\"{server}\" " +
                        $"/TargetDatabaseName:\"{database}\" " +
                        $"/TargetTrustServerCertificate:True " +
                        $"/p:CommandTimeout=300";

                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = sqlPackagePath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using var process = new Process { StartInfo = processStartInfo };

                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputBuilder.AppendLine(e.Data);
                            Debug.WriteLine($"SqlPackage: {e.Data}");

                            // Update progress message with key milestones
                            if (e.Data.Contains("Importing"))
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    ProgressValue = 50;
                                    ProgressMessage = "Se importa datele în baza de date...";
                                });
                            }
                            else if (e.Data.Contains("Successfully imported"))
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    ProgressValue = 80;
                                    ProgressMessage = "Import finalizat cu succes!";
                                });
                            }
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorBuilder.AppendLine(e.Data);
                            Debug.WriteLine($"SqlPackage Error: {e.Data}");
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Use blocking WaitForExit on this background thread to ensure all output is processed
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        string errorMessage = errorBuilder.ToString();
                        if (string.IsNullOrWhiteSpace(errorMessage))
                        {
                            errorMessage = outputBuilder.ToString();
                        }

                        throw new Exception($"SqlPackage a esuat cu codul {process.ExitCode}:\n{errorMessage}");
                    }

                    Debug.WriteLine("Database restored successfully using SqlPackage.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in RestoreUsingSqlPackageAsync: {ex}");
                    throw new Exception($"Eroare la importul bazei de date: {ex.Message}", ex);
                }
            });
        }

        private string FindSqlPackageExe()
        {
            // Common installation paths for SqlPackage.exe
        string[] searchPaths = new[]
  {
            @"C:\Program Files\Microsoft SQL Server\160\DAC\bin\SqlPackage.exe",
        @"C:\Program Files (x86)\Microsoft SQL Server\160\DAC\bin\SqlPackage.exe",
        
        // SQL Server 2019 (150)
        @"C:\Program Files\Microsoft SQL Server\150\DAC\bin\SqlPackage.exe",
        @"C:\Program Files (x86)\Microsoft SQL Server\150\DAC\bin\SqlPackage.exe",
        
        // SQL Server 2017 (140)
        @"C:\Program Files\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe",
        @"C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe",
        
        // Visual Studio 2022 paths
        @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe",
        @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe",
        @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe",
        
        // Visual Studio 2019 paths
        @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe",
        @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe",
        @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe",
            };

          foreach (var path in searchPaths)
            {
if (File.Exists(path))
         {
  Debug.WriteLine($"Found SqlPackage.exe at: {path}");
       return path;
    }
          }

  Debug.WriteLine("SqlPackage.exe not found in common locations.");
 return string.Empty;
        }

        private async Task<bool> VerifyDatabaseExistsAsync(string server, string database)
        {
       try
            {
    string masterConnectionString = $"Server={server};Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=5";
         using var connection = new SqlConnection(masterConnectionString);
      await connection.OpenAsync();

     string checkQuery = $"SELECT database_id FROM sys.databases WHERE name = '{database}'";
       using var command = new SqlCommand(checkQuery, connection);
       var result = await command.ExecuteScalarAsync();

         return result != null;
        }
     catch (Exception ex)
  {
      Debug.WriteLine($"Error verifying database existence: {ex.Message}");
      return false;
    }
  }

        private async Task CreateAppUserLoginAsync(string server, string database)
        {
      try
      {
    Debug.WriteLine("Creating appuser login and database user...");
      
           string masterConnectionString = $"Server={server};Database=master;Integrated Security=true;TrustServerCertificate=true;";
     
     using var connection = new SqlConnection(masterConnectionString);
    await connection.OpenAsync();
        
     // Step 1: Create SQL Server login
    string createLoginSql = @"
     IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'appuser')
BEGIN
           CREATE LOGIN appuser WITH PASSWORD = 'Farmacie2025';
         PRINT 'Login appuser created successfully';
       END
            ELSE
       BEGIN
  ALTER LOGIN appuser WITH PASSWORD = 'Farmacie2025';
         PRINT 'Login appuser password updated';
     END
          
       -- Grant remote connection permission
    GRANT CONNECT SQL TO appuser;
      ";
                
       using var loginCommand = new SqlCommand(createLoginSql, connection);
      await loginCommand.ExecuteNonQueryAsync();
                
     Debug.WriteLine("appuser login created with remote permissions");
     
       // Step 2: Create database user and grant permissions
       string dbConnectionString = $"Server={server};Database={database};Integrated Security=true;TrustServerCertificate=true;";
        using var dbConnection = new SqlConnection(dbConnectionString);
       await dbConnection.OpenAsync();
                
        string createUserSql = @"
            IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'appuser')
  BEGIN
          CREATE USER appuser FOR LOGIN appuser;
         PRINT 'User appuser created in database';
           END
         
                  -- Grant necessary permissions
 ALTER ROLE db_datareader ADD MEMBER appuser;
    ALTER ROLE db_datawriter ADD MEMBER appuser;
          GRANT EXECUTE TO appuser;
     
         PRINT 'Permissions granted to appuser';
       ";
        
                using var userCommand = new SqlCommand(createUserSql, dbConnection);
   await userCommand.ExecuteNonQueryAsync();
                
              Debug.WriteLine("Database user created and permissions granted");
            }
   catch (Exception ex)
          {
        Debug.WriteLine($"Error creating appuser: {ex.Message}");
      throw new Exception($"Eroare la crearea utilizatorului appuser: {ex.Message}", ex);
            }
        }

        [RelayCommand]
        private async Task FinishAsync()
        {
  try
          {
         // Navigate to main application or restart
             await App.NavigateToMainShell();
      }
            catch (Exception ex)
    {
          Debug.WriteLine($"Error finishing setup: {ex.Message}");
           // If navigation fails, just close the current page
        if (Application.Current?.MainPage != null)
      {
     await Application.Current.MainPage.DisplayAlert(
"Configurare Completa",
     "Va rugam sa reporniti aplicatia pentru a continua.",
   "OK");
     }
    }
        }

        [RelayCommand]
        private void Cancel()
        {
            // Close the application or go back
         Application.Current?.Quit();
      }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using PharmacistRecommendation.Helpers;
using System.Diagnostics;

namespace PharmacistRecommendation.ViewModels
{
 public partial class ServerSetupWizardViewModel : ObservableObject
    {
        private const string BACPAC_FILE_PATH = @"C:\Users\cryst\Source\Repos\PharmacistRecommendation\PharmacistRecommendationDB.bacpac";
        private const string SQLPACKAGE_PATH = @"C:\Program Files\Microsoft SQL Server\160\DAC\bin\SqlPackage.exe";

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
     ProgressMessage = "Verificare fisier baza de date...";
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
     ProgressMessage = "Se creaza baza de date... (acest proces poate dura cateva minute)";
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

   // Step 6: Mark as configured
                ProgressMessage = "Finalizare configurare...";
                ProgressValue = 100;
       await Task.Delay(500);

  FirstRunHelper.MarkAsConfigured();

             IsCompleted = true;
     StatusMessage = "Baza de date a fost creata cu succes!\n\nServerul dumneavoastra este gata de utilizare.";
            }
            catch (Exception ex)
  {
          HasError = true;
     StatusMessage = $"A aparut o eroare:\n\n{ex.Message}\n\nVa rugam sa contactati asistenta tehnica.";
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
       string masterConnectionString = $"Server={server};Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=5";
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
             "Va rugam sa instalati SQL Server Data Tools (SSDT) sau\n" +
      "SQL Server Management Studio (SSMS) pentru a continua.");
    }
           }
    }
      catch (Exception ex)
    {
     Debug.WriteLine($"Error restoring database: {ex}");
                throw;
         }
    }

        private async Task RestoreUsingSqlPackageAsync(string server, string database, string sqlPackagePath = SQLPACKAGE_PATH)
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

 var outputBuilder = new System.Text.StringBuilder();
          var errorBuilder = new System.Text.StringBuilder();

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
                  ProgressMessage = "Se importa datele in baza de date...";
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

                await process.WaitForExitAsync();

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
        }

        private string FindSqlPackageExe()
        {
            // Common installation paths for SqlPackage.exe
        string[] searchPaths = new[]
  {
             @"C:\Program Files\Microsoft SQL Server\160\DAC\bin\SqlPackage.exe",
             @"C:\Program Files\Microsoft SQL Server\150\DAC\bin\SqlPackage.exe",
        @"C:\Program Files\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe",
      @"C:\Program Files (x86)\Microsoft SQL Server\160\DAC\bin\SqlPackage.exe",
@"C:\Program Files (x86)\Microsoft SQL Server\150\DAC\bin\SqlPackage.exe",
             @"C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe",
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
      "Va rugam sa reponiti aplicatia pentru a continua.",
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

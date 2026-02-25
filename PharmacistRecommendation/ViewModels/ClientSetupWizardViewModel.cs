using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using PharmacistRecommendation.Helpers;
using System.Diagnostics;

namespace PharmacistRecommendation.ViewModels
{
    public partial class ClientSetupWizardViewModel : ObservableObject
    {
        [ObservableProperty]
     private string serverAddress = string.Empty;

        [ObservableProperty]
  private string appUserPassword = string.Empty;

        [ObservableProperty]
  private bool isTesting;

        [ObservableProperty]
        private bool isConnectionSuccessful;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private string progressMessage = string.Empty;

        public ClientSetupWizardViewModel()
        {
   }

        [RelayCommand]
      private async Task TestConnectionAsync()
        {
    // Validate inputs
     if (string.IsNullOrWhiteSpace(ServerAddress))
       {
   HasError = true;
        StatusMessage = "V? rug?m s? introduce?i adresa serverului.";
   return;
   }

         if (string.IsNullOrWhiteSpace(AppUserPassword))
     {
   HasError = true;
StatusMessage = "V? rug?m s? introduce?i parola pentru utilizatorul appuser.";
  return;
            }

        IsTesting = true;
   IsConnectionSuccessful = false;
 HasError = false;
       StatusMessage = string.Empty;

     try
       {
    // Step 1: Save temporary configuration
     ProgressMessage = "Salvare configurare temporar?...";
       await Task.Delay(300);

         var config = ConfigurationManager.LoadConfiguration();
           config.DatabaseSettings.Server = ServerAddress.Trim();
config.DatabaseSettings.Password = AppUserPassword;
   config.DatabaseSettings.Username = "appuser";
       config.DatabaseSettings.UseSqlAuthentication = true;
 config.DatabaseSettings.UseWindowsAuthentication = false;

    // Save to user config file
ConfigurationManager.SaveUserConfiguration(config.DatabaseSettings);

  // Step 2: Test connection
ProgressMessage = "Testare conexiune la server...";
            await Task.Delay(500);

 bool connectionSuccess = await FirstRunHelper.TestDatabaseConnectionAsync();

         if (connectionSuccess)
       {
       IsConnectionSuccessful = true;
      StatusMessage = "Conexiune reusita!\n\nServerul este accesibil si baza de date este configurata corect.";
         Debug.WriteLine("Client connection test successful.");
      }
   else
                {
                    HasError = true;
                    StatusMessage = $"Conexiune eșuată!\n\n" +
                        $"Server: {config.DatabaseSettings.Server}\n" +
                        $"Database: {config.DatabaseSettings.Database}\n" +
                        $"Username: {config.DatabaseSettings.Username}\n\n" +
                        $"Verificați:\n" +
                        "- Adresa serverului este corectă\n" +
                        "- Parola pentru utilizatorul 'appuser' este corectă\n" +
                        "- Serverul SQL este accesibil din rețeaua dumneavoastră\n" +
                        "- Baza de date PharmacistRecommendationDB există pe server";
                    Debug.WriteLine("Client connection test failed.");
                }
            }
            catch (SqlException sqlEx)
            {
                HasError = true;
                StatusMessage = $"Eroare SQL Server:\n\n" +
                    $"Mesaj: {sqlEx.Message}\n" +
                    $"Număr eroare: {sqlEx.Number}\n\n" +
                    $"Server: {ServerAddress}\n" +
                    $"Database: PharmacistRecommendationDB\n" +
                    $"Username: appuser";
                Debug.WriteLine($"SQL Error: {sqlEx}");
            }
            catch (Exception ex)
            {
                HasError = true;
                StatusMessage = $"Eroare generală:\n\n{ex.Message}";
                Debug.WriteLine($"General error: {ex}");
            }
            finally
{
     IsTesting = false;
           ProgressMessage = string.Empty;
        }
    }

        [RelayCommand]
        private async Task FinishAsync()
        {
       try
            {
   if (!IsConnectionSuccessful)
    {
      await ShowAlert("Atentie", 
    "Va rugam sa testati conexiunea înainte de a finaliza configurarea.", 
              "OK");
         return;
      }

         // Configuration is already saved during test, just mark as configured
                FirstRunHelper.MarkAsConfigured();

            Debug.WriteLine("Client configuration completed and marked as configured.");

     // Navigate to main application
       await App.NavigateToMainShell();
 }
            catch (Exception ex)
            {
    Debug.WriteLine($"Error finishing client setup: {ex.Message}");
                await ShowAlert(
       "Configurare Complet?",
"Configurarea a fost salvat?. V? rug?m s? reporni?i aplica?ia pentru a continua.",
          "OK");
      }
        }

        [RelayCommand]
        private void Cancel()
        {
    // Close the application
    Application.Current?.Quit();
        }

        private async Task ShowAlert(string title, string message, string accept)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, accept);
 }
     }
    }
}

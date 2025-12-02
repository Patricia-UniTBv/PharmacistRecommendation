using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    StatusMessage = "Va rugam sa introduceti adresa serverului.";
    return;
         }

         if (string.IsNullOrWhiteSpace(AppUserPassword))
            {
         HasError = true;
    StatusMessage = "Va rugam sa introduceti parola pentru utilizatorul appuser.";
                return;
            }

          IsTesting = true;
      IsConnectionSuccessful = false;
       HasError = false;
            StatusMessage = string.Empty;

            try
 {
          // Step 1: Save temporary configuration
         ProgressMessage = "Salvare configurare temporara...";
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
         StatusMessage = "Conexiune esuata!\n\nVerificati:\n" +
                  "- Adresa serverului este corecta\n" +
      "- Parola pentru utilizatorul 'appuser' este corecta\n" +
       "- Serverul SQL este accesibil din reteaua dumneavoastra\n" +
         "- Baza de date PharmacistRecommendationDB exista pe server";
     Debug.WriteLine("Client connection test failed.");
   }
         }
    catch (Exception ex)
{
       HasError = true;
      StatusMessage = $"A aparut o eroare:\n\n{ex.Message}\n\nVa rugam sa verificati configurarea.";
      Debug.WriteLine($"Client connection test error: {ex}");
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
    "Va rugam sa testati conexiunea inainte de a finaliza configurarea.", 
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
    "Configurare Completa",
      "Configurarea a fost salvata. Va rugam sa reponiti aplicatia pentru a continua.",
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

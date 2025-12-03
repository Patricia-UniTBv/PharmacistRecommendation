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
      StatusMessage = "Conexiune reu?it?!\n\nServerul este accesibil ?i baza de date este configurat? corect.";
         Debug.WriteLine("Client connection test successful.");
      }
   else
    {
        HasError = true;
     StatusMessage = "Conexiune e?uat?!\n\nVerifica?i:\n" +
          "- Adresa serverului este corect?\n" +
   "- Parola pentru utilizatorul 'appuser' este corect?\n" +
        "- Serverul SQL este accesibil din re?eaua dumneavoastr?\n" +
   "- Baza de date PharmacistRecommendationDB exist? pe server";
   Debug.WriteLine("Client connection test failed.");
      }
 }
        catch (Exception ex)
  {
 HasError = true;
       StatusMessage = $"A ap?rut o eroare:\n\n{ex.Message}\n\nV? rug?m s? verifica?i configurarea.";
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
      await ShowAlert("Aten?ie", 
    "V? rug?m s? testa?i conexiunea înainte de a finaliza configurarea.", 
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

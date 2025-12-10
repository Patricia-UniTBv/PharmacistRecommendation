using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using PharmacistRecommendation.Helpers;

namespace PharmacistRecommendation.ViewModels
{
    public partial class ServerConfigurationViewModel : ObservableObject
    {
        [ObservableProperty]
        private string serverAddress = string.Empty;

        [ObservableProperty]
        private string databaseName = "PharmacistRecommendationDB";

        [ObservableProperty]
        private string instanceName = "PHARMACYREC";

        [ObservableProperty]
        private string username = "appuser";

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private Color statusColor = Colors.Black;

        [ObservableProperty]
        private bool hasStatusMessage = false;

        [ObservableProperty]
        private bool isClientMode = true;

        [ObservableProperty]
        private bool showInstanceName = true;

        [ObservableProperty]
        private string modeDescription = string.Empty;

        [ObservableProperty]
        private string modeInfo = string.Empty;

        public ServerConfigurationViewModel()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            var config = ConfigurationManager.LoadConfiguration();
            
            IsClientMode = config.DeploymentMode == "Client";
            
            if (IsClientMode)
            {
                ModeDescription = "Mod Client - Conectare la Server în Re?ea";
                ModeInfo = "Configura?i adresa serverului central pentru a v? conecta la baza de date.";
            }
            else
            {
                ModeDescription = "Mod Server - Server Central";
                ModeInfo = "Aceast? instalare este configurat? ca server central cu conexiune local?.";
                ServerAddress = "localhost";
            }

            var dbSettings = config.DatabaseSettings;
            
            if (!string.IsNullOrWhiteSpace(dbSettings.Server))
            {
                // Parse server address and instance
                var parts = dbSettings.Server.Split('\\');
                ServerAddress = parts[0];
                if (parts.Length > 1)
                {
                    InstanceName = parts[1];
                }
            }
            
            DatabaseName = dbSettings.Database;
            Username = dbSettings.Username;
            Password = dbSettings.Password;
        }

        [RelayCommand]
        private async Task TestConnection()
        {
            StatusMessage = "Se testeaz? conexiunea...";
            StatusColor = Colors.Blue;
            HasStatusMessage = true;

            await Task.Delay(100); // Allow UI to update

            try
            {
                string connectionString = BuildConnectionString();
                
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                StatusMessage = "? Conexiune reu?it?! Baza de date este accesibil?.";
                StatusColor = Colors.Green;
                HasStatusMessage = true;

                await Application.Current.MainPage.DisplayAlert(
                    "Succes", 
                    "Conexiunea la server a fost stabilit? cu succes!", 
                    "OK");
            }
            catch (SqlException ex)
            {
                StatusMessage = $"? Eroare de conexiune: {ex.Message}";
                StatusColor = Colors.Red;
                HasStatusMessage = true;

                string errorDetails = ex.Number switch
                {
                    -1 => "Serverul nu poate fi g?sit sau nu este accesibil. Verifica?i adresa IP ?i conexiunea la re?ea.",
                    18456 => "Autentificare e?uat?. Verifica?i utilizatorul ?i parola.",
                    4060 => "Baza de date nu exist? sau nu ave?i acces la ea.",
                    _ => ex.Message
                };

                await Application.Current.MainPage.DisplayAlert(
                    "Eroare de Conexiune", 
                    errorDetails, 
                    "OK");
            }
            catch (Exception ex)
            {
                StatusMessage = $"? Eroare: {ex.Message}";
                StatusColor = Colors.Red;
                HasStatusMessage = true;

                await Application.Current.MainPage.DisplayAlert(
                    "Eroare", 
                    $"A ap?rut o eroare: {ex.Message}", 
                    "OK");
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (IsClientMode && string.IsNullOrWhiteSpace(ServerAddress))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Eroare de Validare", 
                    "Adresa serverului este obligatorie.", 
                    "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Eroare de Validare", 
                    "Numele bazei de date este obligatoriu.", 
                    "OK");
                return;
            }

            if (IsClientMode)
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Eroare de Validare", 
                        "Utilizatorul este obligatoriu.", 
                        "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Eroare de Validare", 
                        "Parola este obligatorie.", 
                        "OK");
                    return;
                }
            }

            try
            {
                var config = ConfigurationManager.LoadConfiguration();
                
                // Build server string with instance
                string serverString = ServerAddress;
                if (!string.IsNullOrWhiteSpace(InstanceName))
                {
                    serverString = $"{ServerAddress}\\{InstanceName}";
                }

                config.DatabaseSettings.Server = serverString;
                config.DatabaseSettings.Database = DatabaseName;
                config.DatabaseSettings.Username = Username;
                config.DatabaseSettings.Password = Password;

                ConfigurationManager.SaveUserConfiguration(config.DatabaseSettings);

                StatusMessage = "? Configurare salvat? cu succes!";
                StatusColor = Colors.Green;
                HasStatusMessage = true;

                await Application.Current.MainPage.DisplayAlert(
                    "Succes", 
                    "Configurarea a fost salvat?. V? rug?m s? reporni?i aplica?ia pentru a aplica modific?rile.", 
                    "OK");
            }
            catch (Exception ex)
            {
                StatusMessage = $"? Eroare la salvare: {ex.Message}";
                StatusColor = Colors.Red;
                HasStatusMessage = true;

                await Application.Current.MainPage.DisplayAlert(
                    "Eroare", 
                    $"Nu s-a putut salva configurarea: {ex.Message}", 
                    "OK");
            }
        }

        private string BuildConnectionString()
        {
            string serverString = ServerAddress;
            if (!string.IsNullOrWhiteSpace(InstanceName))
            {
                serverString = $"{ServerAddress}\\{InstanceName}";
            }

            if (IsClientMode)
            {
                return $"Server={serverString};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=true;";
            }
            else
            {
                return $"Server={serverString};Database={DatabaseName};Integrated Security=true;TrustServerCertificate=true;";
            }
        }
    }
}

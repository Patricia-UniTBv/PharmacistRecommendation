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
                ModeDescription = "Mod Client - Conectare la Server în Rețea";
                ModeInfo = "Configurați adresa serverului central pentru a vă conecta la baza de date.";
            }
            else
            {
                ModeDescription = "Mod Server - Server Central";
                ModeInfo = "Această instalare este configurată ca server central cu conexiune locală.";
                ServerAddress = "localhost";
            }

            var dbSettings = config.DatabaseSettings;

            if (!string.IsNullOrWhiteSpace(dbSettings.Server))
            {
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
            StatusMessage = "Se testează conexiunea...";
            StatusColor = Colors.Blue;
            HasStatusMessage = true;

            await Task.Delay(100);

            try
            {
                string connectionString = BuildConnectionString();

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                StatusMessage = "✓ Conexiune reușită! Baza de date este accesibilă.";
                StatusColor = Colors.Green;
                HasStatusMessage = true;

                await Shell.Current.DisplayAlert(
                    "Succes",
                    "Conexiunea la server a fost stabilită cu succes!",
                    "OK");
            }
            catch (SqlException ex)
            {
                StatusMessage = $"✗ Eroare de conexiune: {ex.Message}";
                StatusColor = Colors.Red;
                HasStatusMessage = true;

                string errorDetails = ex.Number switch
                {
                    -1 => "Serverul nu poate fi găsit sau nu este accesibil. Verificați adresa IP și conexiunea la rețea.",
                    18456 => "Autentificare eșuată. Verificați utilizatorul și parola.",
                    4060 => "Baza de date nu există sau nu aveți acces la ea.",
                    _ => ex.Message
                };

                await Shell.Current.DisplayAlert("Eroare de Conexiune", errorDetails, "OK");
            }
            catch (Exception ex)
            {
                StatusMessage = $"✗ Eroare: {ex.Message}";
                StatusColor = Colors.Red;
                HasStatusMessage = true;

                await Shell.Current.DisplayAlert("Eroare", $"A apărut o eroare: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (IsClientMode && string.IsNullOrWhiteSpace(ServerAddress))
            {
                await Shell.Current.DisplayAlert("Eroare de Validare", "Adresa serverului este obligatorie.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                await Shell.Current.DisplayAlert("Eroare de Validare", "Numele bazei de date este obligatoriu.", "OK");
                return;
            }

            if (IsClientMode)
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    await Shell.Current.DisplayAlert("Eroare de Validare", "Utilizatorul este obligatoriu.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Eroare de Validare", "Parola este obligatorie.", "OK");
                    return;
                }
            }

            try
            {
                var config = ConfigurationManager.LoadConfiguration();

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

                StatusMessage = "✓ Configurare salvată cu succes!";
                StatusColor = Colors.Green;
                HasStatusMessage = true;

                await Shell.Current.DisplayAlert(
                    "Succes",
                    "Configurarea a fost salvată. Vă rugăm să reporniți aplicația pentru a aplica modificările.",
                    "OK");
            }
            catch (Exception ex)
            {
                StatusMessage = $"✗ Eroare la salvare: {ex.Message}";
                StatusColor = Colors.Red;
                HasStatusMessage = true;

                await Shell.Current.DisplayAlert("Eroare", $"Nu s-a putut salva configurarea: {ex.Message}", "OK");
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

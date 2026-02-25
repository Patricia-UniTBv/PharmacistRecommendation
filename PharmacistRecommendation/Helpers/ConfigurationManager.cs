using System.Text.Json;

namespace PharmacistRecommendation.Helpers
{
    public class AppConfiguration
    {
        public string DeploymentMode { get; set; } = "Client";
        public ConnectionStrings ConnectionStrings { get; set; } = new();
        public DatabaseSettings DatabaseSettings { get; set; } = new();
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; } = string.Empty;
    }

    public class DatabaseSettings
    {
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = "PharmacistRecommendationDB";
        public bool UseWindowsAuthentication { get; set; } = false;
        public bool UseSqlAuthentication { get; set; } = false;
        public string Username { get; set; } = "appuser";
        public string Password { get; set; } = string.Empty;
        public bool TrustServerCertificate { get; set; } = true;
    }

    public static class ConfigurationManager
    {
        private static AppConfiguration? _configuration;
        private static readonly string ConfigFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
            "PharmacistRecommendation");
        private static readonly string UserConfigPath = Path.Combine(ConfigFolder, "config.json");

        public static AppConfiguration LoadConfiguration()
        {
            if (_configuration != null)
                return _configuration;

            // Ensure config directory exists
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            // Try to load embedded appsettings.json from application directory
            string appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            
            if (File.Exists(appSettingsPath))
            {
                try
                {
                    var json = File.ReadAllText(appSettingsPath);
                    _configuration = JsonSerializer.Deserialize<AppConfiguration>(json);
                    
                    if (_configuration != null)
                    {
                        // For Client mode, check if user-specific configuration exists
                        if (_configuration.DeploymentMode == "Client")
                        {
                            LoadOrCreateClientConfiguration(_configuration);
                        }
                        return _configuration;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading appsettings.json: {ex.Message}");
                }
            }

            // Fallback: create default client configuration
            _configuration = new AppConfiguration
            {
                DeploymentMode = "Client",
                DatabaseSettings = new DatabaseSettings
                {
                    Server = "",
                    Database = "PharmacistRecommendationDB",
                    UseSqlAuthentication = true,
                    Username = "appuser",
                    Password = "",
                    TrustServerCertificate = true
                }
            };

            LoadOrCreateClientConfiguration(_configuration);
            return _configuration;
        }

        private static void LoadOrCreateClientConfiguration(AppConfiguration baseConfig)
        {
            if (File.Exists(UserConfigPath))
            {
                try
                {
                    var json = File.ReadAllText(UserConfigPath);
                    var userConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    
                    if (userConfig != null)
                    {
                        // Migrate old config format to new format if needed
                        if (userConfig.ContainsKey("SqlServer"))
                        {
                            baseConfig.DatabaseSettings.Server = userConfig["SqlServer"]?.ToString() ?? "";
                        }
                        if (userConfig.ContainsKey("Database"))
                        {
                            baseConfig.DatabaseSettings.Database = userConfig["Database"]?.ToString() ?? "PharmacistRecommendationDB";
                        }
                        if (userConfig.ContainsKey("ServerIP"))
                        {
                            baseConfig.DatabaseSettings.Server = userConfig["ServerIP"]?.ToString() ?? "";
                        }
                        if (userConfig.ContainsKey("Password"))
                        {
                            baseConfig.DatabaseSettings.Password = userConfig["Password"]?.ToString() ?? "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading user config: {ex.Message}");
                }
            }
            else
            {
                // Create default user config for client mode
                SaveUserConfiguration(baseConfig.DatabaseSettings);
            }
        }

        public static void SaveUserConfiguration(DatabaseSettings settings)
        {
            try
            {
                var config = new Dictionary<string, object>
                {
                    { "SqlServer", settings.Server },
                    { "Database", settings.Database },
                    { "ServerIP", settings.Server },
                    { "Username", settings.Username },
                    { "Password", settings.Password },
                    { "TrustServerCertificate", settings.TrustServerCertificate }
                };

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(UserConfigPath, json);

                _configuration = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving user config: {ex.Message}");
            }
        }

        public static string BuildConnectionString()
        {
            var config = LoadConfiguration();
            var dbSettings = config.DatabaseSettings;

            if (config.DeploymentMode == "Server" || dbSettings.UseWindowsAuthentication)
            {
                // Server mode: Windows Authentication
                return $"Server={dbSettings.Server};Database={dbSettings.Database};Integrated Security=true;TrustServerCertificate={dbSettings.TrustServerCertificate};";
            }
            else
            {
                // Client mode: SQL Authentication
                if (string.IsNullOrWhiteSpace(dbSettings.Server))
                {
                    throw new InvalidOperationException("Server address is not configured. Please configure the server connection.");
                }

                return $"Server={dbSettings.Server};Database={dbSettings.Database};User Id={dbSettings.Username};Password={dbSettings.Password};TrustServerCertificate={dbSettings.TrustServerCertificate};";
            }
        }

        public static bool IsServerMode()
        {
            var config = LoadConfiguration();
            return config.DeploymentMode == "Server";
        }

        public static bool IsClientMode()
        {
            return !IsServerMode();
        }

        public static bool IsConfigured()
        {
            var config = LoadConfiguration();
            
            if (config.DeploymentMode == "Server")
                return true; // Server mode is always configured with localhost

            // Client mode needs server IP configured
            return !string.IsNullOrWhiteSpace(config.DatabaseSettings.Server);
        }
    }
}

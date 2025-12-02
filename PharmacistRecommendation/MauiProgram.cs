using CommunityToolkit.Maui;
using Entities.Data;
using Entities.Repository;
using Entities.Repository.Interfaces;
using Entities.Services;
using Entities.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI;
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Services;
using PharmacistRecommendation.ViewModels;
using PharmacistRecommendation.Views;
using WinRT.Interop;
using QuestPDF.Infrastructure;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Windows.Storage;
using WinRT.Interop;
#endif

namespace PharmacistRecommendation
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            QuestPDF.Settings.License = LicenseType.Community;

            builder.UseMauiCommunityToolkit();

            // Load configuration from appsettings.json and user config
            string connectionString;
            try
            {
                connectionString = ConfigurationManager.BuildConnectionString();
                
                // Log configuration mode for debugging
                var config = ConfigurationManager.LoadConfiguration();
                System.Diagnostics.Debug.WriteLine($"Deployment Mode: {config.DeploymentMode}");
                
                // Safely log connection string (mask password if it exists)
                string safeConnectionString = connectionString;
                if (!string.IsNullOrWhiteSpace(config.DatabaseSettings.Password))
                {
                    safeConnectionString = connectionString.Replace(config.DatabaseSettings.Password, "****");
                }
                System.Diagnostics.Debug.WriteLine($"Connection String: {safeConnectionString}");
            }
            catch (InvalidOperationException ex)
            {
                // If server is not configured in client mode, use a placeholder
                // The app will show configuration dialog on startup
                System.Diagnostics.Debug.WriteLine($"Configuration not complete: {ex.Message}");
                connectionString = "Server=localhost;Database=PharmacistRecommendationDB;Integrated Security=true;TrustServerCertificate=true;";
            }

            // Register DbContext with the connection string
            builder.Services.AddDbContext<PharmacistRecommendationDbContext>(options =>
                options.UseSqlServer(connectionString),
                contextLifetime: ServiceLifetime.Transient,
                optionsLifetime: ServiceLifetime.Transient);

            // Register Views
            builder.Services.AddTransient<MonitoringView>();
            builder.Services.AddTransient<UsersManagementView>();
            builder.Services.AddTransient<CardConfigurationView>();
            builder.Services.AddTransient<GdprConfigurationView>();
            builder.Services.AddTransient<MixedActIssuanceView>();
            builder.Services.AddTransient<AdministrationModesView>();
            builder.Services.AddTransient<ImportConfigurationView>();
            builder.Services.AddTransient<AddPharmacyView>();
            builder.Services.AddTransient<EmailConfigurationView>();
            builder.Services.AddTransient<ServerConfigurationView>();
            
            builder.Services.AddTransient<MedicationView>();
            builder.Services.AddTransient<AddEditMedicationView>();
            builder.Services.AddTransient<ConflictResolutionView>();

            builder.Services.AddTransient<ReportsView>();
            builder.Services.AddTransient<MainPageView>();

            // Register ViewModels
            builder.Services.AddTransient<MonitoringViewModel>();
            builder.Services.AddTransient<PharmacistConfigurationViewModel>();
            builder.Services.AddTransient<UsersManagementViewModel>();
            builder.Services.AddTransient<CardConfigurationViewModel>();
            builder.Services.AddTransient<GdprConfigurationViewModel>();
            builder.Services.AddTransient<MixedActIssuanceViewModel>();
            builder.Services.AddTransient<AdministrationModesViewModel>();
            builder.Services.AddTransient<ImportConfigurationViewModel>();
            builder.Services.AddTransient<AddPharmacyViewModel>();
            builder.Services.AddTransient<EmailConfigurationViewModel>();
            builder.Services.AddTransient<ServerConfigurationViewModel>();

            builder.Services.AddTransient<MedicationViewModel>();
            builder.Services.AddTransient<AddEditMedicationViewModel>();
            builder.Services.AddTransient<ConflictResolutionViewModel>();
            
            builder.Services.AddTransient<ReportsViewModel>();
            builder.Services.AddTransient<MainPageViewModel>();

            // Register Services
            builder.Services.AddScoped<IMonitoringService, MonitoringService>();
            builder.Services.AddScoped<IMonitoringRepository, MonitoringRepository>();

            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();

            builder.Services.AddScoped<IPdfReportService, PdfReportService>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<IPharmacyCardService, PharmacyCardService>();
            builder.Services.AddScoped<IPharmacyCardRepository, PharmacyCardRepository>();

            builder.Services.AddScoped<IPharmacyService, PharmacyService>();
            builder.Services.AddScoped<IPharmacyRepository, PharmacyRepository>();

            builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
            builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();

            builder.Services.AddScoped<IPrescriptionMedicationRepository, PrescriptionMedicationRepository>();
            builder.Services.AddScoped<IPrescriptionMedicationService, PrescriptionMedicationService>();

            builder.Services.AddScoped<IAdministrationModeRepository, AdministrationModeRepository>();
            builder.Services.AddScoped<IAdministrationModeService, AdministrationModeService>();

            builder.Services.AddScoped<IImportConfigurationRepository, ImportConfigurationRepository>();
            builder.Services.AddScoped<IImportConfigurationService, ImportConfigurationService>();

            builder.Services.AddScoped<IEmailConfigurationService, EmailConfigurationService>();

            builder.Services.AddScoped<IMedicationService, MedicationService>();
            builder.Services.AddScoped<IMedicationImportService, MedicationImportService>();
            builder.Services.AddScoped<ICsvFileParser, CsvFileParser>();
            builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();

            builder.Services.AddScoped<ISecureStorageService, MauiSecureStorageService>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<LoginViewModel>();

            builder.Services.AddTransient<LoginAddUserView>();
            builder.Services.AddTransient<LoginAddUserViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
#if WINDOWS
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(windows =>
                {
                    windows.OnWindowCreated(window =>
                    {
                        var hwnd = WindowNative.GetWindowHandle(window);
                        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                        var appWindow = AppWindow.GetFromWindowId(windowId);

                        appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                    });
                });
            });
#endif
        }
    }
}

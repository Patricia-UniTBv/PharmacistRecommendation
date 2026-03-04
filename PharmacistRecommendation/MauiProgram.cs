using CommunityToolkit.Maui;
using Entities.Data;
using Entities.Repository;
using Entities.Repository.Interfaces;
using Entities.Services;
using Entities.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Services;
using PharmacistRecommendation.ViewModels;
using PharmacistRecommendation.Views;
using QuestPDF.Infrastructure;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;
#endif

namespace PharmacistRecommendation
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Global exception handlers — catch unhandled exceptions before they crash the app
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                System.Diagnostics.Debug.WriteLine($"[FATAL] AppDomain.UnhandledException: {ex}");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] UnobservedTaskException: {e.Exception}");
                e.SetObserved(); // Prevent app termination
            };

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
            builder.Services.AddTransient<ServerSetupWizard>();
            builder.Services.AddTransient<ClientSetupWizard>();
            
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
            builder.Services.AddTransient<ServerSetupWizardViewModel>();
            builder.Services.AddTransient<ClientSetupWizardViewModel>();

            builder.Services.AddTransient<MedicationViewModel>();
            builder.Services.AddTransient<AddEditMedicationViewModel>();
            builder.Services.AddTransient<ConflictResolutionViewModel>();
            
            builder.Services.AddTransient<ReportsViewModel>();
            builder.Services.AddTransient<MainPageViewModel>();

            // Register Services
            builder.Services.AddTransient<IMonitoringService, MonitoringService>();
            builder.Services.AddTransient<IMonitoringRepository, MonitoringRepository>();

            builder.Services.AddTransient<IPatientService, PatientService>();
            builder.Services.AddTransient<IPatientRepository, PatientRepository>();

            builder.Services.AddTransient<IPdfReportService, PdfReportService>();

            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();

            builder.Services.AddTransient<IPharmacyCardService, PharmacyCardService>();
            builder.Services.AddTransient<IPharmacyCardRepository, PharmacyCardRepository>();

            builder.Services.AddTransient<IPharmacyService, PharmacyService>();
            builder.Services.AddTransient<IPharmacyRepository, PharmacyRepository>();

            builder.Services.AddTransient<IPrescriptionRepository, PrescriptionRepository>();
            builder.Services.AddTransient<IPrescriptionService, PrescriptionService>();

            builder.Services.AddTransient<IPrescriptionMedicationRepository, PrescriptionMedicationRepository>();
            builder.Services.AddTransient<IPrescriptionMedicationService, PrescriptionMedicationService>();

            builder.Services.AddTransient<IAdministrationModeRepository, AdministrationModeRepository>();
            builder.Services.AddTransient<IAdministrationModeService, AdministrationModeService>();

            builder.Services.AddTransient<IImportConfigurationRepository, ImportConfigurationRepository>();
            builder.Services.AddTransient<IImportConfigurationService, ImportConfigurationService>();

            builder.Services.AddTransient<IEmailConfigurationService, EmailConfigurationService>();

            builder.Services.AddTransient<IMedicationService, MedicationService>();
            builder.Services.AddTransient<IMedicationImportService, MedicationImportService>();
            builder.Services.AddTransient<ICsvFileParser, CsvFileParser>();
            builder.Services.AddTransient<IMedicationRepository, MedicationRepository>();

            builder.Services.AddTransient<ISecureStorageService, MauiSecureStorageService>();
            
            // AuthenticationService holds user state, so it must be Singleton.
            // Note: This causes a captive dependency on IUserRepository (Transient).
            // This means the UserRepository and its DbContext will live as long as the app.
            // Since this is only used for Login/Logout which are infrequent, this is acceptable,
            // but ideally AuthenticationService should use IServiceScopeFactory for its dependencies.
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();

            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<LoginViewModel>();

            builder.Services.AddTransient<LoginAddUserView>();
            builder.Services.AddTransient<LoginAddUserViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

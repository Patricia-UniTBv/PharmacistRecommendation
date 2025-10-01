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
            builder.Services.AddDbContext<PharmacistRecommendationDbContext>(options =>
                options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=PharmacistRecommendationDB;Trusted_Connection=true;TrustServerCertificate=true;"));

            builder.Services.AddTransient<MonitoringView>();
            builder.Services.AddTransient<UsersManagementView>();
            builder.Services.AddTransient<CardConfigurationView>();
            builder.Services.AddTransient<GdprConfigurationView>();
            builder.Services.AddTransient<MixedActIssuanceView>();
            builder.Services.AddTransient<AdministrationModesView>();
            builder.Services.AddTransient<ImportConfigurationView>();
            builder.Services.AddTransient<AddPharmacyView>();
            builder.Services.AddTransient<EmailConfigurationView>();
            
            builder.Services.AddTransient<MedicationView>();
            builder.Services.AddTransient<AddEditMedicationView>();
            builder.Services.AddTransient<ConflictResolutionView>();

            builder.Services.AddTransient<ReportsView>();

            builder.Services.AddTransient<MainPageView>();

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

            builder.Services.AddTransient<MedicationViewModel>();
            builder.Services.AddTransient<AddEditMedicationViewModel>();
            builder.Services.AddTransient<ConflictResolutionViewModel>();
            
            builder.Services.AddTransient<ReportsViewModel>();
            
            builder.Services.AddTransient<MainPageViewModel>();

            builder.Services.AddSingleton<IMonitoringService, MonitoringService>();
            builder.Services.AddSingleton<IMonitoringRepository, MonitoringRepository>();

            builder.Services.AddSingleton<IPatientService, PatientService>();
            builder.Services.AddSingleton<IPatientRepository, PatientRepository>();

            builder.Services.AddSingleton<IPdfReportService, PdfReportService>();

            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IUserRepository, UserRepository>();

            builder.Services.AddSingleton<IPharmacyCardService, PharmacyCardService>();
            builder.Services.AddSingleton<IPharmacyCardRepository, PharmacyCardRepository>();

            builder.Services.AddSingleton<IPharmacyService, PharmacyService>();
            builder.Services.AddSingleton<IPharmacyRepository, PharmacyRepository>();

            builder.Services.AddSingleton<IPrescriptionRepository, PrescriptionRepository>();
            builder.Services.AddSingleton<IPrescriptionService, PrescriptionService>();

            builder.Services.AddSingleton<IPrescriptionMedicationRepository, PrescriptionMedicationRepository>();
            builder.Services.AddSingleton<IPrescriptionMedicationService, PrescriptionMedicationService>();

            builder.Services.AddSingleton<IAdministrationModeRepository, AdministrationModeRepository>();
            builder.Services.AddSingleton<IAdministrationModeService, AdministrationModeService>();

            builder.Services.AddSingleton<IImportConfigurationRepository, ImportConfigurationRepository>();
            builder.Services.AddSingleton<IImportConfigurationService, ImportConfigurationService>();

            builder.Services.AddSingleton<IEmailConfigurationService, EmailConfigurationService>();

            builder.Services.AddScoped<IMedicationService, MedicationService>();
            builder.Services.AddScoped<IMedicationImportService, MedicationImportService>();
            builder.Services.AddScoped<ICsvFileParser, CsvFileParser>();
            builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();

            builder.Services.AddSingleton<ISecureStorageService, MauiSecureStorageService>();

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

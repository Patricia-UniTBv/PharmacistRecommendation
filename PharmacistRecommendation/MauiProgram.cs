using Entities.Data;
using Entities.Repository;
using Entities.Repository.Interfaces;
using Entities.Services;
using Entities.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmacistRecommendation.ViewModels;
using PharmacistRecommendation.Views;

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

            // Configure DbContext with connection string
            builder.Services.AddDbContext<PharmacistRecommendationDbContext>(options =>
                options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=PharmacistRecommendationDB;Trusted_Connection=true;TrustServerCertificate=true;"));

            // Register pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MedicationView>();
            builder.Services.AddTransient<CardioMonitoringView>();
            builder.Services.AddTransient<TestConnectionView>();
            builder.Services.AddTransient<ConflictResolutionView>();
            builder.Services.AddTransient<AddEditMedicationView>();

            // Register ViewModels
            builder.Services.AddTransient<MedicationViewModel>();
            builder.Services.AddTransient<CardioMonitoringViewModel>();
            builder.Services.AddTransient<ConflictResolutionViewModel>();
            builder.Services.AddTransient<AddEditMedicationViewModel>();

            // Register Services
            builder.Services.AddScoped<IMedicationService, MedicationService>();
            builder.Services.AddScoped<IMedicationImportService, MedicationImportService>();
            builder.Services.AddScoped<ICsvFileParser, CsvFileParser>();
            builder.Services.AddSingleton<ICardioMonitoringService, CardioMonitoringService>();
            builder.Services.AddSingleton<IMonitoringService, MonitoringService>();
            builder.Services.AddSingleton<IPatientService, PatientService>();

            // Register Repositories
            builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();
            builder.Services.AddScoped<ICardioMonitoringRepository, CardioMonitoringRepository>();
            builder.Services.AddSingleton<IMonitoringRepository, MonitoringRepository>();
            builder.Services.AddSingleton<IPatientRepository, PatientRepository>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
using Entities.Services.Interfaces;
using Entities.Services;
using Microsoft.Extensions.Logging;
using PharmacistRecommendation.ViewModels;
using PharmacistRecommendation.Views;
using Entities.Repository.Interfaces;
using Entities.Repository;
using Entities.Data;
using Microsoft.EntityFrameworkCore;

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
            // Add MainPage registration
            builder.Services.AddTransient<MainPage>();

            builder.Services.AddTransient<CardioMonitoringView>();
            builder.Services.AddTransient<CardioMonitoringViewModel>();
            builder.Services.AddSingleton<ICardioMonitoringService, CardioMonitoringService>();
            builder.Services.AddSingleton<IMonitoringService, MonitoringService>();
            builder.Services.AddSingleton<IMonitoringRepository, MonitoringRepository>();
            builder.Services.AddSingleton<IPatientService, PatientService>();
            builder.Services.AddSingleton<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<ICardioMonitoringRepository, CardioMonitoringRepository>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
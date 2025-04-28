using Entities.Services.Interfaces;
using Entities.Services;
using Microsoft.Extensions.Logging;
using PharmacistRecommendation.ViewModels;
using PharmacistRecommendation.Views;
using Entities.Repository.Interfaces;
using Entities.Repository;
using Entities.Data;

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
            builder.Services.AddTransient<CardioMonitoringView>();
            builder.Services.AddTransient<CardioMonitoringViewModel>();
            builder.Services.AddSingleton<IMonitoringService, MonitoringService>();
            builder.Services.AddSingleton<IMonitoringRepository, MonitoringRepository>();
            builder.Services.AddSingleton<PharmacistRecommendationDbContext>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

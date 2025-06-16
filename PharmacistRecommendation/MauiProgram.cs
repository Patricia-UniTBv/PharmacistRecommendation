using Entities.Services.Interfaces;
using Entities.Services;
using Microsoft.Extensions.Logging;
using PharmacistRecommendation.ViewModels;
using PharmacistRecommendation.Views;
using Entities.Repository.Interfaces;
using Entities.Repository;
using Entities.Data;
using PharmacistRecommendation.Helpers;
using CommunityToolkit.Maui;

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
            builder.UseMauiCommunityToolkit();
            builder.Services.AddSingleton<PharmacistRecommendationDbContext>();

            builder.Services.AddTransient<MonitoringView>();
            builder.Services.AddTransient<UsersManagementView>();

            builder.Services.AddTransient<MonitoringViewModel>();
            builder.Services.AddTransient<PharmacistConfigurationViewModel>();
            builder.Services.AddTransient<UsersManagementViewModel>();

            builder.Services.AddSingleton<IMonitoringService, MonitoringService>();
            builder.Services.AddSingleton<IMonitoringRepository, MonitoringRepository>();

            builder.Services.AddSingleton<IPatientService, PatientService>();
            builder.Services.AddSingleton<IPatientRepository, PatientRepository>();

            builder.Services.AddSingleton<IPdfReportService, PdfReportService>();
            builder.Services.AddSingleton<IEmailService, EmailService>();

            builder.Services.AddSingleton<IPharmacistService, PharmacistService>(); // de sters
            builder.Services.AddSingleton<IPharmacistRepository, PharmacistRepository>(); // de sters

            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IUserRepository, UserRepository>();

            builder.Services.AddScoped<IMonitoringRepository, MonitoringRepository>();



#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

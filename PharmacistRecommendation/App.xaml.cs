using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Views;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace PharmacistRecommendation
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            Current.UserAppTheme = AppTheme.Light;

            // Get MainPage from dependency injection
            MainPage = serviceProvider.GetRequiredService<MainPage>();
        }
    }
}
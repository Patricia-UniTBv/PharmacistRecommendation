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

            // Get MainPage from dependency injection and wrap it in NavigationPage
            var mainPage = serviceProvider.GetRequiredService<MainPage>();
            MainPage = new NavigationPage(mainPage);
        }
    }
}
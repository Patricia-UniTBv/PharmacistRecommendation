using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Views;

namespace PharmacistRecommendation
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Current.UserAppTheme = AppTheme.Light;
            
            // Always start with AppShell - this will show MainPageView by default
            MainPage = new AppShell();
        }

        public static async Task NavigateToMainShell()
        {
            // Already on shell, just navigate back to main content
            if (Current.MainPage is AppShell shell)
            {
                await shell.GoToAsync("..");
            }
            else
            {
                Current.MainPage = new AppShell();
            }
        }

        public static async Task NavigateToLogin()
        {
            if (Current.MainPage is AppShell shell)
            {
                await shell.GoToAsync("login");
            }
            else
            {
                Current.MainPage = new AppShell();
                if (Current.MainPage is AppShell newShell)
                {
                    await newShell.GoToAsync("login");
                }
            }
        }
    }
}

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
            Current!.UserAppTheme = AppTheme.Light;
            
            MainPage = new AppShell();
        }

        public static async Task NavigateToMainShell()
        {
            if (Current!.MainPage is AppShell shell)
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
            if (Current!.MainPage is AppShell shell)
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

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

#if WINDOWS
            window.MaximumWidth = double.PositiveInfinity;
            window.MaximumHeight = double.PositiveInfinity;
            window.Width = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Width;
            window.Height = Microsoft.Maui.Devices.DeviceDisplay.MainDisplayInfo.Height;
#endif

            return window;
        }

    }
}

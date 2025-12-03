using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Views;
using System.Diagnostics;

namespace PharmacistRecommendation
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Current!.UserAppTheme = AppTheme.Light;
           
            // Initialize application with first-run check
            InitializeApplicationAsync();
         }

        private async void InitializeApplicationAsync()
        {
         try
                {
        Debug.WriteLine("Starting application initialization...");
            
          // Check if this is the first run
           bool isFirstRun = await FirstRunHelper.IsFirstRunAsync();
             
    if (isFirstRun)
          {
      Debug.WriteLine("First run detected. Showing setup wizard...");
             
          // Get deployment mode to determine which wizard to show
             string deploymentMode = FirstRunHelper.GetDeploymentMode();
               Debug.WriteLine($"Deployment mode: {deploymentMode}");
            
         if (deploymentMode == "Server")
                {
          // Show Server Setup Wizard
           var serverWizard = ServiceHelper.GetService<ServerSetupWizard>();
            MainPage = new NavigationPage(serverWizard);
         Debug.WriteLine("Showing Server Setup Wizard");
           }
            else // Client mode
            {
      // Show Client Setup Wizard
     var clientWizard = ServiceHelper.GetService<ClientSetupWizard>();
                MainPage = new NavigationPage(clientWizard);
                Debug.WriteLine("Showing Client Setup Wizard");
             }
              }
         else
             {
        // Normal startup - configuration is complete
          Debug.WriteLine("Application already configured. Starting normally...");
        MainPage = new AppShell();
        }
         }
         catch (Exception ex)
              {
        Debug.WriteLine($"Error during application initialization: {ex.Message}");
      Debug.WriteLine($"Stack trace: {ex.StackTrace}");
           
     // Fallback to normal shell in case of error
         MainPage = new AppShell();
                }
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

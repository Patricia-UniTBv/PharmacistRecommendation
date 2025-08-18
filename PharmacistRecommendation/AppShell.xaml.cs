using CommunityToolkit.Maui.Views;
using DTO;
using Entities.Services.Interfaces;
using Entities.Models; // Add this using directive for Prescription
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.ViewModels;
using PharmacistRecommendation.Views;

namespace PharmacistRecommendation
{
    public partial class AppShell : Shell
    {
        private readonly IAuthenticationService _authService;

        public AppShell()
        {
            InitializeComponent();
            
            // Get authentication service
            _authService = ServiceHelper.GetService<IAuthenticationService>();
            
            // Register routes
            Routing.RegisterRoute("login", typeof(LoginView));
            Routing.RegisterRoute("monitoring", typeof(MonitoringView));
            Routing.RegisterRoute("users_management", typeof(UsersManagementView));
            Routing.RegisterRoute("new_card", typeof(CardConfigurationView));
            Routing.RegisterRoute("gdpr_configuration", typeof(GdprConfigurationView));
            Routing.RegisterRoute("mixed_issuance", typeof(MixedActIssuanceView));
            Routing.RegisterRoute("administration_modes", typeof(AdministrationModesView));
            Routing.RegisterRoute("import_configuration", typeof(ImportConfigurationView));
            Routing.RegisterRoute("medications", typeof(MedicationView));
            Routing.RegisterRoute("test_main", typeof(MainPageView));
            Routing.RegisterRoute("reports", typeof(ReportsView));

            // Subscribe to authentication changes
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        private async void OnAuthenticationStateChanged(object sender, Entities.Services.Interfaces.AuthResult e)
        {
            try
            {
                if (e.IsSuccess)
                {
                    // User logged in successfully - stay on current page or navigate as needed
                    // You could navigate to test main page here if desired
                    // await GoToAsync("test_main");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private async void OnNewCardClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("new_card");
        }

        private async void OnMonitClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("monitoring");
        }

        private async void OnMixedIssuanceClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("mixed_issuance?mode=mixed");
        }

        private async void OnPrescriptionOnlyClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("mixed_issuance?mode=withprescription");
        }

        private async void OnWithoutPrescriptionClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("mixed_issuance?mode=withoutprescription");
        }

        // Keep only one version of OnTestMainPageClicked
        private async void OnTestMainPageClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("test_main");
        }

        // Reports navigation methods - Fixed routing
        private async void OnReportsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("reports");
        }

        private async void OnMixedActsReportClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("reports?type=mixed");
        }

        private async void OnOwnActsReportClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("reports?type=own");
        }

        private async void OnConsecutiveActsReportClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("reports?type=consecutive");
        }

        private async void OnMonitoringListReportClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("reports?type=monitoring");
        }

        private async void OnAddPharmacistClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//PharmacistConfigurationView");
        }

        private async void OnUsersManagementClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("users_management");
        }

        private async void OnGdprConfigClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("gdpr_configuration");
        }

        private async void OnAdministrationModesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("administration_modes");
        }

        private async void OnImportConfigClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("import_configuration");
        }

        private async void OnMedicationsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("medications");
        }

        private async Task<bool> CheckAuthenticationOrPrompt()
        {
            try
            {
                var isAuthenticated = await _authService.IsAuthenticatedAsync();
                if (!isAuthenticated)
                {
                    await DisplayAlert("Autentificare Necesară", 
                        "Trebuie să vă autentificați pentru a accesa această funcționalitate.", 
                        "OK");
                    return false;
                }
                return true;
            }
            catch
            {
                await DisplayAlert("Eroare", 
                    "A apărut o eroare la verificarea autentificării.", 
                    "OK");
                return false;
            }
        }
    }
}

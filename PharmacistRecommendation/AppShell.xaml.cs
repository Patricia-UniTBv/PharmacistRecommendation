using CommunityToolkit.Maui.Views;
using DTO;
using Entities.Services.Interfaces;
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
            Routing.RegisterRoute("email_configuration", typeof(EmailConfigurationView));
            Routing.RegisterRoute("add_pharmacy", typeof(AddPharmacyView));
            Routing.RegisterRoute("test_main", typeof(MainPageView)); // Add route for test main page

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

        // New handler for Test menu
        private async void OnTestMainPageClicked(object sender, EventArgs e)
        {
            await GoToAsync("test_main");
        }

        private async void OnNewCardClicked(object sender, EventArgs e)
        {
            if (await CheckAuthenticationOrPrompt())
                await GoToAsync("new_card");
        }

        private async void OnMonitClicked(object sender, EventArgs e)
        {
            if (await CheckAuthenticationOrPrompt())
                await GoToAsync("monitoring");
        }

        private async void OnMixedIssuanceClicked(object sender, EventArgs e)
        {
            if (await CheckAuthenticationOrPrompt())
                await GoToAsync("mixed_issuance?mode=mixed");
        }

        private async void OnPrescriptionOnlyClicked(object sender, EventArgs e)
        {
            if (await CheckAuthenticationOrPrompt())
                await Shell.Current.GoToAsync("mixed_issuance?mode=withprescription");
        }

        private async void OnWithoutPrescriptionClicked(object sender, EventArgs e)
        {
            if (await CheckAuthenticationOrPrompt())
                await Shell.Current.GoToAsync("mixed_issuance?mode=withoutprescription");
        }

        private async void OnAddPharmacistClicked(object sender, EventArgs e)
        {
            // Allow adding users without authentication (for initial setup)
            var vm = ServiceHelper.GetService<PharmacistConfigurationViewModel>();
            var popup = new PharmacistConfigurationView(vm);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is null) return;
        }

        private async void OnUsersManagementClicked(object sender, EventArgs e)
        {
            // Allow access to user management without authentication (for initial setup)
            await GoToAsync("users_management");
        }

        private async void OnGdprConfigClicked(object sender, EventArgs e)
        {
            if (await CheckAuthenticationOrPrompt())
                await GoToAsync("gdpr_configuration");
        }

        private async void OnAdministrationModesClicked(object sender, EventArgs e)
        {
            if (await CheckAuthenticationOrPrompt())
                await GoToAsync("administration_modes");
        }

        private async void OnImportConfigClicked(object sender, EventArgs e)
        {
            if (await CheckAuthenticationOrPrompt())
                await GoToAsync("import_configuration");
        }

        private async void OnMedicationsClicked(object sender, EventArgs e)
        {
            if (await CheckAuthenticationOrPrompt())
                await GoToAsync("medications");
        }
        private async void OnEmailConfigClicked(object sender, EventArgs e)
        {
            await GoToAsync("email_configuration");
        }

        private async void OnAddPharmacyClicked(object sender, EventArgs e)
        {
            await GoToAsync("add_pharmacy");
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

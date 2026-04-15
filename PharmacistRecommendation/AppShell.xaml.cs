using CommunityToolkit.Maui.Views;
using DTO;
using Entities.Services.Interfaces;
using Entities.Models;
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
            Routing.RegisterRoute("email_configuration", typeof(EmailConfigurationView));
            Routing.RegisterRoute("add_pharmacy", typeof(AddPharmacyView));
            Routing.RegisterRoute("server_configuration", typeof(ServerConfigurationView));
            Routing.RegisterRoute("admin_dashboard", typeof(AdminDashboardView));

            Routing.RegisterRoute(nameof(AddPharmacyView), typeof(AddPharmacyView));
            Routing.RegisterRoute(nameof(LoginAddUserView), typeof(LoginAddUserView));
            Routing.RegisterRoute(nameof(MixedActIssuanceView),typeof(MixedActIssuanceView));
            Routing.RegisterRoute(nameof(MonitoringView), typeof(MonitoringView));

            // Subscribe to authentication changes
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;

            // Attach WinUI3 flyouts to dropdown buttons
            AttachDropdownMenus();
        }

        private void AttachDropdownMenus()
        {
#if WINDOWS
            BtnMonitorizare.Loaded += (_, _) => AttachFlyout(BtnMonitorizare,
                ("Monitorizare pacient",              (EventHandler)OnMonitClicked),
                ("Listă Monitorizări",                (EventHandler)OnMonitoringListReportClicked));

            BtnEmitere.Loaded += (_, _) => AttachFlyout(BtnEmitere,
                ("Emitere act consecutiv prescripției", (EventHandler)OnPrescriptionOnlyClicked),
                ("Emitere act farmaceutic",              (EventHandler)OnWithoutPrescriptionClicked));

            BtnRapoarte.Loaded += (_, _) => AttachFlyout(BtnRapoarte,
                ("Raport Acte Proprii",     (EventHandler)OnOwnActsReportClicked),
                ("Raport Acte Consecutive", (EventHandler)OnConsecutiveActsReportClicked),
                ("Centru Rapoarte",         (EventHandler)OnReportsClicked));

            BtnConfigurari.Loaded += (_, _) => AttachFlyout(BtnConfigurari,
                ("Medicamente",                  (EventHandler)OnMedicationsClicked),
                ("Moduri administrare",          (EventHandler)OnAdministrationModesClicked),
                ("Configurare email",            (EventHandler)OnEmailConfigClicked),
                ("Configurare importuri",        (EventHandler)OnImportConfigClicked),
                ("Document GDPR",                (EventHandler)OnGdprConfigClicked),
                ("Configurare conexiune server", (EventHandler)OnServerConfigClicked));
#endif
        }

#if WINDOWS
        private static void AttachFlyout(Button button, params (string text, EventHandler handler)[] items)
        {
            if (button.Handler?.PlatformView is not Microsoft.UI.Xaml.Controls.Button nativeBtn)
                return;
            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout();
            foreach (var (text, handler) in items)
            {
                var item = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = text };
                item.Click += (_, _) => handler(null, EventArgs.Empty);
                flyout.Items.Add(item);
            }
            nativeBtn.Flyout = flyout;
        }
#endif

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

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);
            TitleBar.IsVisible = !IsAdmin();
        }

        private bool IsAdmin() => SessionManager.CurrentUser?.Role?.ToLower() == "admin";

        private async void OnNewCardClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("new_card"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnMonitClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("monitoring"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnMixedIssuanceClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("mixed_issuance?mode=mixed"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnPrescriptionOnlyClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("mixed_issuance?mode=withprescription"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnWithoutPrescriptionClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("mixed_issuance?mode=withoutprescription"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnTestMainPageClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("test_main"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnReportsClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("reports"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnMixedActsReportClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("reports?type=mixed"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnOwnActsReportClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("reports?type=own"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnConsecutiveActsReportClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("reports?type=consecutive"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnMonitoringListReportClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("reports?type=monitoring"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        //private async void OnAddPharmacistClicked(object sender, EventArgs e)
        //{
        //    await Shell.Current.GoToAsync("//PharmacistConfigurationView");
        //}

        private async void OnUsersManagementClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("users_management"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnGdprConfigClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("gdpr_configuration"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnAdministrationModesClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("administration_modes"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnImportConfigClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("import_configuration"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnMedicationsClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await Shell.Current.GoToAsync("medications"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnEmailConfigClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await GoToAsync("email_configuration"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnAddPharmacyClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await GoToAsync("add_pharmacy"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnServerConfigClicked(object sender, EventArgs e)
        {
            try { if (IsAdmin()) return; await GoToAsync("server_configuration"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Nav error: {ex.Message}"); }
        }

        private async void OnNavigateBackClicked(object sender, EventArgs e)
        {
            try
            {
                if (Shell.Current.Navigation.NavigationStack.Count >= 1)
                    await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Back navigation error: {ex.Message}");
            }
        }

        private void OnCloseAppClicked(object sender, EventArgs e)
        {
            try
            {
                // Dispose the DI service provider — closes all DbContext connections
                // and releases any held SQL Server ports/resources cleanly
                if (Application.Current?.Handler?.MauiContext?.Services is IDisposable services)
                    services.Dispose();
            }
            catch { /* best-effort */ }

            Application.Current?.Quit();

            // Force-exit the process so the OS reclaims all network ports immediately,
            // preventing dynamic-port conflicts with the database on next launch
            Environment.Exit(0);
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

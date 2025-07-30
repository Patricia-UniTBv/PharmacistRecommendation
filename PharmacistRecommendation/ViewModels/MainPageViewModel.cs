using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Views;

namespace PharmacistRecommendation.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            // Initialize commands
            NavigateToMedicationsCommand = new Command(async () => await NavigateToMedicationsAsync());
            NavigateToNewCardCommand = new Command(async () => await NavigateToNewCardAsync());
            NavigateToMonitoringCommand = new Command(async () => await NavigateToMonitoringAsync());
            NavigateToMixedIssuanceCommand = new Command(async () => await NavigateToMixedIssuanceAsync());
            NavigateToPrescriptionOnlyCommand = new Command(async () => await NavigateToPrescriptionOnlyAsync());
            NavigateToWithoutPrescriptionCommand = new Command(async () => await NavigateToWithoutPrescriptionAsync());
            NavigateToUsersManagementCommand = new Command(async () => await NavigateToUsersManagementAsync());
            NavigateToAdministrationModesCommand = new Command(async () => await NavigateToAdministrationModesAsync());
            NavigateToImportConfigCommand = new Command(async () => await NavigateToImportConfigAsync());
            NavigateToGdprConfigCommand = new Command(async () => await NavigateToGdprConfigAsync());
            NavigateToAddPharmacistCommand = new Command(async () => await NavigateToAddPharmacistAsync());
            NavigateToReportsCommand = new Command(async () => await NavigateToReportsAsync());
        }

        public ICommand NavigateToMedicationsCommand { get; }
        public ICommand NavigateToNewCardCommand { get; }
        public ICommand NavigateToMonitoringCommand { get; }
        public ICommand NavigateToMixedIssuanceCommand { get; }
        public ICommand NavigateToPrescriptionOnlyCommand { get; }
        public ICommand NavigateToWithoutPrescriptionCommand { get; }
        public ICommand NavigateToUsersManagementCommand { get; }
        public ICommand NavigateToAdministrationModesCommand { get; }
        public ICommand NavigateToImportConfigCommand { get; }
        public ICommand NavigateToGdprConfigCommand { get; }
        public ICommand NavigateToAddPharmacistCommand { get; }
        public ICommand NavigateToReportsCommand { get; }

        private async Task NavigateToMedicationsAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("medications");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare Navigare", $"Nu s-a putut accesa pagina de medicamente: {ex.Message}");
            }
        }

        private async Task NavigateToNewCardAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("new_card");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare Navigare", $"Nu s-a putut accesa pagina de card nou: {ex.Message}");
            }
        }

        private async Task NavigateToMonitoringAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("monitoring");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare Navigare", $"Nu s-a putut accesa pagina de monitorizare: {ex.Message}");
            }
        }

        private async Task NavigateToMixedIssuanceAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("mixed_issuance?mode=mixed");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare Navigare", $"Nu s-a putut accesa pagina de emitere act mixt: {ex.Message}");
            }
        }

        private async Task NavigateToPrescriptionOnlyAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("mixed_issuance?mode=withprescription");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare Navigare", $"Nu s-a putut accesa pagina de emitere act cu prescripție: {ex.Message}");
            }
        }

        private async Task NavigateToWithoutPrescriptionAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("mixed_issuance?mode=withoutprescription");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare Navigare", $"Nu s-a putut accesa pagina de emitere act propriu: {ex.Message}");
            }
        }

        private async Task NavigateToUsersManagementAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("users_management");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare Navigare", $"Nu s-a putut accesa pagina de gestionare utilizatori: {ex.Message}");
            }
        }

        private async Task NavigateToAdministrationModesAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("administration_modes");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare Navigare", $"Nu s-a putut accesa pagina de moduri de administrare: {ex.Message}");
            }
        }

        private async Task NavigateToImportConfigAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("import_configuration");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare Navigare", $"Nu s-a putut accesa pagina de configurare import: {ex.Message}");
            }
        }

        private async Task NavigateToGdprConfigAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("gdpr_configuration");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare Navigare", $"Nu s-a putut accesa pagina de configurare GDPR: {ex.Message}");
            }
        }

        private async Task NavigateToAddPharmacistAsync()
        {
            try
            {
                var vm = ServiceHelper.GetService<PharmacistConfigurationViewModel>();
                var popup = new PharmacistConfigurationView(vm);
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                if (result is not null)
                {
                    // Handle the result if needed
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Eroare", $"Nu s-a putut deschide formularul de adăugare utilizator: {ex.Message}");
            }
        }

        private async Task NavigateToReportsAsync()
        {
            // For now, show a placeholder message since reports functionality might not be implemented yet
            await Application.Current.MainPage.DisplayAlert(
                "În curs de dezvoltare",
                "Funcționalitatea de rapoarte va fi disponibilă în curând.",
                "OK");
        }

        private async Task ShowErrorAsync(string title, string message)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
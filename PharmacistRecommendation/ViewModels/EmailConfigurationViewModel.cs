using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Models;
using Entities.Services;
using Entities.Services.Interfaces;
using Microsoft.Maui.Controls;
using PharmacistRecommendation.Helpers;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    public partial class EmailConfigurationViewModel : ObservableObject
    {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string appPassword;

        [ObservableProperty]
        private bool showGmailHelp;

        private readonly IEmailConfigurationService _emailService;
        private readonly int _pharmacyId;


        public EmailConfigurationViewModel(IEmailConfigurationService emailConfigurationService)
        {
            _emailService = emailConfigurationService;
            _pharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1;
            LoadEmailConfiguration();
        }

        private async void LoadEmailConfiguration()
        {
            var config = await _emailService.GetByPharmacyIdAsync(_pharmacyId);
            if (config != null)
            {
                Email = config.Username ?? string.Empty;
                AppPassword = config.Password ?? string.Empty;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(AppPassword))
            {
                await Shell.Current.DisplayAlert("Eroare", "Completează ambele câmpuri.", "OK");
                return;
            }

            var config = await _emailService.GetByPharmacyIdAsync(_pharmacyId);

            if (config == null)
            {
                config = new Entities.Models.EmailConfiguration
                {
                    PharmacyId = _pharmacyId,
                    Username = Email,
                    Password = AppPassword
                };

                await _emailService.AddAsync(config);
            }
            else
            {
                config.Username = Email;
                config.Password = AppPassword;

                await _emailService.UpdateAsync(config);
            }

            await Shell.Current.DisplayAlert("Succes", "Datele au fost salvate.", "OK");
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private void ToggleHelp()
        {
            ShowGmailHelp = !ShowGmailHelp;
        }

    }
}

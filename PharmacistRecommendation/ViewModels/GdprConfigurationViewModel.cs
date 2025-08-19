using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Services;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    public partial class GdprConfigurationViewModel : ObservableObject
    {
        private readonly IPharmacyService _pharmacyService;
        private readonly int _pharmacyId;

        [ObservableProperty]
        private string consentTemplate = string.Empty;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public GdprConfigurationViewModel(IPharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
            _pharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1;
        }

        [RelayCommand]
        public async Task LoadConsentTemplateAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                ConsentTemplate = await _pharmacyService.GetConsentTemplateAsync(_pharmacyId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la încărcarea template-ului: {ex.Message}";
                ConsentTemplate = "Nu s-a putut încărca template-ul de consimțământ.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveConsentTemplateAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                await _pharmacyService.UpdateConsentTemplateAsync(_pharmacyId, ConsentTemplate);
                await Shell.Current.DisplayAlert("Succes", "Textul de consimțământ a fost salvat!", "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la salvare: {ex.Message}";
                await Shell.Current.DisplayAlert("Eroare", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ResetConsentTemplateAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                await _pharmacyService.ResetConsentTemplateAsync(_pharmacyId);
                ConsentTemplate = await _pharmacyService.GetConsentTemplateAsync(_pharmacyId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Eroare la resetare: {ex.Message}";
                await Shell.Current.DisplayAlert("Eroare", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

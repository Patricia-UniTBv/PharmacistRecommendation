using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Services;
using Entities.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    public partial class GdprConfigurationViewModel: ObservableObject
    {
        private readonly IPharmacyService _pharmacyService;
        private readonly int _pharmacyId;

        [ObservableProperty]
        string consentTemplate;

        [ObservableProperty]
        bool isBusy = false;

        public GdprConfigurationViewModel(IPharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
            _pharmacyId = 3; // to be modified!!
        }

        [RelayCommand]
        public async Task LoadConsentTemplateAsync()
        {
            IsBusy = true;
            ConsentTemplate = await _pharmacyService.GetConsentTemplateAsync(_pharmacyId);
            IsBusy = false;
        }

        [RelayCommand]
        private async Task SaveConsentTemplateAsync()
        {
            IsBusy = true;
            await _pharmacyService.UpdateConsentTemplateAsync(_pharmacyId, ConsentTemplate);
            IsBusy = false;

            await Shell.Current.DisplayAlert("Succes", "Textul de consimțământ a fost salvat!", "OK");
        }

        [RelayCommand]
        private async Task ResetConsentTemplateAsync()
        {
            IsBusy = true;
            await _pharmacyService.ResetConsentTemplateAsync(_pharmacyId);
            ConsentTemplate = await _pharmacyService.GetConsentTemplateAsync(_pharmacyId);
            IsBusy = false;
        }
    }
}

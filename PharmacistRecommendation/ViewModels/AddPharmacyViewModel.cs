using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Models;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Views;

namespace PharmacistRecommendation.ViewModels
{
    public partial class AddPharmacyViewModel : ObservableObject
    {
        private readonly IPharmacyService _pharmacyService;

        public AddPharmacyViewModel(IPharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
        }

        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private string? address;
        [ObservableProperty] private string? cui;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? phone;
        [ObservableProperty] private string? logo;
        [ObservableProperty] private string? consentTemplate;

        [RelayCommand]
        private async Task SelectLogoAsync()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Selectează sigla farmaciei"
            });

            if (result != null)
                Logo = result.FullPath;
        }

        [RelayCommand]
        private async Task SavePharmacyAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Eroare", "Completează denumirea!", "OK");
                return;
            }

            var pharmacy = new Pharmacy
            {
                Name = Name,
                Address = Address,
                CUI = Cui,
                Email = Email,
                Phone = Phone,
                Logo = Logo
            };

            try
            {
                await _pharmacyService.AddPharmacyAsync(pharmacy);
                await Shell.Current.DisplayAlert("Succes", "Farmacia a fost adăugată!", "OK");
                await Shell.Current.GoToAsync(nameof(LoginAddUserView));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Eroare", $"Eroare la salvare: {ex.Message}", "OK");
            }
        }
    }

}

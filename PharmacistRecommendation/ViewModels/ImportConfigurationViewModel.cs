using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Models;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;


namespace PharmacistRecommendation.ViewModels
{
    public partial class ImportConfigurationViewModel : ObservableObject
    {
        [ObservableProperty] string receiptPath = string.Empty;
        [ObservableProperty] string prescriptionPath = string.Empty;
        [ObservableProperty] string message = string.Empty;
        [ObservableProperty] bool isLoaded = true;

        public ImportConfigurationViewModel()
        {
            LoadConfiguration();
        }

        [RelayCommand]
        private void LoadConfiguration()
        {
            ReceiptPath = Preferences.Get(nameof(ReceiptPath), string.Empty);
            PrescriptionPath = Preferences.Get(nameof(PrescriptionPath), string.Empty);
            IsLoaded = true;
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                Preferences.Set(nameof(ReceiptPath), ReceiptPath ?? string.Empty);
                Preferences.Set(nameof(PrescriptionPath), PrescriptionPath ?? string.Empty);

                App.Current.MainPage.DisplayAlert("Succes", "Configurarea a fost salvată!", "OK");
            }
            catch (Exception ex)
            {
                Message = "Eroare: " + ex.Message;
            }
        }
    }
}

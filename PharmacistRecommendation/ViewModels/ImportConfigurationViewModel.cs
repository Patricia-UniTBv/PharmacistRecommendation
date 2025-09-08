using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Models;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;


namespace PharmacistRecommendation.ViewModels
{
    public partial class ImportConfigurationViewModel : ObservableObject
    {
        private readonly IImportConfigurationService _service;

        [ObservableProperty] string receiptPath;
        [ObservableProperty] string prescriptionPath;
        [ObservableProperty] string message;
        [ObservableProperty] bool isLoaded;
        private int pharmacyId;

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

                Message = "Configurarea a fost salvată!";
            }
            catch (Exception ex)
            {
                Message = "Eroare: " + ex.Message;
            }
        }
    }
}

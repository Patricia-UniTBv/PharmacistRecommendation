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

        public ImportConfigurationViewModel(IImportConfigurationService service)
        {
            pharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1;
            _service = service;
            LoadConfigurationAsync();
        }

        public IAsyncRelayCommand LoadConfigurationCommand { get; }

        private async Task LoadConfigurationAsync()
        {
            var config = await _service.GetAsync();
            if (config != null)
            {
                ReceiptPath = config.ReceiptPath;
                PrescriptionPath = config.PrescriptionPath;
            }
            IsLoaded = true;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                var existing = await _service.GetAsync();
                if (existing == null)
                {
                    await _service.AddAsync(new ImportConfiguration
                    {
                        ReceiptPath = ReceiptPath,
                        PrescriptionPath = PrescriptionPath,
                        PharmacyId = pharmacyId
                    }); 
                    Message = "Configurarea a fost salvată!";
                }
                else
                {
                    existing.ReceiptPath = ReceiptPath;
                    existing.PrescriptionPath = PrescriptionPath;
                    await _service.UpdateAsync(existing);
                    Message = "Configurarea a fost actualizată!";
                }
            }
            catch (Exception ex)
            {
                Message = "Eroare: " + ex.Message;
            }
        }
    }
}

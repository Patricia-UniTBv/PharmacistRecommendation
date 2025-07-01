using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;

namespace PharmacistRecommendation.ViewModels
{
    public partial class CardConfigurationViewModel : ObservableObject
    {
        private readonly IPharmacyCardService _pharmacyCardService;
        [ObservableProperty] private string cardNumber;
        [ObservableProperty] private string firstName;
        [ObservableProperty] private string lastName;
        [ObservableProperty] private string cnp;
        [ObservableProperty] private string cid;
        [ObservableProperty] private string phone;
        [ObservableProperty] private string email;
        [ObservableProperty] private string gender = null;
        [ObservableProperty] private DateOnly birthdate;
        
        public List<string> GenderOptions { get; } = new() { "Feminin", "Masculin", "Altul" };

        public event Action<PharmacyCardDTO?>? CloseRequested;

        public CardConfigurationViewModel(IPharmacyCardService pharmacyCardService)
        {
            _pharmacyCardService = pharmacyCardService;
        }

        [RelayCommand]
        private async Task GenerateCid()
        {
            if (string.IsNullOrWhiteSpace(Cnp) || Cnp.Length != 13)
            {
                await Shell.Current.DisplayAlert("Eroare", "Introduceți un CNP valid (13 caractere).", "OK");
                return;
            }

            try
            {
                var cid = CidGen.GetCidHash(Cnp); 

                if (!string.IsNullOrEmpty(cid))
                    Cid = cid;
                else
                    await Shell.Current.DisplayAlert("Eroare", "Nu s-a putut genera CID-ul.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Eroare neașteptată", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                await Shell.Current.DisplayAlert("Eroare", "Introduceți datele obligatorii (nume, prenume, număr card).", "OK");
                return;
            }
            try
            {
                var card = await _pharmacyCardService.CreateCardAsync(
                    code: cardNumber,
                    pharmacyId: 1, // to be modified with the actual id!!!
                    firstName: FirstName,
                    lastName: LastName,
                    cnp: Cnp,
                    cid: Cid,
                    email: Email,
                    phone: Phone, 
                    gender: Gender,
                    birthdate: birthdate
                );

                await Shell.Current.DisplayAlert("Succes", "Cardul pacientului a fost salvat!", "OK");
                CloseRequested?.Invoke(null);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Eroare", $"A apărut o eroare la salvare: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            CloseRequested?.Invoke(null);
        }
    }
}

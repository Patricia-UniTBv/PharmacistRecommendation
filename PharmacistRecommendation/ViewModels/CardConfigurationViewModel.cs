using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    public partial class CardConfigurationViewModel : ObservableObject
    {
        private readonly IPharmacyCardService _pharmacyCardService;
        [ObservableProperty] private string cardNumber;
        [ObservableProperty] private string firstName;
        [ObservableProperty] private string lastName;
        [ObservableProperty] private string CNP;
        [ObservableProperty] private string CID;
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
        private void GenerateCid()
        {
            //if (!string.IsNullOrWhiteSpace(Cnp) && Cnp.Length == 13)
            //{
            //    Cid = "CID" + Cnp.Substring(Cnp.Length - 6); // Poți înlocui cu un generator CNASS real
            //}
            //else
            //{
            //    Shell.Current.DisplayAlert("Eroare", "Introduceți un CNP valid (13 caractere).", "OK");
            //}
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
                    cnp: CNP,
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

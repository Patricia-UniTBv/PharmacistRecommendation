using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    public partial class PharmacistConfigurationViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        public event Action<UserDTO?>? CloseRequested;

        public PharmacistConfigurationViewModel(IUserService userService)
        {
            _userService = userService;
        }

        [ObservableProperty] private string lastName = string.Empty;
        [ObservableProperty] private string firstName = string.Empty;
        [ObservableProperty] private string? ncm;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? phoneNr;
        [ObservableProperty] private string? username;
        [ObservableProperty] private string? password;
        [ObservableProperty] private bool isPassword = true;


        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName))
            {
                await Shell.Current.DisplayAlert("Eroare", "Completaţi câmpurile Nume şi Prenume!", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email.Trim(),
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                await Shell.Current.DisplayAlert("Eroare", "Introduceți un email valid!", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || !Regex.IsMatch(Password,
                    @"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$"))
            {
                await Shell.Current.DisplayAlert("Eroare",
                    "Parola trebuie să aibă cel puțin 6 caractere, o literă mare, o cifră și un caracter special.",
                    "OK");
                return;
            }

            var dto = new UserDTO
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Ncm = Ncm?.Trim(),
                Email = Email?.Trim(),
                Phone = PhoneNr?.Trim(),
                Username = Username?.Trim(),
                Password = Password, 
                Role = "Pharmacist",
                PharmacyId = 1 // to be replaced
            };
            try
            {
                int newId = await _userService.AddUserAsync(dto);
                await Shell.Current.DisplayAlert("Succes", "Farmacist adăugat cu succes! " + newId, "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Eroare", $"Eroare la salvare: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
            CloseRequested?.Invoke(null);
        }

    }
}


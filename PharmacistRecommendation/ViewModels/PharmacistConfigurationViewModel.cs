using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
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
        public enum UserFormMode { Add, Edit }

        public UserFormMode Mode { get; private set; }


        public PharmacistConfigurationViewModel(IUserService userService)
        {
            _userService = userService;
            Mode = UserFormMode.Add;
        }

        [ObservableProperty] private string lastName = string.Empty;
        [ObservableProperty] private string firstName = string.Empty;
        [ObservableProperty] private string? ncm;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? phoneNr;
        [ObservableProperty] private string? username;
        [ObservableProperty] private string? role = "Pharmacist";
        [ObservableProperty] private bool isAssistant;
        [ObservableProperty] private string? password;
        [ObservableProperty] private bool isPassword = true;
        private int Id { get; set; } = 0;

        partial void OnIsAssistantChanged(bool value)
        {
            Role = value ? "Assistant" : "Pharmacist";
        }

        partial void OnFirstNameChanged(string? value)
        {
            UpdateUsername();
        }

        partial void OnLastNameChanged(string? value)
        {
            UpdateUsername();
        }

        private void UpdateUsername()
        {
            if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
            {
                Username = $"{LastName.Trim().ToLower()}.{FirstName.Trim().ToLower()}";
            }
        }

        public void LoadFrom(UserDTO dto)
        {
            Id = dto.Id;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            Email = dto.Email;
            PhoneNr = dto.Phone;
            Username = dto.Username;
            Ncm = dto.Ncm;

            Mode = UserFormMode.Edit;
        }

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

            if (Mode == UserFormMode.Add || !string.IsNullOrWhiteSpace(Password))
            {
                if (string.IsNullOrWhiteSpace(Password) || !Regex.IsMatch(Password,
                        @"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$"))
                {
                    await Shell.Current.DisplayAlert("Eroare",
                        "Parola trebuie să aibă cel puțin 6 caractere, o literă mare, o cifră și un caracter special.",
                        "OK");
                    return;
                }
            }

            var dto = new UserDTO
            {
                Id = Id,
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Ncm = Ncm?.Trim(),
                Email = Email?.Trim(),
                Phone = PhoneNr?.Trim(),
                Username = Username?.Trim(),
                Password = Password, 
                Role = Role,
                PharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1
        };
            try
            {
                if (Mode == UserFormMode.Edit)
                {
                    await _userService.UpdateUserAsync(dto);
                    await Shell.Current.DisplayAlert("Succes", "Datele au fost actualizate cu succes! ", "OK");
                }
                else
                {
                    int newId = await _userService.AddUserAsync(dto);
                    await Shell.Current.DisplayAlert("Succes", "Utilizator adăugat cu succes! ", "OK");
                }

              
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Eroare", $"Eroare la salvare: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        private async Task CancelAsync()
        {
            CloseRequested?.Invoke(null);
        }

    }
}


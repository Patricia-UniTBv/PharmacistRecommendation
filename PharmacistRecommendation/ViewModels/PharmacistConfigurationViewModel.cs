using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    public partial class PharmacistConfigurationViewModel: ObservableObject
    {
        public event Action<PharmacistDTO?>? CloseRequested;

        /* ─────────────  Proprietăţi legate în Entry-uri  ───────────── */
        [ObservableProperty] private string lastName = string.Empty;
        [ObservableProperty] private string firstName = string.Empty;
        [ObservableProperty] private string? ncm;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? phoneNr;
        [ObservableProperty] private string? username;
        [ObservableProperty] private string? password;

        /* ─────────────  Comenzi legate pe butoane  ───────────── */

        [RelayCommand]
        private async Task SaveAsync()
        {
            // validare minimă – nume şi prenume obligatorii
            if (string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName))
            {
                await Shell.Current.DisplayAlert(
                    "Eroare", "Completaţi câmpurile Nume şi Prenume!", "OK");
                return;
            }

            // construim DTO şi anunţăm View-ul să închidă popup-ul
            var dto = new PharmacistDTO(
                LastName.Trim(),
                FirstName.Trim(),
                Ncm?.Trim(),
                Email?.Trim(),
                PhoneNr?.Trim(),
                Username?.Trim(),
                Password);

            CloseRequested?.Invoke(dto);
        }

        [RelayCommand]
        private void Cancel() => CloseRequested?.Invoke(null);
    }

}


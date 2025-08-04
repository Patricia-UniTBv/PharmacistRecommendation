using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    public partial class EmailConfigurationViewModel : ObservableObject
    {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string appPassword;

        [ObservableProperty]
        private bool showGmailHelp;

        public EmailConfigurationViewModel()
        {
            Email = Preferences.Get("Email", string.Empty);
            AppPassword = Preferences.Get("AppPassword", string.Empty);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(AppPassword))
            {
                await Shell.Current.DisplayAlert("Eroare", "Completează ambele câmpuri.", "OK");
                return;
            }

            Preferences.Set("Email", Email);
            Preferences.Set("AppPassword", AppPassword);

            await Shell.Current.DisplayAlert("Succes", "Datele au fost salvate.", "OK");
            await Shell.Current.GoToAsync(".."); 
        }

        [RelayCommand]
        private void ToggleHelp()
        {
            ShowGmailHelp = !ShowGmailHelp;
        }

    }
}

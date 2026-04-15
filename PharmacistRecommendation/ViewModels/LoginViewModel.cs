using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;

namespace PharmacistRecommendation.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authService;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isPasswordVisible;

        public bool IsNotLoading => !IsLoading;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public LoginViewModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        partial void OnIsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotLoading));
        }

        partial void OnErrorMessageChanged(string value)
        {
            OnPropertyChanged(nameof(HasError));
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsLoading) return;

            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                var result = await _authService.LoginAsync(Username, Password);

                if (result.IsSuccess)
                {
                    SessionManager.SetCurrentUser(result.User);
                    await Shell.Current.GoToAsync("test_main");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Autentificare eșuată";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"A apărut o eroare: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                Password = string.Empty; 
            }
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            try
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Info", "Vă rugăm să contactați administratorul pentru a reseta parola.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing forgot password dialog: {ex.Message}");
            }
        }

        public void ClearForm()
        {
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
            IsLoading = false;
        }
    }
}
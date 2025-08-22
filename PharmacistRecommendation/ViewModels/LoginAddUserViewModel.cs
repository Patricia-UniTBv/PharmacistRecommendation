using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Services.Interfaces;
using System.Text.RegularExpressions;
using PharmacistRecommendation.Views;
using PharmacistRecommendation.Helpers;

namespace PharmacistRecommendation.ViewModels
{
    public partial class LoginAddUserViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;
        private readonly IPharmacyService _pharmacyService;

        [ObservableProperty]
        private bool isLoginMode = true;

        [ObservableProperty]
        private string loginUsername = string.Empty;

        [ObservableProperty]
        private string loginPassword = string.Empty;

        [ObservableProperty]
        private string loginErrorMessage = string.Empty;

        [ObservableProperty]
        private bool isLoginLoading;

        [ObservableProperty]
        private string firstName = string.Empty;

        [ObservableProperty]
        private string lastName = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string phone = string.Empty;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string ncm = string.Empty;

        [ObservableProperty]
        private string selectedRole = "Assistant";

        [ObservableProperty]
        private string addUserErrorMessage = string.Empty;

        [ObservableProperty]
        private bool isAddUserLoading;

        [ObservableProperty]
        private bool hasNoPharmacy;

        public bool IsNotLoginLoading => !IsLoginLoading;
        public bool IsNotAddUserLoading => !IsAddUserLoading;
        public bool HasLoginError => !string.IsNullOrEmpty(LoginErrorMessage);
        public bool HasAddUserError => !string.IsNullOrEmpty(AddUserErrorMessage);

        public string[] AvailableRoles { get; } = { "Pharmacist", "Assistant" };

        public LoginAddUserViewModel(IAuthenticationService authService, IUserService userService, IPharmacyService pharmacyService)
        {
            _authService = authService;
            _userService = userService;
            _pharmacyService = pharmacyService;

            Initialize();
        }

        partial void OnIsLoginLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotLoginLoading));
        }

        partial void OnIsAddUserLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotAddUserLoading));
        }

        partial void OnLoginErrorMessageChanged(string value)
        {
            OnPropertyChanged(nameof(HasLoginError));
        }

        partial void OnAddUserErrorMessageChanged(string value)
        {
            OnPropertyChanged(nameof(HasAddUserError));
        }

        [RelayCommand]
        private void ToggleMode()
        {
            IsLoginMode = !IsLoginMode;
            ClearAllErrors();
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsLoginLoading) return;

            LoginErrorMessage = string.Empty;
            IsLoginLoading = true;

            try
            {
                var result = await _authService.LoginAsync(LoginUsername, LoginPassword);

                if (result.IsSuccess)
                {
                    SessionManager.SetCurrentUser(result.User);

                    await Shell.Current.GoToAsync("test_main");
                }
                else
                {
                    LoginErrorMessage = result.ErrorMessage ?? "Login failed";
                }
            }
            catch (System.Exception ex)
            {
                LoginErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoginLoading = false;
                LoginPassword = string.Empty; 
            }
        }

        [RelayCommand]
        private async Task AddUserAsync()
        {
            if (IsAddUserLoading) return;

            AddUserErrorMessage = string.Empty;

            if (!ValidateAddUserInput())
                return;

            IsAddUserLoading = true;

            try
            {
                var id = await _pharmacyService.GetPharmacyId();
                var userDto = new UserDTO
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Email = Email.Trim(),
                    Phone = Phone?.Trim(),
                    Username = Username.Trim(),
                    Password = Password,
                    Ncm = Ncm?.Trim(),
                    Role = SelectedRole,
                    PharmacyId = id
                };

                await _userService.AddUserAsync(userDto);

                await Application.Current.MainPage.DisplayAlert("Success",
                    $"User {Username} has been added successfully!", "OK");

                ClearAddUserForm();
            }
            catch (System.Exception ex)
            {
                AddUserErrorMessage = $"Failed to add user: {ex.Message}";
            }
            finally
            {
                IsAddUserLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddPharmacyAsync()
        {
            if (IsAddUserLoading) return;

            AddUserErrorMessage = string.Empty;

            await Shell.Current.GoToAsync(nameof(AddPharmacyView));
        }

        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            try
            {
                await Application.Current.MainPage.DisplayAlert("Info",
                    "Please contact your administrator to reset your password.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing forgot password dialog: {ex.Message}");
            }
        }

        private bool ValidateAddUserInput()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                AddUserErrorMessage = "First name and last name are required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email))
            {
                AddUserErrorMessage = "Please enter a valid email address.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                AddUserErrorMessage = "Username is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                AddUserErrorMessage = "Password must be at least 6 characters long.";
                return false;
            }

            if (Password != ConfirmPassword)
            {
                AddUserErrorMessage = "Passwords do not match.";
                return false;
            }

            if (!IsValidPassword(Password))
            {
                AddUserErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.";
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }

        private bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$");
        }

        private void ClearAllErrors()
        {
            LoginErrorMessage = string.Empty;
            AddUserErrorMessage = string.Empty;
        }

        private void ClearAddUserForm()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            Ncm = string.Empty;
            SelectedRole = "Assistant";
        }

        public void ClearLoginForm()
        {
            LoginUsername = string.Empty;
            LoginPassword = string.Empty;
            LoginErrorMessage = string.Empty;
            IsLoginLoading = false;
        }

        private async void Initialize()
        {
            HasNoPharmacy = !await _pharmacyService.HasAnyPharmacyAsync();
        }
    }
}
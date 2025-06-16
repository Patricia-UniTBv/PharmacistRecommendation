using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    public partial class UsersManagementViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly PharmacistConfigurationViewModel _editUserVm;

        [ObservableProperty]
        private ObservableCollection<UserDTO> filteredUsers = new();

        public ObservableCollection<RoleOption> AvailableRoles { get; } =
                new ObservableCollection<RoleOption>
                {
                    new RoleOption { Name = "Farmaciști", Value = "Pharmacist" },
                    new RoleOption { Name = "Asistenți",  Value = "Assistant" }
                };

        [ObservableProperty]
        private RoleOption selectedRole;

        public UsersManagementViewModel(IUserService userService, PharmacistConfigurationViewModel editUserVm)
        {
            _userService = userService;
            _editUserVm = editUserVm;

            SelectedRole = AvailableRoles.FirstOrDefault(r => r.Value == "Assistant");
            Task.Run(async () => await LoadUsersAsync());
        }

        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            var filtered = users.Where(u => u.Role == SelectedRole?.Value);
            FilteredUsers = new ObservableCollection<UserDTO>(filtered);
        }

        partial void OnSelectedRoleChanged(RoleOption value)
        {
            LoadUsersCommand.Execute(null);
        }

        [RelayCommand]
        private async Task DeleteUserAsync(UserDTO user)
        {
            string roleRo = user.Role switch
            {
                "Pharmacist" => "farmacistul/a",
                "Assistant" => "asistentul/a",
                _ => "utilizatorul"
            };

            var confirm = await Shell.Current.DisplayAlert(
                "Confirmare",
                $"Ștergi {roleRo} {user.Username}?",
                "Da", "Nu");
            if (!confirm) return;

            await _userService.DeleteUserAsync(user.Id);
            FilteredUsers.Remove(user);
        }

        [RelayCommand]
        private async Task EditUserAsync(UserDTO user)
        {
            _editUserVm.LoadFrom(user);

            var popup = new PharmacistConfigurationView(_editUserVm);
            var result = await App.Current.MainPage.ShowPopupAsync(popup) as UserDTO;

            if (result != null)
                await LoadUsersAsync();
        }
    }

}

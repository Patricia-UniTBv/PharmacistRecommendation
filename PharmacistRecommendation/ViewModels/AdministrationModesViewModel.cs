using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Models;
using Entities.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    public partial class AdministrationModesViewModel : ObservableObject
    {
        private readonly IAdministrationModeService _service;

        [ObservableProperty]
        string searchText = "";

        [ObservableProperty]
        ObservableCollection<AdministrationMode> modes = new();

        [ObservableProperty]
        AdministrationMode? selectedMode;

        public AdministrationModesViewModel(IAdministrationModeService service)
        {
            _service = service;
            LoadModesCommand.Execute(null);
        }

        [RelayCommand]
        public async Task LoadModes()
        {
            var all = await _service.GetAllAsync();
            Modes = new ObservableCollection<AdministrationMode>(all);
        }

        [RelayCommand]
        public async Task Search()
        {
            var all = await _service.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(SearchText))
                Modes = new ObservableCollection<AdministrationMode>(all.Where(m => m.Name.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase)));
            else
                Modes = new ObservableCollection<AdministrationMode>(all);
        }

        [RelayCommand]
        public async Task Add()
        {
            var newMode = new AdministrationMode { Name = "NOU", IsActive = true };
            await _service.AddAsync(newMode);
            await LoadModes();
        }

        [RelayCommand]
        public async Task Edit(AdministrationMode mode)
        {
            if (mode != null)
            {
                await _service.UpdateAsync(mode);
                await LoadModes();
            }
        }

        [RelayCommand]
        public async Task Delete(AdministrationMode mode)
        {
            if (mode != null)
            {
                await _service.DeleteAsync(mode.Id);
                await LoadModes();
            }
        }

    }
}

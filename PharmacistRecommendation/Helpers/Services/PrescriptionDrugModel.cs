using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers.Services
{
    public partial class PrescriptionDrugModel : ObservableObject
    {
        [ObservableProperty]
        int index;

        [ObservableProperty]
        string name = string.Empty;

        [ObservableProperty]
        string displayText = string.Empty;

        [ObservableProperty]
        string concentration = string.Empty;

        [ObservableProperty]
        string pharmaceuticalForm = string.Empty;

        [ObservableProperty]
        string diseaseCode = string.Empty;

        [ObservableProperty]
        string dose = string.Empty;

        [ObservableProperty]
        string morning = string.Empty;

        [ObservableProperty]
        string noon = string.Empty;

        [ObservableProperty]
        string evening = string.Empty;

        [ObservableProperty]
        string night = string.Empty;

        [ObservableProperty]
        ObservableCollection<string> filteredMedications = new();

        [ObservableProperty]
        bool showSuggestions = false;

        [ObservableProperty]
        AdministrationMode? administrationMode; 

        [ObservableProperty]
        int? administrationModeId;
    }
}
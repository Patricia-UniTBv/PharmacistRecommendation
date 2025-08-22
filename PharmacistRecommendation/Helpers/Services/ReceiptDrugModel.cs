using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Models;

namespace PharmacistRecommendation.Helpers.Services
{
    public partial class ReceiptDrugModel: ObservableObject
    {
        [ObservableProperty]
        int index;

        [ObservableProperty]
        string name = string.Empty;

        [ObservableProperty]
        string morning = string.Empty;

        [ObservableProperty]
        string noon = string.Empty;

        [ObservableProperty]
        string evening = string.Empty;

        [ObservableProperty]
        string night = string.Empty;

        [ObservableProperty]
        AdministrationMode? administrationMode;
    }
}

using PharmacistRecommendation.Helpers.Services;
using PharmacistRecommendation.ViewModels;
using System.Collections.ObjectModel;

namespace PharmacistRecommendation.Views;

public partial class MixedActIssuanceView : ContentPage
{
    public MixedActIssuanceViewModel ViewModel { get; }

    public MixedActIssuanceView(MixedActIssuanceViewModel vm)
    {
        InitializeComponent();
        BindingContext = ViewModel = vm;
    }

    private void OnMedicationNameTextChanged(object sender, TextChangedEventArgs e)
  {
        if (sender is Entry entry && entry.BindingContext is PrescriptionDrugModel drug)
        {
            if (BindingContext is MixedActIssuanceViewModel vm)
            {
                vm.FilterMedications(e.NewTextValue, drug);
            }
        }
    }

}
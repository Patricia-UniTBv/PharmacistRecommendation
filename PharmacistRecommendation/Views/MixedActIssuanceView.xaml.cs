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

        if (BindingContext is MixedActIssuanceViewModel vm2)
            vm2.SearchEntryReference = SearchEntry;
    }

    private async void OnMedicationNameTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry || entry.BindingContext is not PrescriptionDrugModel drug)
            return;

        if (BindingContext is not MixedActIssuanceViewModel vm)
            return;

        // Add a small delay to prevent too many calls
        await Task.Delay(50);

        vm.FilterMedications(e.NewTextValue ?? string.Empty, drug);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is MixedActIssuanceViewModel vm)
            vm.UpdateSuggestions(e.NewTextValue);
    }

    //private void OnRowSearchTextChanged(object sender, TextChangedEventArgs e)
    //{
    //    if (sender is Entry entry && entry.BindingContext is ReceiptDrugModel row &&
    //        BindingContext is MixedActIssuanceViewModel vm)
    //    {
    //        vm.UpdateSuggestionsForRow(row, e.NewTextValue);
    //    }
    //}


}
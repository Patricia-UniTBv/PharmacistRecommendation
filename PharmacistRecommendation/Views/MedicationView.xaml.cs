using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class MedicationView : ContentPage
{
    public MedicationView(MedicationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MedicationViewModel viewModel)
        {
            await viewModel.LoadMedicationsAsync();
        }
    }
}
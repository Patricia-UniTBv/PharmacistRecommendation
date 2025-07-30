using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class AddEditMedicationView : ContentPage
{
    public AddEditMedicationView(AddEditMedicationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
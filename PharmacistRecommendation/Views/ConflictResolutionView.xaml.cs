using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class ConflictResolutionView : ContentPage
{
    public ConflictResolutionView(ConflictResolutionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
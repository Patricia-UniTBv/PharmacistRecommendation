using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class ReportsView : ContentPage
{
    public ReportsView(ReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
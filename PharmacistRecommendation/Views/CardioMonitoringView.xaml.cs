using PharmacistRecommendation.ViewModels;
using System.Diagnostics;
namespace PharmacistRecommendation.Views;

public partial class CardioMonitoringView : ContentPage
{
    private readonly CardioMonitoringViewModel _viewModel;

    public CardioMonitoringView(CardioMonitoringViewModel viewModel)
    {
        try
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
        }
    }
}
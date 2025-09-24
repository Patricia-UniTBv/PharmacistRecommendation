using PharmacistRecommendation.ViewModels;
using System.Diagnostics;
namespace PharmacistRecommendation.Views;

public partial class MonitoringView : ContentPage
{
    double _savedY;                 
    bool _restorePending;
    public MonitoringView(MonitoringViewModel viewModel)
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
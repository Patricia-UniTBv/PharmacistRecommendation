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


    //void OnPickerFocused(object sender, FocusEventArgs e)
    //{
    //    _savedY = MainScroll.ScrollY;
    //    _restorePending = true;
    //}

    //void OnPickerUnfocused(object sender, FocusEventArgs e)
    //{
    //    if (!_restorePending) return;

    //    const int framesToWait = 2;
    //    var delay = (int)(1000.0 / DeviceDisplay.MainDisplayInfo.RefreshRate * framesToWait);

    //    MainThread.BeginInvokeOnMainThread(async () =>
    //    {
    //        await Task.Delay(delay);
    //        await MainScroll.ScrollToAsync(0, _savedY, false);
    //        _restorePending = false;
    //    });
    //}
}
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Views;
using System.Diagnostics;

namespace PharmacistRecommendation
{
    public partial class App : Application
    {
        public App(CardioMonitoringView cardioMonitoringView)
        {
            try
            {
                InitializeComponent();
                MainPage = cardioMonitoringView;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}

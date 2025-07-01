using PharmacistRecommendation.Views;

namespace PharmacistRecommendation
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void OnCardioClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CardioMonitoringView));
        }

        private async void OnDiabetClicked(object sender, EventArgs e)
        {
           // await Shell.Current.GoToAsync(nameof(DiabetesMonitoringView));
        }

        private async void OnTemperatureClicked(object sender, EventArgs e)
        {
           // await Shell.Current.GoToAsync(nameof(TemperatureMonitoringView));
        }
    }
}

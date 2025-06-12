using PharmacistRecommendation.Views;

namespace PharmacistRecommendation
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("monitoring", typeof(MonitoringView));
        }

        private async void OnMonitClicked(object sender, EventArgs e)
        {
            await GoToAsync("monitoring");
        }
    }
}

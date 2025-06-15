using CommunityToolkit.Maui.Views;
using DTO;
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.ViewModels;
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

        private async void OnAddPharmacistClicked(object sender, EventArgs e)
        {
            var vm = ServiceHelper.GetService<PharmacistConfigurationViewModel>();
            var popup = new PharmacistConfigurationView(vm);

            var result = await App.Current.MainPage.ShowPopupAsync(popup);


            if (result is null) return;   

        }

    }
}

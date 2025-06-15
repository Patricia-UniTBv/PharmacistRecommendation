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
            // creezi ViewModel-ul prin DI
            var vm = ServiceHelper.GetService<PharmacistConfigurationViewModel>();
            var popup = new PharmacistConfigurationView(vm);

            // rulează pe pagina curentă
            var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup)
                              as PharmacistDTO;

            if (result is null) return;   // utilizatorul a închis fără salvare

            // TODO: salvează în DB …
        }

    }
}

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
            Routing.RegisterRoute("users_management", typeof(UsersManagementView));
            Routing.RegisterRoute("new_card", typeof(CardConfigurationView));
            Routing.RegisterRoute("gdpr_configuration", typeof(GdprConfigurationView));
            Routing.RegisterRoute("mixed_issuance", typeof(MixedActIssuanceView));
            Routing.RegisterRoute("administration_modes", typeof (AdministrationModesView));
        }

        private async void OnNewCardClicked(object sender,  EventArgs e)
        {
            await GoToAsync("new_card");
        }


        private async void OnMonitClicked(object sender, EventArgs e)
        {
            await GoToAsync("monitoring");
        }

        private async void OnMixedIssuanceClicked(object sender, EventArgs e)
        {
            await GoToAsync("mixed_issuance");
        }

        private async void OnAddPharmacistClicked(object sender, EventArgs e)
        {
            var vm = ServiceHelper.GetService<PharmacistConfigurationViewModel>();
            var popup = new PharmacistConfigurationView(vm);

            var result = await App.Current.MainPage.ShowPopupAsync(popup);


            if (result is null) return;   

        }

        private async void OnUsersManagementClicked(object sender, EventArgs e)
        {
            await GoToAsync("users_management");
        }

        private async void OnGdprConfigClicked(object sender, EventArgs e)
        {
            await GoToAsync("gdpr_configuration");
        }

        private async void OnAdministrationModesClicked(object sender, EventArgs e)
        {
            await GoToAsync("administration_modes");
        }

    }
}

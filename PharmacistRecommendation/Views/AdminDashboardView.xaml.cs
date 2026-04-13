using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views
{
    public partial class AdminDashboardView : ContentPage
    {
        public AdminDashboardView(AdminDashboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (SessionManager.CurrentUser?.Role?.ToLower() != "admin")
            {
                await DisplayAlert("Acces interzis", "Nu aveți permisiunea de a accesa această pagină.", "OK");
                SessionManager.SetCurrentUser(null);
                await Shell.Current.GoToAsync("//login");
            }
        }
    }
}

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

        private async void OnNavigateBack(object sender, EventArgs e)
        {
            try
            {
                if (Shell.Current.Navigation.NavigationStack.Count >= 1)
                    await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Back navigation error: {ex.Message}");
            }
        }

        private void OnCloseApp(object sender, EventArgs e)
        {
            try
            {
                if (Application.Current?.Handler?.MauiContext?.Services is IDisposable services)
                    services.Dispose();
            }
            catch { }
            Application.Current?.Quit();
        }
    }
}

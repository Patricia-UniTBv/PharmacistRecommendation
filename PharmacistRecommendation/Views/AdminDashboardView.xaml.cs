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
    }
}

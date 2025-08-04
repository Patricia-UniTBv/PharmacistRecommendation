using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views
{
    public partial class MainPageView : ContentPage
    {
        public MainPageView(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
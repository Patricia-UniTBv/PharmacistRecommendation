using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views
{
    public partial class LoginView : ContentPage
    {
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is LoginViewModel viewModel)
            {
                viewModel.ClearForm();
            }
        }
    }
}
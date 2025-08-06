using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views
{
    public partial class LoginAddUserView : ContentPage
    {
        public LoginAddUserView(LoginAddUserViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is LoginAddUserViewModel viewModel)
            {
                viewModel.ClearLoginForm();
            }
        }
    }
}
using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class UsersManagementView : ContentPage
{
	public UsersManagementView(UsersManagementViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}
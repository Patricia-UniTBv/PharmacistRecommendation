using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class EmailConfigurationView : ContentPage
{
	public EmailConfigurationView(EmailConfigurationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
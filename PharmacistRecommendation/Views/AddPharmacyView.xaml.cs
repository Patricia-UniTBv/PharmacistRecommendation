using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class AddPharmacyView : ContentPage
{
	public AddPharmacyView(AddPharmacyViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
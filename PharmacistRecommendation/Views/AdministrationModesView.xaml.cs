using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class AdministrationModesView : ContentPage
{
	public AdministrationModesView(AdministrationModesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
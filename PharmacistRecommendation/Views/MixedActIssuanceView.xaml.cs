using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class MixedActIssuanceView : ContentPage
{
	public MixedActIssuanceView(MixedActIssuanceViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
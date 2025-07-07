using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class GdprConfigurationView : ContentPage
{
	public GdprConfigurationView(GdprConfigurationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is GdprConfigurationViewModel vm)
            await vm.LoadConsentTemplateAsync();
    }

}
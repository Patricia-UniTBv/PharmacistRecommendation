using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class AddPharmacyView : ContentPage
{
	public AddPharmacyView(AddPharmacyViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AddPharmacyViewModel vm)
        {
            await vm.LoadPharmacyAsync();
        }
    }
}
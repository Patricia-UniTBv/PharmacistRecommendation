using CommunityToolkit.Maui.Views;
using DTO;
using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class CardConfigurationView : Popup
{
	public CardConfigurationView(CardConfigurationViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;

        vm.CloseRequested += OnCloseRequested;
    }
    private void OnCloseRequested(PharmacyCardDTO? result)
    {
        this.Close(result);
    }
}

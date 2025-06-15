using CommunityToolkit.Maui.Views;
using DTO;
using PharmacistRecommendation.ViewModels;
namespace PharmacistRecommendation.Views;

public partial class PharmacistConfigurationView : Popup
{
    public PharmacistConfigurationView(PharmacistConfigurationViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        vm.CloseRequested += OnCloseRequested;
    }
    private void OnCloseRequested(UserDTO? result)
    {
        this.Close(result); 
    }
}
using CommunityToolkit.Maui.Views;
using PharmacistRecommendation.ViewModels;
namespace PharmacistRecommendation.Views;

public partial class PharmacistConfigurationView : Popup
{
    public PharmacistConfigurationView(PharmacistConfigurationViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
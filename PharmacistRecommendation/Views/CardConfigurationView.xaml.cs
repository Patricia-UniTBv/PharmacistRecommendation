using CommunityToolkit.Maui.Views;
using DTO;
using PharmacistRecommendation.ViewModels;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace PharmacistRecommendation.Views;

public partial class CardConfigurationView : ContentPage
{
	public CardConfigurationView(CardConfigurationViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}

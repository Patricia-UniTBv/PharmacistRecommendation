using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class ImportConfigurationView : ContentPage
{
	public ImportConfigurationView(ImportConfigurationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
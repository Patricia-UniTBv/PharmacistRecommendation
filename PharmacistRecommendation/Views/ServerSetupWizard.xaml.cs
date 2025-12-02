using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views
{
    public partial class ServerSetupWizard : ContentPage
    {
        public ServerSetupWizard(ServerSetupWizardViewModel viewModel)
        {
       InitializeComponent();
            BindingContext = viewModel;
    }
    }
}

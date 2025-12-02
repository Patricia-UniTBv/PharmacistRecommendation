using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views
{
    public partial class ClientSetupWizard : ContentPage
    {
        public ClientSetupWizard(ClientSetupWizardViewModel viewModel)
        {
            InitializeComponent();
        BindingContext = viewModel;
        }
    }
}

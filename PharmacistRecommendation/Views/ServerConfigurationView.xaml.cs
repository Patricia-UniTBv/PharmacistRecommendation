using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views
{
    public partial class ServerConfigurationView : ContentPage
    {
        public ServerConfigurationView(ServerConfigurationViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}

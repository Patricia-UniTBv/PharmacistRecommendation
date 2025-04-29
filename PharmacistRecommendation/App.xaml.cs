using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Views;
using System.Diagnostics;

namespace PharmacistRecommendation
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //Current.UserAppTheme = AppTheme.Light;
            MainPage = new AppShell();
        }
    }
}

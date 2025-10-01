using PharmacistRecommendation.ViewModels;

namespace PharmacistRecommendation.Views;

public partial class EmailConfigurationView : ContentPage
{
	public EmailConfigurationView(EmailConfigurationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
    private async void OnSecurityLinkTapped(object sender, EventArgs e)
    {
        await Launcher.OpenAsync("https://myaccount.google.com/security");
    }

    private async void OnAppPasswordsTapped(object sender, EventArgs e)
    {
        await Launcher.OpenAsync("https://myaccount.google.com/apppasswords");
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
    }


}
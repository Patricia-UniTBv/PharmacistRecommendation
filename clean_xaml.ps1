$xamlFiles = @(
    "PharmacistRecommendation\Views\AddEditMedicationView.xaml",
    "PharmacistRecommendation\Views\AddPharmacyView.xaml",
    "PharmacistRecommendation\Views\AdministrationModesView.xaml",
    "PharmacistRecommendation\Views\CardConfigurationView.xaml",
    "PharmacistRecommendation\Views\EmailConfigurationView.xaml",
    "PharmacistRecommendation\Views\GdprConfigurationView.xaml",
    "PharmacistRecommendation\Views\ImportConfigurationView.xaml",
    "PharmacistRecommendation\Views\LoginAddUserView.xaml",
    "PharmacistRecommendation\Views\LoginView.xaml",
    "PharmacistRecommendation\Views\MainPageView.xaml",
    "PharmacistRecommendation\Views\MedicationView.xaml",
    "PharmacistRecommendation\Views\MixedActIssuanceView.xaml",
    "PharmacistRecommendation\Views\MonitoringView.xaml",
    "PharmacistRecommendation\Views\PharmacistConfigurationView.xaml",
    "PharmacistRecommendation\Views\ReportsView.xaml",
    "PharmacistRecommendation\Views\ServerConfigurationView.xaml",
    "PharmacistRecommendation\Views\UsersManagementView.xaml",
    "PharmacistRecommendation\AppShell.xaml"
)

foreach ($file in $xamlFiles) {
    if (Test-Path $file) {
        Write-Host "Processing $file..."
        $content = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)
        
        # Remove all non-ASCII characters (including emojis)
        $content = $content -replace '[^\x00-\x7F]', ''
        
        # Save with UTF-8 encoding
        [System.IO.File]::WriteAllText($file, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  Cleaned $file"
    }
}

Write-Host "All XAML files have been cleaned!"

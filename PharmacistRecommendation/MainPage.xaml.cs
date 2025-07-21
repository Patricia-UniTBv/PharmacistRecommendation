using Entities.Data;
using Microsoft.Data.SqlClient;
using PharmacistRecommendation.Views;

namespace PharmacistRecommendation
{
    public partial class MainPage : ContentPage
    {
        private readonly PharmacistRecommendationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public MainPage(PharmacistRecommendationDbContext context, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _context = context;
            _serviceProvider = serviceProvider;
        }

        private async void OnMedicationClicked(object sender, EventArgs e)
        {
            try
            {
                // Get the MedicationView from the DI container
                var medicationView = _serviceProvider.GetRequiredService<MedicationView>();
                await Navigation.PushAsync(medicationView);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigation Error", $"Failed to open Medication page: {ex.Message}", "OK");
            }
        }

        private async void OnTestDbClicked(object sender, EventArgs e)
        {
            try
            {
                TestDbBtn.Text = "Testing...";
                TestDbBtn.IsEnabled = false;

                // Test basic connection
                var connectionString = "Server=localhost\\SQLEXPRESS;Database=PharmacistRecommendationDB;Trusted_Connection=true;TrustServerCertificate=true;";
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Test if Medication table exists and has data
                using var command = new SqlCommand("SELECT COUNT(*) FROM Medication", connection);
                var count = await command.ExecuteScalarAsync();

                await DisplayAlert("Database Connection",
                    $"✅ Connected successfully!\nMedication table has {count} records.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Connection Error", $"❌ Error: {ex.Message}", "OK");
            }
            finally
            {
                TestDbBtn.Text = "Test Database Connection";
                TestDbBtn.IsEnabled = true;
            }
        }


        // Add this method to your MainPage.xaml.cs class

        private async void OnTestMedicationClicked(object sender, EventArgs e)
        {
            try
            {
                TestMedicationBtn.Text = "Testing...";
                TestMedicationBtn.IsEnabled = false;

                // Get medication service from DI
                var medicationService = _serviceProvider.GetRequiredService<Entities.Services.Interfaces.IMedicationService>();

                // Test loading all medications
                var medications = await medicationService.GetAllMedicationsAsync();

                if (medications.Any())
                {
                    var firstMed = medications.First();
                    await DisplayAlert("Medication Test",
                        $"✅ Found {medications.Count} medications!\n\n" +
                        $"First medication:\n" +
                        $"Name: {firstMed.Denumire ?? "N/A"}\n" +
                        $"CodCIM: {firstMed.CodCIM ?? "N/A"}\n" +
                        $"ATC: {firstMed.CodATC ?? "N/A"}", "OK");
                }
                else
                {
                    await DisplayAlert("Medication Test",
                        "⚠️ No medications found in database.\n\n" +
                        "You may need to import medication data.", "OK");
                }

                // Test search functionality
                var searchResults = await medicationService.SearchMedicationsAsync("aspirin");
                await DisplayAlert("Search Test",
                    $"Search for 'aspirin' returned {searchResults.Count} results", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Medication Test Error",
                    $"❌ Error testing medication functionality: {ex.Message}", "OK");
            }
            finally
            {
                TestMedicationBtn.Text = "Test Medication Data";
                TestMedicationBtn.IsEnabled = true;
            }
        }
    }
}
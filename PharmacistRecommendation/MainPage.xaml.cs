using Entities.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace PharmacistRecommendation
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly PharmacistRecommendationDbContext _context;

        public MainPage(PharmacistRecommendationDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";
            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void OnTestDbClicked(object sender, EventArgs e)
        {
            try
            {
                TestDbBtn.Text = "Testing...";
                TestDbBtn.IsEnabled = false;

                // Test basic connection first
                var connectionString = "Server=localhost\\SQLEXPRESS;Database=PharmacistRecommendationDB;Trusted_Connection=true;TrustServerCertificate=true;";

                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                await connection.OpenAsync();

                await DisplayAlert("Database Connection", "✅ Connected successfully!", "OK");
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
    }
}
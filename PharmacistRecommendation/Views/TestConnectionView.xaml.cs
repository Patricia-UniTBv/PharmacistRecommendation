using Entities.Data;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace PharmacistRecommendation.Views;

public partial class TestConnectionView : ContentPage
{
    private readonly PharmacistRecommendationDbContext _context;

    public TestConnectionView(PharmacistRecommendationDbContext context)
    {
        InitializeComponent();
        _context = context;
    }

    private async void OnTestConnectionClicked(object sender, EventArgs e)
    {
        try
        {
            TestButton.Text = "Testing...";
            TestButton.IsEnabled = false;
            ResultLabel.Text = "Testing database connection...";

            // Test database connection
            await _context.Database.OpenConnectionAsync();
            await _context.Database.CloseConnectionAsync();

            // Load some medications to test data access
            var medications = await _context.Medications
                .Take(10)
                .ToListAsync();

            MedicationsCollectionView.ItemsSource = medications;

            ResultLabel.Text = $"✅ Connection successful! Found {medications.Count} medications.";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"❌ Connection failed: {ex.Message}";
            MedicationsCollectionView.ItemsSource = null;
        }
        finally
        {
            TestButton.Text = "Test Database Connection";
            TestButton.IsEnabled = true;
        }
    }
}
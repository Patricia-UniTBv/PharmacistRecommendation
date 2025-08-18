using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmacistRecommendation.Helpers;
using System.Collections.ObjectModel;

namespace PharmacistRecommendation.ViewModels;

[QueryProperty(nameof(ReportType), "type")]
public partial class ReportsViewModel : ObservableObject
{
    private readonly IPdfReportService _pdfReportService;

    public ReportsViewModel(IPdfReportService pdfReportService)
    {
        _pdfReportService = pdfReportService;
        StartDate = DateTime.Today.AddDays(-30);
        EndDate = DateTime.Today;
    }

    [ObservableProperty]
    private string reportType = string.Empty;

    [ObservableProperty]
    private DateTime startDate;

    [ObservableProperty]
    private DateTime endDate;

    [ObservableProperty]
    private string patientFilter = string.Empty;

    [ObservableProperty]
    private bool isGeneratingReport;

    // This will be called when the page is navigated to with a query parameter
    partial void OnReportTypeChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500); // Wait for UI to stabilize
                var reportTypeModel = ReportTypes.FirstOrDefault(r => GetReportTypeString(r.ReportType) == value);
                if (reportTypeModel != null)
                {
                    await GenerateReportAsync(reportTypeModel);
                }
            });
        }
    }

    public ObservableCollection<ReportTypeModel> ReportTypes { get; } = new()
    {
        new ReportTypeModel
        {
            Title = "Raport Acte Mixte",
            Description = "Rapoarte pentru recomandări farmaceutice mixte",
            Icon = "📋",
            ReportType = ReportTypeEnum.MixedActs
        },
        new ReportTypeModel
        {
            Title = "Raport Acte Proprii",
            Description = "Rapoarte pentru recomandările proprii ale farmaciei",
            Icon = "💊",
            ReportType = ReportTypeEnum.OwnActs
        },
        new ReportTypeModel
        {
            Title = "Raport Acte Consecutive Prescripției",
            Description = "Rapoarte pentru recomandări consecutive prescripțiilor medicale",
            Icon = "📄",
            ReportType = ReportTypeEnum.ConsecutivePrescriptionActs
        },
        new ReportTypeModel
        {
            Title = "Lista Monitorizări",
            Description = "Rapoarte de monitorizare a pacienților",
            Icon = "📊",
            ReportType = ReportTypeEnum.MonitoringList
        }
    };

    [RelayCommand]
    private async Task GenerateReportAsync(ReportTypeModel reportType)
    {
        if (IsGeneratingReport) return;

        try
        {
            IsGeneratingReport = true;
            System.Diagnostics.Debug.WriteLine($"=== GenerateReportAsync: Starting {reportType.ReportType} ===");

            string filePath = await Task.Run(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Background task: Starting report generation");
                    var result = reportType.ReportType switch
                    {
                        ReportTypeEnum.MixedActs => await _pdfReportService.CreateMixedActsReportAsync(StartDate, EndDate, PatientFilter),
                        ReportTypeEnum.OwnActs => await _pdfReportService.CreateOwnActsReportAsync(StartDate, EndDate, PatientFilter),
                        ReportTypeEnum.ConsecutivePrescriptionActs => await _pdfReportService.CreateConsecutivePrescriptionActsReportAsync(StartDate, EndDate, PatientFilter),
                        ReportTypeEnum.MonitoringList => await _pdfReportService.CreateMonitoringListReportAsync(StartDate, EndDate, PatientFilter),
                        _ => throw new ArgumentException("Unknown report type")
                    };
                    System.Diagnostics.Debug.WriteLine($"Background task: Report generated successfully at {result}");
                    return result;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Background task error: {ex}");
                    throw;
                }
            });

            System.Diagnostics.Debug.WriteLine("Returning to main thread for UI operations");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine("Main thread: Showing success dialog");
                
                // Show success message with option to open file
                var openFile = await Shell.Current.DisplayAlert(
                    "Succes", 
                    $"Raportul a fost generat cu succes!\nLocație: {filePath}\n\nDoriți să deschideți fișierul?", 
                    "Da", "Nu");

                if (openFile)
                {
                    System.Diagnostics.Debug.WriteLine("User chose to open file, attempting to open...");
                    await TryOpenFileAsync(filePath);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User chose not to open file");
                }
            });

            System.Diagnostics.Debug.WriteLine($"=== GenerateReportAsync: Completed Successfully {reportType.ReportType} ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Main error in GenerateReportAsync: {ex}");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlert("Eroare", 
                    $"Eroare la generarea raportului: {ex.Message}", "OK");
            });
        }
        finally
        {
            IsGeneratingReport = false;
            System.Diagnostics.Debug.WriteLine("IsGeneratingReport set to false");
        }
    }

    private async Task TryOpenFileAsync(string filePath)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Attempting to open file: {filePath}");
            
            // Check if file exists first
            if (!File.Exists(filePath))
            {
                await Shell.Current.DisplayAlert("Eroare", "Fișierul nu a fost găsit.", "OK");
                return;
            }

            // Try different approaches to open the file
            try
            {
                System.Diagnostics.Debug.WriteLine("Method 1: Using Launcher.Default.OpenAsync with ReadOnlyFile");
                await Launcher.Default.OpenAsync(new OpenFileRequest("Raport", new ReadOnlyFile(filePath)));
                System.Diagnostics.Debug.WriteLine("Method 1: Success");
            }
            catch (Exception ex1)
            {
                System.Diagnostics.Debug.WriteLine($"Method 1 failed: {ex1.Message}");
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("Method 2: Using Launcher.Default.OpenAsync with file path");
                    await Launcher.Default.OpenAsync(filePath);
                    System.Diagnostics.Debug.WriteLine("Method 2: Success");
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Method 2 failed: {ex2.Message}");
                    
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Method 3: Using Process.Start");
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                        System.Diagnostics.Debug.WriteLine("Method 3: Success");
                    }
                    catch (Exception ex3)
                    {
                        System.Diagnostics.Debug.WriteLine($"Method 3 failed: {ex3.Message}");
                        
                        // All methods failed, just show the path
                        await Shell.Current.DisplayAlert("Info", 
                            $"Raportul a fost salvat la:\n{filePath}\n\nNu s-a putut deschide automat.", "OK");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TryOpenFileAsync general error: {ex}");
            await Shell.Current.DisplayAlert("Avertizare", 
                $"Raportul a fost generat, dar nu a putut fi deschis automat.\nLocație: {filePath}", "OK");
        }
    }

    [RelayCommand]
    private void ResetFilters()
    {
        StartDate = DateTime.Today.AddDays(-30);
        EndDate = DateTime.Today;
        PatientFilter = string.Empty;
    }

    private string GetReportTypeString(ReportTypeEnum reportType)
    {
        return reportType switch
        {
            ReportTypeEnum.MixedActs => "mixed",
            ReportTypeEnum.OwnActs => "own",
            ReportTypeEnum.ConsecutivePrescriptionActs => "consecutive",
            ReportTypeEnum.MonitoringList => "monitoring",
            _ => string.Empty
        };
    }
}

public class ReportTypeModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public ReportTypeEnum ReportType { get; set; }
}

public enum ReportTypeEnum
{
    MixedActs,
    OwnActs,
    ConsecutivePrescriptionActs,
    MonitoringList
}
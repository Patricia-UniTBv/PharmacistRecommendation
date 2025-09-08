using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmacistRecommendation.Helpers;
using System.Collections.ObjectModel;
using Entities.Services.Interfaces;
using Entities.Models;

namespace PharmacistRecommendation.ViewModels;

[QueryProperty(nameof(ReportType), "type")]
public partial class ReportsViewModel : ObservableObject        
{
    private readonly IPdfReportService _pdfReportService;
    private readonly IPrescriptionService _prescriptionService;
    private readonly IMonitoringService _monitoringService;

    public ReportsViewModel(IPdfReportService pdfReportService, IPrescriptionService prescriptionService, IMonitoringService monitoringService)
    {
        _pdfReportService = pdfReportService;
        _prescriptionService = prescriptionService;
        _monitoringService = monitoringService;
        StartDate = DateTime.Today.AddDays(-30);
        EndDate = DateTime.Today;
        
        System.Diagnostics.Debug.WriteLine("ReportsViewModel constructor called - services injected successfully");
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

    [ObservableProperty]
    private bool isLoadingData;

    [ObservableProperty]
    private ReportTypeModel? selectedReportType;

    [ObservableProperty]
    private string currentReportTitle = string.Empty;

    [ObservableProperty]
    private bool hasData;

    [ObservableProperty]
    private string dataCount = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Prescription> prescriptionsData = new();

    [ObservableProperty]
    private ObservableCollection<object> monitoringData = new();

    partial void OnReportTypeChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500);
                var reportTypeModel = ReportTypes.FirstOrDefault(r => GetReportTypeString(r.ReportType) == value);
                if (reportTypeModel != null)
                {
                    SelectedReportType = reportTypeModel;
                    await LoadReportDataAsync();
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
    private async Task SelectReportTypeAsync(ReportTypeModel reportType)
    {
        System.Diagnostics.Debug.WriteLine($"SelectReportTypeAsync called for: {reportType.Title}");
        SelectedReportType = reportType;
        await LoadReportDataAsync();
    }

    [RelayCommand]
    private async Task LoadReportDataAsync()
    {
        if (SelectedReportType == null || IsLoadingData) return;

        try
        {
            IsLoadingData = true;
            CurrentReportTitle = SelectedReportType.Title;
            
            PrescriptionsData.Clear();
            MonitoringData.Clear();
            HasData = false;
            DataCount = "";

            System.Diagnostics.Debug.WriteLine($"🔍 Loading data for {SelectedReportType.ReportType}");

            switch (SelectedReportType.ReportType)
            {
                case ReportTypeEnum.MixedActs:
                    await LoadMixedActsData();
                    break;
                case ReportTypeEnum.OwnActs:
                    await LoadOwnActsData();
                    break;
                case ReportTypeEnum.ConsecutivePrescriptionActs:
                    await LoadConsecutiveActsData();
                    break;
                case ReportTypeEnum.MonitoringList:
                    await LoadMonitoringData();
                    break;
            }

            if (!HasData)
            {
                await Shell.Current.DisplayAlert("Info", 
                    $"Nu au fost găsite date pentru {SelectedReportType.Title} în perioada {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}.\n\n" +
                    $"Datele au fost căutate cu succes, dar nu există înregistrări pentru criteriile selectate.", 
                    "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading data: {ex}");
            await Shell.Current.DisplayAlert("Eroare", $"Eroare la încărcarea datelor: {ex.Message}", "OK");
        }
        finally
        {
            IsLoadingData = false;
            System.Diagnostics.Debug.WriteLine($"✅ Data loading completed. HasData: {HasData}, DataCount: {DataCount}");
        }
    }

    private async Task LoadMixedActsData()
    {
        System.Diagnostics.Debug.WriteLine("📋 Loading Mixed Acts data...");
        var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
        System.Diagnostics.Debug.WriteLine($"📋 Total prescriptions from DB: {prescriptions.Count}");
        
        var filteredPrescriptions = prescriptions
            .Where(p => p.IssueDate >= StartDate && p.IssueDate <= EndDate.Date.AddDays(1).AddTicks(-1))
            .Where(p => string.IsNullOrEmpty(PatientFilter) || 
                       (p.PatientName?.Contains(PatientFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                       (p.PatientCnp?.Contains(PatientFilter, StringComparison.OrdinalIgnoreCase) == true))
            .Where(p => p.PrescriptionMedications.Any(m => m.IsWithPrescription == true) && 
                       p.PrescriptionMedications.Any(m => m.IsWithPrescription == false))
            .OrderBy(p => p.IssueDate)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"📋 Filtered Mixed Acts: {filteredPrescriptions.Count}");

        foreach (var prescription in filteredPrescriptions)
        {
            PrescriptionsData.Add(prescription);
        }

        HasData = PrescriptionsData.Any();
        DataCount = $"Total acte mixte: {PrescriptionsData.Count}";
    }

    private async Task LoadOwnActsData()
    {
        System.Diagnostics.Debug.WriteLine("💊 Loading Own Acts data...");
        var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
        System.Diagnostics.Debug.WriteLine($"💊 Total prescriptions from DB: {prescriptions.Count}");
        
        var filteredPrescriptions = prescriptions
            .Where(p => p.IssueDate >= StartDate && p.IssueDate <= EndDate.Date.AddDays(1).AddTicks(-1))
            .Where(p => string.IsNullOrEmpty(PatientFilter) || 
                       (p.PatientName?.Contains(PatientFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                       (p.PatientCnp?.Contains(PatientFilter, StringComparison.OrdinalIgnoreCase) == true))
            .Where(p => p.PrescriptionMedications.Any() && p.PrescriptionMedications.All(m => m.IsWithPrescription == false))
            .OrderBy(p => p.IssueDate)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"💊 Filtered Own Acts: {filteredPrescriptions.Count}");

        foreach (var prescription in filteredPrescriptions)
        {
            PrescriptionsData.Add(prescription);
        }

        HasData = PrescriptionsData.Any();
        DataCount = $"Total acte proprii: {PrescriptionsData.Count}";
    }

    private async Task LoadConsecutiveActsData()
    {
        System.Diagnostics.Debug.WriteLine("📄 Loading Consecutive Acts data...");
        var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
        System.Diagnostics.Debug.WriteLine($"📄 Total prescriptions from DB: {prescriptions.Count}");
        
        var filteredPrescriptions = prescriptions
            .Where(p => p.IssueDate >= StartDate && p.IssueDate <= EndDate.Date.AddDays(1).AddTicks(-1))
            .Where(p => string.IsNullOrEmpty(PatientFilter) || 
                       (p.PatientName?.Contains(PatientFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                       (p.PatientCnp?.Contains(PatientFilter, StringComparison.OrdinalIgnoreCase) == true))
            .Where(p => p.PrescriptionMedications.Any(m => m.IsWithPrescription == true))
            .OrderBy(p => p.IssueDate)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"📄 Filtered Consecutive Acts: {filteredPrescriptions.Count}");

        foreach (var prescription in filteredPrescriptions)
        {
            PrescriptionsData.Add(prescription);
        }

        HasData = PrescriptionsData.Any();
        DataCount = $"Total acte consecutive: {PrescriptionsData.Count}";
    }

    private async Task LoadMonitoringData()
    {
        System.Diagnostics.Debug.WriteLine("📊 Loading Monitoring data...");
        DataCount = "Monitorizări vor fi implementate în versiunea viitoare";
        HasData = false;
        
        await Shell.Current.DisplayAlert("Info", 
            "Monitorizările sunt încă în dezvoltare.\nAceastă funcționalitate va fi disponibilă în curând.", 
            "OK");
    }

    [RelayCommand]
    private async Task SaveAsPdfAsync()
    {
        if (SelectedReportType == null) return;

        await GenerateReportAsync(SelectedReportType);
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        System.Diagnostics.Debug.WriteLine("🔄 RefreshDataAsync called");
        
        if (SelectedReportType == null)
        {
            await Shell.Current.DisplayAlert("Info", 
                "Selectați mai întâi un tip de raport folosind butonul 'Încarcă Date'.", 
                "OK");
            return;
        }
        
        await LoadReportDataAsync();
    }

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
                
                var openFile = await Shell.Current.DisplayAlert(
                    "Succes", 
                    $"PDF-ul a fost salvat cu succes!\nLocație: {filePath}\n\nDoriți să deschideți fișierul?", 
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
                    $"Eroare la generarea PDF-ului: {ex.Message}", "OK");
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
            
            if (!File.Exists(filePath))
            {
                await Shell.Current.DisplayAlert("Eroare", "Fișierul nu a fost găsit.", "OK");
                return;
            }

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
                    
                    await Shell.Current.DisplayAlert("Info", 
                        $"PDF-ul a fost salvat la:\n{filePath}\n\nNu s-a putut deschide automat.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TryOpenFileAsync general error: {ex}");
            await Shell.Current.DisplayAlert("Avertizare", 
                $"PDF-ul a fost generat, dar nu a putut fi deschis automat.\nLocație: {filePath}", "OK");
        }
    }

    [RelayCommand]
    private void ResetFilters()
    {
        System.Diagnostics.Debug.WriteLine("🔄 ResetFilters called");
        StartDate = DateTime.Today.AddDays(-30);
        EndDate = DateTime.Today;
        PatientFilter = string.Empty;
        
        if (SelectedReportType != null)
        {
            _ = LoadReportDataAsync();
        }
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
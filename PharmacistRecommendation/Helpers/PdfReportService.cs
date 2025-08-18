using DTO;
using Entities.Services.Interfaces;
using Entities.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using Colors = QuestPDF.Helpers.Colors;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace PharmacistRecommendation.Helpers;

public class PdfReportService : IPdfReportService
{
    private readonly IMonitoringService _monitoringService;
    private readonly IPatientService _patientService;
    private readonly IPrescriptionService _prescriptionService;

    public PdfReportService(IMonitoringService svc, IPatientService patientSvc, IPrescriptionService prescriptionSvc)
    {
        _monitoringService = svc;
        _patientService = patientSvc;
        _prescriptionService = prescriptionSvc;
    }

    public async Task<string> CreatePatientReportAsync(int patientId, DateTime from, DateTime to)
    {
        System.Diagnostics.Debug.WriteLine("=== CreatePatientReportAsync: Starting ===");
        
        try
        {
            System.Diagnostics.Debug.WriteLine("Step 1: Getting patient data");
            var p = await _patientService.GetByIdAsync(patientId);
            if (p == null)
                throw new ArgumentException($"Patient with ID {patientId} not found");

            System.Diagnostics.Debug.WriteLine("Step 2: Preparing patient info");
            string patientName = $"{p.FirstName} {p.LastName}";
            string patientCnp = p.Cnp ?? "—";
            string patientCid = p.Cid ?? "—";
            
            System.Diagnostics.Debug.WriteLine("Step 3: Getting monitoring history");
            var rows = (await _monitoringService.GetHistoryAsync(patientId, from, to))
                       .OrderBy(r => r.Date)
                       .ToList();

            System.Diagnostics.Debug.WriteLine("Step 4: Setting up output directory");
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var reportsFolder = Path.Combine(folder, "PharmacistReports");
            Directory.CreateDirectory(reportsFolder);
            
            var filePath = Path.Combine(reportsFolder,
                $"Raport_Pacient_{patientId}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

            System.Diagnostics.Debug.WriteLine($"Step 5: Output path: {filePath}");

            System.Diagnostics.Debug.WriteLine("Step 6: Setting QuestPDF license");
            QuestPDF.Settings.License = LicenseType.Community;
            
            System.Diagnostics.Debug.WriteLine("Step 7: Starting PDF document creation");
            
            // Try a minimal PDF first to isolate the issue
            try
            {
                System.Diagnostics.Debug.WriteLine("Step 7a: Creating minimal document");
                var document = QuestPDF.Fluent.Document.Create(container =>
                {
                    System.Diagnostics.Debug.WriteLine("Step 7b: Inside document creation");
                    container.Page(page =>
                    {
                        System.Diagnostics.Debug.WriteLine("Step 7c: Setting page properties");
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        System.Diagnostics.Debug.WriteLine("Step 7d: Adding content");
                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                System.Diagnostics.Debug.WriteLine("Step 7e: Adding title");
                                x.Spacing(20);
                                x.Item().Text("Test Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                                x.Item().Text($"Patient: {patientName}");
                                x.Item().Text($"Period: {from:dd.MM.yyyy} - {to:dd.MM.yyyy}");
                                x.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                                
                                if (!rows.Any())
                                {
                                    x.Item().Text("No monitoring data available for this period.");
                                }
                                else
                                {
                                    x.Item().Text($"Found {rows.Count} monitoring records.");
                                }
                            });
                    });
                });

                System.Diagnostics.Debug.WriteLine("Step 8: Generating PDF to file");
                document.GeneratePdf(filePath);
                System.Diagnostics.Debug.WriteLine("Step 9: PDF generation completed successfully");
            }
            catch (Exception pdfEx)
            {
                System.Diagnostics.Debug.WriteLine($"PDF Generation Error: {pdfEx}");
                throw new Exception($"PDF generation failed: {pdfEx.Message}", pdfEx);
            }

            System.Diagnostics.Debug.WriteLine($"=== CreatePatientReportAsync: Completed Successfully, file: {filePath} ===");
            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== CreatePatientReportAsync: FAILED with error: {ex} ===");
            throw new Exception($"Error generating patient report: {ex.Message}", ex);
        }
    }

    public async Task<string> CreateMixedActsReportAsync(DateTime startDate, DateTime endDate, string patientFilter)
    {
        System.Diagnostics.Debug.WriteLine("=== CreateMixedActsReportAsync: Starting ===");
        
        try
        {
            System.Diagnostics.Debug.WriteLine("Step 1: Getting prescriptions data");
            var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
            System.Diagnostics.Debug.WriteLine($"Step 2: Found {prescriptions.Count} total prescriptions");
            
            // Filter by date range and patient filter
            var filteredPrescriptions = prescriptions
                .Where(p => p.IssueDate >= startDate && p.IssueDate <= endDate)
                .Where(p => string.IsNullOrEmpty(patientFilter) || 
                           (p.PatientName?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                           (p.PatientCnp?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true))
                .Where(p => p.PrescriptionMedications.Any(m => m.IsWithPrescription == true) && 
                           p.PrescriptionMedications.Any(m => m.IsWithPrescription == false))
                .OrderBy(p => p.IssueDate)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Step 3: Filtered to {filteredPrescriptions.Count} mixed prescriptions");
            return await CreateActsReport(filteredPrescriptions, "Raport Acte Mixte", startDate, endDate, "Mixed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== CreateMixedActsReportAsync: FAILED with error: {ex} ===");
            throw new Exception($"Error generating mixed acts report: {ex.Message}", ex);
        }
    }

    public async Task<string> CreateOwnActsReportAsync(DateTime startDate, DateTime endDate, string patientFilter)
    {
        System.Diagnostics.Debug.WriteLine("=== CreateOwnActsReportAsync: Starting ===");
        
        try
        {
            var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
            
            var filteredPrescriptions = prescriptions
                .Where(p => p.IssueDate >= startDate && p.IssueDate <= endDate)
                .Where(p => string.IsNullOrEmpty(patientFilter) || 
                           (p.PatientName?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                           (p.PatientCnp?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true))
                .Where(p => p.PrescriptionMedications.Any() && p.PrescriptionMedications.All(m => m.IsWithPrescription == false))
                .OrderBy(p => p.IssueDate)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Filtered to {filteredPrescriptions.Count} own acts prescriptions");
            return await CreateActsReport(filteredPrescriptions, "Raport Acte Proprii", startDate, endDate, "Own");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== CreateOwnActsReportAsync: FAILED with error: {ex} ===");
            throw new Exception($"Error generating own acts report: {ex.Message}", ex);
        }
    }

    public async Task<string> CreateConsecutivePrescriptionActsReportAsync(DateTime startDate, DateTime endDate, string patientFilter)
    {
        System.Diagnostics.Debug.WriteLine("=== CreateConsecutivePrescriptionActsReportAsync: Starting ===");
        
        try
        {
            var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
            
            var filteredPrescriptions = prescriptions
                .Where(p => p.IssueDate >= startDate && p.IssueDate <= endDate)
                .Where(p => string.IsNullOrEmpty(patientFilter) || 
                           (p.PatientName?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                           (p.PatientCnp?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true))
                .Where(p => p.PrescriptionMedications.Any(m => m.IsWithPrescription == true))
                .OrderBy(p => p.IssueDate)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Filtered to {filteredPrescriptions.Count} consecutive prescriptions");
            return await CreateActsReport(filteredPrescriptions, "Raport Acte Consecutive Prescripției", startDate, endDate, "Consecutive");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== CreateConsecutivePrescriptionActsReportAsync: FAILED with error: {ex} ===");
            throw new Exception($"Error generating consecutive prescription acts report: {ex.Message}", ex);
        }
    }

    public async Task<string> CreateMonitoringListReportAsync(DateTime startDate, DateTime endDate, string patientFilter)
    {
        System.Diagnostics.Debug.WriteLine("=== CreateMonitoringListReportAsync: Starting ===");
        
        try
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var reportsFolder = Path.Combine(folder, "PharmacistReports");
            Directory.CreateDirectory(reportsFolder);
            
            var filePath = Path.Combine(reportsFolder, $"Raport_Monitorizari_{DateTime.Now:yyyyMMddHHmmss}.pdf");

            System.Diagnostics.Debug.WriteLine("Setting QuestPDF license for monitoring report");
            QuestPDF.Settings.License = LicenseType.Community;

            System.Diagnostics.Debug.WriteLine("Creating monitoring report document");
            QuestPDF.Fluent.Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    
                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text("Lista Monitorizări").FontSize(18).SemiBold();
                        col.Item().Text($"Perioadă: {startDate:dd.MM.yyyy} – {endDate:dd.MM.yyyy}");
                        
                        if (!string.IsNullOrEmpty(patientFilter))
                        {
                            col.Item().Text($"Filtru pacient: {patientFilter}");
                        }

                        col.Item().PaddingTop(10).Text("Acest raport va conține statistici detaliate despre monitorizările efectuate în perioada selectată.");
                        col.Item().PaddingTop(20).Text("Funcționalitatea va fi completată cu date din serviciul de monitorizare.").FontSize(10).Italic();
                    });
                });
            }).GeneratePdf(filePath);

            System.Diagnostics.Debug.WriteLine($"=== CreateMonitoringListReportAsync: Completed Successfully ===");
            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== CreateMonitoringListReportAsync: FAILED with error: {ex} ===");
            throw new Exception($"Error generating monitoring list report: {ex.Message}", ex);
        }
    }

    private async Task<string> CreateActsReport(IEnumerable<Prescription> prescriptions, string reportTitle, DateTime startDate, DateTime endDate, string reportType)
    {
        System.Diagnostics.Debug.WriteLine($"=== CreateActsReport: Starting {reportType} ===");
        
        try
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var reportsFolder = Path.Combine(folder, "PharmacistReports");
            Directory.CreateDirectory(reportsFolder);
            
            var filePath = Path.Combine(reportsFolder, $"Raport_{reportType}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

            System.Diagnostics.Debug.WriteLine($"Setting up PDF for {reportType}, path: {filePath}");
            QuestPDF.Settings.License = LicenseType.Community;

            System.Diagnostics.Debug.WriteLine("Creating acts report document");
            QuestPDF.Fluent.Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    
                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text(reportTitle).FontSize(18).SemiBold();
                        col.Item().Text($"Perioadă: {startDate:dd.MM.yyyy} – {endDate:dd.MM.yyyy}");
                        col.Item().Text($"Total acte: {prescriptions.Count()}");

                        if (prescriptions.Any())
                        {
                            col.Item().Text("Date găsite - tabelul va fi implementat în versiunea următoare.");
                        }
                        else
                        {
                            col.Item().PaddingTop(20).Text("Nu au fost găsite acte în perioada selectată.").FontSize(12);
                        }
                    });
                });
            }).GeneratePdf(filePath);

            System.Diagnostics.Debug.WriteLine($"=== CreateActsReport: Completed Successfully {reportType} ===");
            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== CreateActsReport: FAILED {reportType} with error: {ex} ===");
            throw new Exception($"Error creating acts report: {ex.Message}", ex);
        }
    }
}

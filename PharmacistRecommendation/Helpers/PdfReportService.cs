using DTO;
using Entities.Services.Interfaces;
using Entities.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using Colors = QuestPDF.Helpers.Colors;
using IContainer = QuestPDF.Infrastructure.IContainer;
using Document = QuestPDF.Fluent.Document;
using SD = System.Drawing;
using System.Drawing;
using System.Drawing.Printing;
using Entities.Services;

namespace PharmacistRecommendation.Helpers;

public class PdfReportService : IPdfReportService
{
    private readonly IMonitoringService _monitoringService;
    private readonly IPatientService _patientService;
    private readonly IPrescriptionService _prescriptionService;
    private readonly IUserService _userService;

    public PdfReportService(IMonitoringService svc, IPatientService patientSvc, IPrescriptionService prescriptionSvc, IUserService userService)
    {
        _monitoringService = svc;
        _patientService = patientSvc;
        _prescriptionService = prescriptionSvc;
        _userService = userService;
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

            var filteredPrescriptions = prescriptions
                .Where(p => p.IssueDate >= startDate && p.IssueDate <= endDate)
                .Where(p => string.IsNullOrEmpty(patientFilter) ||
                           (p.PatientName?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                           (p.PatientCnp?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                           (p.Patient?.PharmacyCards.Any(c => c.Code?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true) == true))
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
                           (p.PatientCnp?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                           (p.Patient?.PharmacyCards.Any(c => c.Code?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true) == true))
                .Where(p => !p.PrescriptionMedications.Any() || !p.PrescriptionMedications.All(m => m.IsWithPrescription == true))
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
                           (p.PatientCnp?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                           (p.Patient?.PharmacyCards.Any(c => c.Code?.Contains(patientFilter, StringComparison.OrdinalIgnoreCase) == true) == true))
                .Where(p => p.PrescriptionMedications.Any() && p.PrescriptionMedications.All(m => m.IsWithPrescription == true))
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
                            col.Item().Text("Date găsite - tabelul va fi implementat în versiunea următore.");
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

    //Printare monitorizare

    public async Task<PrintDocument> CreateMonitoringPatientReportAsync(int patientId, DateTime from, DateTime to)
    {
        try
        {
            var p = await _patientService.GetByIdAsync(patientId);
            string patientName = $"{p?.FirstName ?? ""} {p?.LastName ?? ""}".Trim();
            if (string.IsNullOrWhiteSpace(patientName)) patientName = "-";
            string safePatientName = string.Join("_", patientName.Split(Path.GetInvalidFileNameChars()));
            string patientCnp = string.IsNullOrWhiteSpace(p?.Cnp) ? "—" : p.Cnp!;
            string patientCid = string.IsNullOrWhiteSpace(p?.Cid) ? "—" : p.Cid!;
            string patientCard = p?.PharmacyCards?.FirstOrDefault()?.Code ?? "—";

            var rows = (await _monitoringService.GetHistoryAsync(patientId, from, to))
                       .OrderBy(r => r.Date)
                       .ToList();

            var charts = new Dictionary<string, byte[]>();
            void AddChart(string key, byte[]? bytes) { if (bytes is { Length: > 0 }) charts[key] = bytes; }

            AddChart("hta", PlotDualLine(rows, r => r.MaxBloodPressure, r => r.MinBloodPressure, "Tensiune arterială (mmHg)", "Sist.", "Diast."));
            AddChart("puls", PlotLine(rows, r => r.HeartRate, "Puls bpm"));
            AddChart("spo2", PlotLine(rows, r => r.PulseOximetry, "SpO₂ %"));
            AddChart("gly", PlotLine(rows, r => r.BloodGlucose, "Glicemie mg/dL"));
            AddChart("temp", PlotLine(rows, r => r.BodyTemperature, "Temperatură °C"));

            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RaportPDFs");
            Directory.CreateDirectory(folder);
            var filePath = Path.Combine(folder, $"Raport_{safePatientName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            UserDTO? effectivePharmacist = null;
            string? assistantName = null;

            if (SessionManager.CurrentUser?.Role?.ToLower() == "pharmacist")
            {
                effectivePharmacist = SessionManager.CurrentUser;
            }
            else
            {
                effectivePharmacist = await _userService.GetEffectivePharmacistAsync(SessionManager.CurrentUser);
                assistantName = $"{SessionManager.CurrentUser?.FirstName ?? "-"} {SessionManager.CurrentUser?.LastName ?? "-"}";
            }

            string footerPharmacist = $"{effectivePharmacist?.FirstName ?? "-"} {effectivePharmacist?.LastName ?? "-"}";
            string footerAssistant = assistantName ?? "";

            using var printDoc = new PrintDocument();

            printDoc.PrintPage += (sender, e) =>
            {
                var g = e.Graphics;
                float margin = 50;
                float left = margin, top = margin;
                float lineHeight = 18;

                using var fontTitle = new SD.Font("Arial", 16, FontStyle.Bold);
                using var headerFont = new SD.Font("Arial", 9, FontStyle.Bold);
                using var fontText = new SD.Font("Arial", 10);
                using var fontSmall = new SD.Font("Arial", 7);
                using var fontSection = new SD.Font("Arial", 12, FontStyle.Bold);

                // Titlu document
                g.DrawString("Raport monitorizare", fontTitle, Brushes.Black, left, top);
                top += lineHeight * 2;

                // Info pacient
                g.DrawString($"Perioada: {from:dd.MM.yyyy} – {to:dd.MM.yyyy}", fontText, Brushes.Black, left, top);
                top += lineHeight;
                g.DrawString($"Pacient: {patientName}", fontText, Brushes.Black, left, top);
                g.DrawString($"CNP: {patientCnp}", fontText, Brushes.Black, left + 300, top);
                g.DrawString($"CID: {patientCid}", fontText, Brushes.Black, left + 450, top);
                top += lineHeight;
                g.DrawString($"Card farmacie: {patientCard}", fontText, Brushes.Black, left, top);
                top += lineHeight * 2;

                // Tabel date (simplificat aici)
                float col1 = left, col2 = left + 60, col3 = left + 120, col4 = left + 180, col5 = left + 240,
                      col6 = left + 300, col7 = left + 360, col8 = left + 420, col9 = left + 480;

                g.DrawString("Data", headerFont, Brushes.Black, col1, top);
                g.DrawString("TA↑", headerFont, Brushes.Black, col2, top);
                g.DrawString("TA↓", headerFont, Brushes.Black, col3, top);
                g.DrawString("Puls", headerFont, Brushes.Black, col4, top);
                g.DrawString("SpO₂", headerFont, Brushes.Black, col5, top);
                g.DrawString("Glic.", headerFont, Brushes.Black, col6, top);
                g.DrawString("Temp.", headerFont, Brushes.Black, col7, top);
                g.DrawString("Kg", headerFont, Brushes.Black, col8, top);
                g.DrawString("Cm", headerFont, Brushes.Black, col9, top);
                top += lineHeight;

                foreach (var r in rows)
                {
                    g.DrawString(r.Date.ToString("dd.MM"), fontText, Brushes.Black, col1, top);
                    g.DrawString(r.MaxBloodPressure?.ToString() ?? "—", fontText, Brushes.Black, col2, top);
                    g.DrawString(r.MinBloodPressure?.ToString() ?? "—", fontText, Brushes.Black, col3, top);
                    g.DrawString(r.HeartRate?.ToString() ?? "—", fontText, Brushes.Black, col4, top);
                    g.DrawString(r.PulseOximetry?.ToString() ?? "—", fontText, Brushes.Black, col5, top);
                    g.DrawString(r.BloodGlucose?.ToString() ?? "—", fontText, Brushes.Black, col6, top);
                    g.DrawString(r.BodyTemperature?.ToString() ?? "—", fontText, Brushes.Black, col7, top);
                    g.DrawString(r.Weight?.ToString() ?? "—", fontText, Brushes.Black, col8, top);
                    g.DrawString(r.Height?.ToString() ?? "—", fontText, Brushes.Black, col9, top);
                    top += lineHeight;
                }

                // Grafice (simplificat)
                float chartTop = top + 20;
                float chartWidth = 350;
                float chartHeight = 200;
                float chartLeft = left;

                foreach (var key in new[] { "hta", "puls", "spo2", "gly", "temp" })
                {
                    if (charts.TryGetValue(key, out var bytes))
                    {
                        using var ms = new MemoryStream(bytes);
                        using var img = SD.Image.FromStream(ms);
                        g.DrawImage(img, chartLeft, chartTop, chartWidth, chartHeight);

                        chartLeft += chartWidth + 10;
                        if (chartLeft + chartWidth > e.PageBounds.Width - margin)
                        {
                            chartLeft = left;
                            chartTop += chartHeight + 20;
                        }
                    }
                }

                // --- FOOTER ---
                float footerY = e.PageBounds.Height - margin - 50;

                g.DrawString($"Data: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", fontSmall, Brushes.Black, left, footerY);

                g.DrawString($"Farmacist: {footerPharmacist}", fontSmall, Brushes.Black, left, footerY + 15);

                if (!string.IsNullOrEmpty(footerAssistant))
                    g.DrawString($"Asistent: {footerAssistant}", fontSmall, Brushes.Black, left, footerY + 30);

                g.DrawString("Semnătură: ______________________", fontSmall, Brushes.Black, left, footerY + 45);

                g.DrawString("Document generat cu Recomandarea Farmacistului", fontSmall, Brushes.Gray, left, footerY + 60);

            };

            return printDoc;
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Eroare",
                $"Raportul nu a putut fi generat: {ex.Message}",
                "OK");

            return new PrintDocument(); 
        }
    }

    public async Task<string> CreateMonitoringPatientReportEmailAsync(int patientId, DateTime from, DateTime to)
    {
        var p = await _patientService.GetByIdAsync(patientId);
        string patientName = $"{p?.FirstName ?? ""} {p?.LastName ?? ""}".Trim();
        if (string.IsNullOrWhiteSpace(patientName)) patientName = "-";
        string safePatientName = string.Join("_", patientName.Split(Path.GetInvalidFileNameChars()));
        string patientCnp = string.IsNullOrWhiteSpace(p?.Cnp) ? "—" : p.Cnp!;
        string patientCid = string.IsNullOrWhiteSpace(p?.Cid) ? "—" : p.Cid!;
        string patientCard = p?.PharmacyCards?.FirstOrDefault()?.Code ?? "—";

        var rows = (await _monitoringService.GetHistoryAsync(patientId, from, to))
                   .OrderBy(r => r.Date)
                   .ToList();

        var charts = new Dictionary<string, byte[]>();
        void AddChart(string key, byte[]? bytes) { if (bytes is { Length: > 0 }) charts[key] = bytes; }

        AddChart("hta", PlotDualLine(rows, r => r.MaxBloodPressure, r => r.MinBloodPressure, "Tensiune arterială (mmHg)", "Sist.", "Diast."));
        AddChart("puls", PlotLine(rows, r => r.HeartRate, "Puls bpm"));
        AddChart("spo2", PlotLine(rows, r => r.PulseOximetry, "SpO₂ %"));
        AddChart("gly", PlotLine(rows, r => r.BloodGlucose, "Glicemie mg/dL"));
        AddChart("temp", PlotLine(rows, r => r.BodyTemperature, "Temperatură °C"));

        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RaportPDFs");
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, $"Raport_{safePatientName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

        UserDTO? effectivePharmacist = null;
        string? assistantName = null;

        if (SessionManager.CurrentUser?.Role?.ToLower() == "pharmacist")
        {
            effectivePharmacist = SessionManager.CurrentUser;
        }
        else
        {
            effectivePharmacist = await _userService.GetEffectivePharmacistAsync(SessionManager.CurrentUser);
            assistantName = $"{SessionManager.CurrentUser?.FirstName} {SessionManager.CurrentUser?.LastName}".Trim();
        }

        string footerPharmacist = $"{effectivePharmacist?.FirstName ?? "-"} {effectivePharmacist?.LastName ?? "-"}";
        string footerAssistant = assistantName ?? "";

        using var printDoc = new PrintDocument();

        printDoc.PrintPage += (sender, e) =>
        {
            var g = e.Graphics;
            float margin = 50;
            float left = margin, top = margin;
            float lineHeight = 18;

            using var fontTitle = new SD.Font("Arial", 16, FontStyle.Bold);
            using var headerFont = new SD.Font("Arial", 9, FontStyle.Bold);
            using var fontText = new SD.Font("Arial", 10);
            using var fontSmall = new SD.Font("Arial", 7);
            using var fontSection = new SD.Font("Arial", 12, FontStyle.Bold);

            // Titlu document
            g.DrawString("Raport monitorizare", fontTitle, Brushes.Black, left, top);
            top += lineHeight * 2;

            // Info pacient
            g.DrawString($"Perioada: {from:dd.MM.yyyy} – {to:dd.MM.yyyy}", fontText, Brushes.Black, left, top);
            top += lineHeight;
            g.DrawString($"Pacient: {patientName}", fontText, Brushes.Black, left, top);
            g.DrawString($"CNP: {patientCnp}", fontText, Brushes.Black, left + 300, top);
            g.DrawString($"CID: {patientCid}", fontText, Brushes.Black, left + 450, top);
            top += lineHeight;
            g.DrawString($"Card farmacie: {patientCard}", fontText, Brushes.Black, left, top);
            top += lineHeight * 2;

            // Tabel date (simplificat aici)
            float col1 = left, col2 = left + 60, col3 = left + 120, col4 = left + 180, col5 = left + 240,
                  col6 = left + 300, col7 = left + 360, col8 = left + 420, col9 = left + 480;

            g.DrawString("Data", headerFont, Brushes.Black, col1, top);
            g.DrawString("TA↑", headerFont, Brushes.Black, col2, top);
            g.DrawString("TA↓", headerFont, Brushes.Black, col3, top);
            g.DrawString("Puls", headerFont, Brushes.Black, col4, top);
            g.DrawString("SpO₂", headerFont, Brushes.Black, col5, top);
            g.DrawString("Glic.", headerFont, Brushes.Black, col6, top);
            g.DrawString("Temp.", headerFont, Brushes.Black, col7, top);
            g.DrawString("Kg", headerFont, Brushes.Black, col8, top);
            g.DrawString("Cm", headerFont, Brushes.Black, col9, top);
            top += lineHeight;

            foreach (var r in rows)
            {
                g.DrawString(r.Date.ToString("dd.MM"), fontText, Brushes.Black, col1, top);
                g.DrawString(r.MaxBloodPressure?.ToString() ?? "—", fontText, Brushes.Black, col2, top);
                g.DrawString(r.MinBloodPressure?.ToString() ?? "—", fontText, Brushes.Black, col3, top);
                g.DrawString(r.HeartRate?.ToString() ?? "—", fontText, Brushes.Black, col4, top);
                g.DrawString(r.PulseOximetry?.ToString() ?? "—", fontText, Brushes.Black, col5, top);
                g.DrawString(r.BloodGlucose?.ToString() ?? "—", fontText, Brushes.Black, col6, top);
                g.DrawString(r.BodyTemperature?.ToString() ?? "—", fontText, Brushes.Black, col7, top);
                g.DrawString(r.Weight?.ToString() ?? "—", fontText, Brushes.Black, col8, top);
                g.DrawString(r.Height?.ToString() ?? "—", fontText, Brushes.Black, col9, top);
                top += lineHeight;
            }

            // Grafice (simplificat)
            float chartTop = top + 20;
            float chartWidth = 350;
            float chartHeight = 200;
            float chartLeft = left;

            foreach (var key in new[] { "hta", "puls", "spo2", "gly", "temp" })
            {
                if (charts.TryGetValue(key, out var bytes))
                {
                    using var ms = new MemoryStream(bytes);
                    using var img = SD.Image.FromStream(ms);
                    g.DrawImage(img, chartLeft, chartTop, chartWidth, chartHeight);

                    chartLeft += chartWidth + 10;
                    if (chartLeft + chartWidth > e.PageBounds.Width - margin)
                    {
                        chartLeft = left;
                        chartTop += chartHeight + 20;
                    }
                }
            }

            // --- FOOTER ---
            float footerY = e.PageBounds.Height - margin - 50;

            g.DrawString($"Data: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", fontSmall, Brushes.Black, left, footerY);

            g.DrawString($"Farmacist: {footerPharmacist.ToUpper()}", fontSmall, Brushes.Black, left, footerY + 15);

            if (!string.IsNullOrEmpty(footerAssistant))
                g.DrawString($"Asistent: {footerAssistant.ToUpper()}", fontSmall, Brushes.Black, left, footerY + 30);

            g.DrawString("Semnătură: ______________________", fontSmall, Brushes.Black, left, footerY + 45);

            g.DrawString("Document generat cu Recomandarea Farmacistului", fontSmall, Brushes.Gray, left, footerY + 60);

        };

        var pd = new System.Windows.Forms.PrintDialog();
        pd.Document = printDoc;
        pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
        pd.PrinterSettings.PrintFileName = filePath;
        pd.PrinterSettings.PrintToFile = true;

        printDoc.Print();

        return filePath;
    }


    private static byte[]? PlotLine<T>(IEnumerable<HistoryRowDto> data,
                                     Func<HistoryRowDto, T> selector,
                                     string yLabel)
    {
        var points = data.Select(r =>
        {
            double y;
            var v = selector(r);

            y = v switch
            {
                ValueTuple<int?, int?> tup => tup.Item1 ?? double.NaN,
                null => double.NaN,
                _ => Convert.ToDouble(v)
            };

            return (x: r.Date.ToOADate(), y);
        })
        .Where(p => !double.IsNaN(p.y))
        .ToList();

        if (points.Count == 0) return null;

        var xs = points.Select(p => p.x).ToArray();
        var ys = points.Select(p => p.y).ToArray();

        var plt = new ScottPlot.Plot(600, 400);

        var scatter = plt.AddScatter(xs, ys);
        scatter.MarkerShape = ScottPlot.MarkerShape.filledCircle;
        scatter.MarkerSize = 8;
        scatter.LineWidth = 2;

        plt.XAxis.DateTimeFormat(true);
        plt.Title(yLabel);

        return plt.GetImageBytes();
    }

    private static byte[]? PlotDualLine(
     IEnumerable<HistoryRowDto> data,
     Func<HistoryRowDto, int?> selector1,
     Func<HistoryRowDto, int?> selector2,
     string title,
     string label1,
     string label2)
    {
        var points1 = data
            .Where(r => selector1(r).HasValue)
            .Select(r => (x: r.Date.ToOADate(), y: (double)selector1(r).Value))
            .ToList();

        var points2 = data
            .Where(r => selector2(r).HasValue)
            .Select(r => (x: r.Date.ToOADate(), y: (double)selector2(r).Value))
            .ToList();

        if (!points1.Any() && !points2.Any())
            return null;

        var plt = new ScottPlot.Plot(600, 400);

        if (points1.Any())
            plt.AddScatter(points1.Select(p => p.x).ToArray(), points1.Select(p => p.y).ToArray(), label: label1, lineWidth: 2);

        if (points2.Any())
            plt.AddScatter(points2.Select(p => p.x).ToArray(), points2.Select(p => p.y).ToArray(), label: label2, lineWidth: 3, markerSize: 6, markerShape: ScottPlot.MarkerShape.filledCircle);

        plt.XAxis.DateTimeFormat(true);
        plt.Title(title);
        plt.Legend();
        plt.SetAxisLimits(yMin: 50);

        return plt.GetImageBytes();
    }

}

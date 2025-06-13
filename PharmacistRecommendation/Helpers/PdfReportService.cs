using DTO;
using Entities.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScottPlot;
using System.Text.Json;
using Colors = QuestPDF.Helpers.Colors;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace PharmacistRecommendation.Helpers;

public class PdfReportService : IPdfReportService
{
    private readonly IMonitoringService _monitoringService;
    private readonly IPatientService _patientService;
    public PdfReportService(IMonitoringService svc, IPatientService parientSvc)
    {
        _monitoringService = svc;
        _patientService = parientSvc;
    }

    public async Task<string> CreatePatientReportAsync(int patientId, DateTime from, DateTime to)
    {
        var p = await _patientService.GetByIdAsync(patientId);
        string patientName = $"{p.FirstName} {p.LastName}";
        string patientCnp = p.Cnp ?? "—";
        string patientCid = p.Cid ?? "—";
        var rows = (await _monitoringService.GetHistoryAsync(patientId, from, to))
                   .OrderBy(r => r.Date)
                   .ToList();

        var charts = new Dictionary<string, byte[]>();

        void Add(string key, byte[]? bytes)
        {
            if (bytes is { Length: > 0 })
                charts[key] = bytes;            
        }

        Add("hta", PlotLine(rows, r => (r.MaxBloodPressure, r.MinBloodPressure), "TA mmHg"));
        Add("puls", PlotLine(rows, r => r.HeartRate, "Puls bpm"));
        Add("spo2", PlotLine(rows, r => r.PulseOximetry, "SpO₂ %"));
        Add("gly", PlotLine(rows, r => r.BloodGlucose, "Glicemie mg/dL"));
        Add("temp", PlotLine(rows, r => r.BodyTemperature, "Temperatură °C"));

        // 3️⃣  Output PDF în Documents
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder,
            $"Raport_{patientId}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Margin(20);

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text($"Raport monitorizare")
                              .FontSize(18).Bold();

                    col.Item().Text($"Perioadă: {from:dd.MM.yyyy} – {to:dd.MM.yyyy}");
                    col.Item().Text(txt =>
                    {
                        txt.Span("Pacient: ").SemiBold();
                        txt.Span($"{patientName}   ");          // ex: „Ion Popescu”
                        txt.Span("CNP: ").SemiBold();
                        txt.Span($"{patientCnp}   ");
                        txt.Span("CID: ").SemiBold();
                        txt.Span($"{patientCid}");
                    });
                    // ─── TABEL ───
                    col.Item().PaddingTop(10).Table(tab =>
                    {
                        tab.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(60); c.ConstantColumn(45); c.ConstantColumn(45);
                            c.ConstantColumn(40); c.ConstantColumn(40); c.ConstantColumn(55);
                            c.ConstantColumn(55); c.ConstantColumn(50); c.ConstantColumn(50);
                        });

                        void Head(string t) =>
                            tab.Cell().Element(h => h
                                    .Background(Colors.Grey.Lighten3)
                                    .Padding(2)
                                    .DefaultTextStyle(s => s.SemiBold())
                                    .AlignCenter())
                               .Text(t);

                        Head("Data"); Head("TA↑"); Head("TA↓"); Head("Puls");
                        Head("SpO₂"); Head("Glic."); Head("Temp."); Head("Kg"); Head("Cm");

                        void Cell(object? v) => tab.Cell().Padding(2)
                                                     .AlignCenter()
                                                     .Text(v?.ToString() ?? "—");

                        foreach (var r in rows)
                        {
                            Cell(r.Date.ToString("dd.MM"));
                            Cell(r.MaxBloodPressure);
                            Cell(r.MinBloodPressure);
                            Cell(r.HeartRate);
                            Cell(r.PulseOximetry);
                            Cell(r.BloodGlucose);
                            Cell(r.BodyTemperature);
                            Cell(r.Weight);
                            Cell(r.Height);
                        }
                    });

                    // ─── GRAFICE (2 pe rând) ───
                    if (charts.Count > 0)
                    {
                        col.Item().PaddingTop(15).Column(imgCol =>
                        {
                            var order = new[] { "hta", "puls", "spo2", "gly", "temp" };
                            for (int i = 0; i < order.Length; i += 2)
                            {
                                imgCol.Item().Row(r =>
                                {
                                    AddChartCell(r, order[i]);
                                    if (i + 1 < order.Length)
                                        AddChartCell(r, order[i + 1]);

                                    void AddChartCell(RowDescriptor row, string k)
                                    {
                                        if (charts.TryGetValue(k, out var bytes))
                                            row.ConstantItem(260).Image(bytes);
                                        else
                                            row.ConstantItem(260)
                                               .Border(1)
                                               .AlignMiddle()
                                               .AlignCenter()
                                               .Text("Fără date")
                                               .FontSize(10);
                                    }
                                });
                            }
                        });
                    }
                });
            });
        })
        .GeneratePdf(filePath);

        return filePath;
    }

    // ───────── Grafic cu ScottPlot 4.x ─────────
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

        var plt = new ScottPlot.Plot(500, 300);
        plt.AddScatter(xs, ys);
        plt.XAxis.DateTimeFormat(true);
        plt.Title(yLabel);

        return plt.GetImageBytes();   // disponibil în v4
    }
}

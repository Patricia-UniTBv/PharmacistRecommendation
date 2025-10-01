using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;

namespace PharmacistRecommendation.Helpers
{
    public class ActPdfDocument
    {
        public string? PharmacyName { get; set; }
        public string? PharmacyAddress { get; set; }
        public string? PharmacyPhone { get; set; }
        public string? Series { get; set; }
        public string? Number { get; set; }
        public DateTime IssueDate { get; set; }
        public string? PatientName { get; set; }
        public string? PatientCnp { get; set; }
        public string? CaregiverName { get; set; }
        public string? CaregiverCnp { get; set; }
        public string? ModeCode { get; set; }
        public string? DoctorStamp { get; set; }
        public string? Diagnostic { get; set; }
        public string? DiagnosisMentioned { get; set; }
        public string? MedicationsMentioned { get; set; }
        public string? Symptoms { get; set; }
        public string? Suspicion { get; set; }
        public string? PharmacistObservations { get; set; }
        public string? NotesToDoctor { get; set; }
        public string? PharmacistRecommendation { get; set; }
        public string? PharmaceuticalService { get; set; }

        public List<MedicationLine> MedicationsWithPrescription { get; set; } = new();
        public List<MedicationLine> MedicationsWithoutPrescription { get; set; } = new();

        public class MedicationLine
        {
            public string? Name { get; set; }
            public string? Morning { get; set; }
            public string? Noon { get; set; }
            public string? Evening { get; set; }
            public string? Night { get; set; }
            public string? AdministrationMode { get; set; }
        }

        public byte[] GeneratePdf()
        {
            PdfDocument pdf = new PdfDocument();
            PdfPage page = pdf.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            XGraphics g = XGraphics.FromPdfPage(page);

            double left = 10;
            double top = 40;
            double contentWidth = page.Width.Point - left - 100;
            double y = top;

            GlobalFontSettings.FontResolver = new MauiFontResolver();

            XFont fontTitle = new XFont("OpenSans", 16, XFontStyleEx.Bold);
            XFont fontText = new XFont("OpenSans", 10, XFontStyleEx.Regular);
            XFont fontSection = new XFont("OpenSans", 12, XFontStyleEx.Bold);

            double lineHeight = Math.Max(1, fontText.GetHeight() * 1.2);

            try
            {
                var logo = XImage.FromFile("Resources/Images/farma.png");
                g.DrawImage(logo, left, y, 60, 60);
            }
            catch { }

            double textStartX = left + 90;

            string pageTitle = ModeCode switch
            {
                "AC" => "Act consecutiv prescripției",
                "AP" => "Act propriu",
                "AM" => "Act mixt",
                _ => "Document farmaceutic"
            };
            g.DrawString(pageTitle, fontTitle, XBrushes.Black,
                new XRect(textStartX, y, contentWidth - 90, 40), XStringFormats.TopLeft);
            y += 50;

            string code = $"{ModeCode}-{SessionManager.CurrentUser?.Ncm ?? "0000"}-{new Random().Next(1000, 9999):D4}";
            var codeSize = g.MeasureString(code, fontText);

            g.DrawString($"Data: {IssueDate:dd.MM.yyyy HH:mm}", fontText, XBrushes.Black, textStartX, y);
            g.DrawString(code, fontText, XBrushes.Black, page.Width.Point - 250, y);
            y += lineHeight;

            g.DrawString($"FARMACIST: {SessionManager.CurrentUser?.FirstName} {SessionManager.CurrentUser?.LastName}", fontText, XBrushes.Black, textStartX, y);
            y += lineHeight;

            g.DrawString($"FARMACIA: {PharmacyName}", fontText, XBrushes.Black, textStartX, y);
            y += lineHeight;
            g.DrawString($"ADRESA: {PharmacyAddress}", fontText, XBrushes.Black, textStartX, y);
            y += lineHeight;
            g.DrawString($"TELEFON: {PharmacyPhone}", fontText, XBrushes.Black, textStartX, y);
            y += lineHeight;

            double cnpOffset = 200;
            g.DrawString($"PACIENT: {PatientName}", fontText, XBrushes.Black, textStartX, y);
            g.DrawString($"CNP: {PatientCnp}", fontText, XBrushes.Black, textStartX + cnpOffset, y);
            y += lineHeight;

            g.DrawString($"APARȚINĂTOR: {CaregiverName}", fontText, XBrushes.Black, textStartX, y);
            g.DrawString($"CNP: {CaregiverCnp}", fontText, XBrushes.Black, textStartX + cnpOffset, y);
            y += lineHeight;

            void DrawWrappedText(string text)
            {
                if (string.IsNullOrEmpty(text)) return;
                XRect rect = new XRect(textStartX, y, contentWidth, page.Height.Point - y - 50);
                g.DrawString(text, fontText, XBrushes.Black, rect, XStringFormats.TopLeft);
                y += Math.Max(1, g.MeasureString(text, fontText).Height) + lineHeight / 2;
            }

            DrawWrappedText($"DIAGNOSTIC MENȚIONAT DE PACIENT: {DiagnosisMentioned}");
            DrawWrappedText($"MEDICAMENTE UTILIZATE DE PACIENT: {MedicationsMentioned}");

            if (ModeCode == "AC" || ModeCode == "AM")
            {
                double offset = 250;
                g.DrawString($"PARAFĂ MEDIC: {DoctorStamp}", fontText, XBrushes.Black, textStartX, y);
                g.DrawString($"SERIE/NUMĂR MEDIC: {Series}", fontText, XBrushes.Black, textStartX + offset, y);
                y += Math.Max(1, fontText.GetHeight() * 1.1);
                DrawWrappedText($"DIAGNOSTIC: {Diagnostic}");
            }

            DrawWrappedText($"SIMPTOMATOLOGIE: {Symptoms}");
            DrawWrappedText($"SUSPICIUNE: {Suspicion}");
            DrawWrappedText($"CONSTATĂRILE FARMACISTULUI: {PharmacistObservations}");
            y += 5;

            if (MedicationsWithPrescription.Count > 0)
            {
                y = DrawMedicationSection(g, textStartX, y, contentWidth, "MEDICAMENTE ELIBERATE CU REȚETĂ", MedicationsWithPrescription, fontSection, fontText);
                y += 10;
            }

            if (MedicationsWithoutPrescription.Count > 0)
            {
                y = DrawMedicationSection(g, textStartX, y, contentWidth, "MEDICAMENTE ELIBERATE FĂRĂ REȚETĂ", MedicationsWithoutPrescription, fontSection, fontText);
            }

            DrawWrappedText(NotesToDoctor ?? "");
            DrawWrappedText(PharmacistRecommendation ?? "");
            DrawWrappedText($"Serviciu farmaceutic: {PharmaceuticalService}");

            using var ms = new MemoryStream();
            pdf.Save(ms, false);
            return ms.ToArray();
        }

        private double DrawMedicationSection(XGraphics g, double left, double y, double width, string title,
                                     List<MedicationLine> meds, XFont sectionFont, XFont textFont)
        {
            XTextFormatter tf = new XTextFormatter(g);

            g.DrawString(title, sectionFont, XBrushes.Black, left, y);
            y += sectionFont.GetHeight() * 1.5;

            double[] colWidths = { 30, 200, 65, 65, 65, 90, 130 };
            double totalWidth = 0;
            foreach (var w in colWidths) totalWidth += w;
            double scale = totalWidth > width ? width / totalWidth : 1.0;
            for (int i = 0; i < colWidths.Length; i++) colWidths[i] = Math.Max(30, colWidths[i] * scale); // min width 30

            string[] headers = { "NR CRT", "MEDICAMENT", "DIMIN.", "PRÂNZ", "SEARA", "NOAPTEA", "MOD ADMIN" };

            double rowHeight = sectionFont.GetHeight() * 1.5;
            double x = left;
            for (int i = 0; i < headers.Length; i++)
            {
                g.DrawRectangle(XPens.Gray, x, y, colWidths[i], rowHeight);
                var rectHeader = new XRect(x + 2, y + 2, colWidths[i] - 4, rowHeight - 4);
                tf.DrawString(headers[i], sectionFont, XBrushes.Black, rectHeader, XStringFormats.TopLeft);
                x += colWidths[i];
            }
            y += rowHeight;

            for (int r = 0; r < meds.Count; r++)
            {
                x = left;
                var med = meds[r];
                string[] values = {
            (r + 1).ToString(),
            med.Name ?? "-",
            med.Morning ?? "-",
            med.Noon ?? "-",
            med.Evening ?? "-",
            med.Night ?? "-",
            med.AdministrationMode ?? "-"
        };

                double maxRowHeight = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    var rectMeasure = new XRect(0, 0, colWidths[i] - 4, 1000);
                    var size = g.MeasureString(values[i], textFont);
                    double h = Math.Ceiling(size.Height * Math.Max(1, values[i].Length * textFont.Size / colWidths[i]));
                    if (h > maxRowHeight) maxRowHeight = h;
                }
                maxRowHeight = Math.Max(maxRowHeight, rowHeight); 

                for (int i = 0; i < values.Length; i++)
                {
                    g.DrawRectangle(XPens.Gray, x, y, colWidths[i], maxRowHeight);
                    var rectText = new XRect(x + 2, y + 2, colWidths[i] - 4, maxRowHeight - 4);
                    tf.DrawString(values[i], textFont, XBrushes.Black, rectText, XStringFormats.TopLeft);
                    x += colWidths[i];
                }

                y += maxRowHeight;
            }

            return y + 5;
        }

    }
}

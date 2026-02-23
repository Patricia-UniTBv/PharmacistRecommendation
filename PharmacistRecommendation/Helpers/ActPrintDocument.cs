using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using SD = System.Drawing;

namespace PharmacistRecommendation.Helpers
{
    public class ActPrintDocument : PrintDocument
    {
        public string? PharmacyName { get; set; }
        public string? PharmacyAddress { get; set; }
        public string? PharmacyPhone { get; set; }
        public string? Logo { get; set; }
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
        public string? LoggedUserNcm { get; set; }
        public string? LoggedUserName { get; set; }

        public List<MedicationLine> MedicationsWithPrescription { get; set; } = new();
        public List<MedicationLine> MedicationsWithoutPrescription { get; set; } = new();

        public string PharmacistNameEffective { get; set; } = "-";
        public string? AssistantName { get; set; }


        public class MedicationLine
        {
            public string? Name { get; set; }
            public string? Morning { get; set; }
            public string? Noon { get; set; }
            public string? Evening { get; set; }
            public string? Night { get; set; }
            public string? AdministrationMode { get; set; }
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            try
            {
                SafePrint(e);
            }
            catch (Exception ex)
            {
                // log the error somewhere (optional)
                Debug.WriteLine("Print failed: " + ex);
                // optionally, show a friendly message:
                MessageBox.Show("An error occurred while printing. Please try again.",
                                "Printing Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SafePrint(PrintPageEventArgs e)
        {
            var g = e.Graphics;
            float margin = 50;
            float left = margin;
            float top = margin;
            float right = e.PageBounds.Width - margin;
            float bottom = e.PageBounds.Height - margin;
            float y = top;
            float contentWidth = right - left;

            using var fontTitle = SafeCreateFont("Arial", 16f, FontStyle.Bold);
            using var fontText = SafeCreateFont("Arial", 10f, FontStyle.Regular);
            using var fontSmall = SafeCreateFont("Arial", 7f, FontStyle.Regular);
            using var fontSection = SafeCreateFont("Arial", 12f, FontStyle.Bold);

            float lineHeight = fontText.GetHeight(g) * 1.5f;

            SafeDrawLogo(g, right, ref y);

            float textStartX = left;
            var format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

            string pageTitle = ModeCode switch
            {
                "AC" => "Act consecutiv prescripției",
                "AP" => "Act farmaceutic",
                "AM" => "Act mixt",
                _ => "Document farmaceutic"
            };

            SafeDrawString(g, pageTitle, fontTitle, Brushes.Black, textStartX, y);
            y += lineHeight * 2.5f;

            string code = $"{ModeCode}-{SessionManager.CurrentUser?.Ncm ?? "0000"}-{SafeGetNextSequentialNumber(ModeCode)}";
            SafeDrawString(g, $"Data: {IssueDate:dd.MM.yyyy HH:mm}", fontText, Brushes.Black, textStartX, y);
            SafeDrawString(g, code, fontText, Brushes.Black, right - 180, y);
            y += lineHeight;

            SafeDrawString(g, $"FARMACIST: {PharmacistNameEffective}", fontText, Brushes.Black, textStartX, y);
            y += lineHeight;

            SafeDrawString(g, $"FARMACIA: {PharmacyName}", fontText, Brushes.Black, textStartX, y);
            y += lineHeight;

            SafeDrawString(g, $"ADRESĂ: {PharmacyAddress}", fontText, Brushes.Black, textStartX, y);
            y += lineHeight;

            SafeDrawString(g, $"TELEFON: {PharmacyPhone}", fontText, Brushes.Black, textStartX, y);
            y += lineHeight;

            float cnpOffset = 350;
            SafeDrawString(g, $"PACIENT: {PatientName}", fontText, Brushes.Black, textStartX, y);
            SafeDrawString(g, $"CNP: {PatientCnp}", fontText, Brushes.Black, textStartX + cnpOffset, y);
            y += lineHeight;

            SafeDrawString(g, $"APARȚINĂTOR: {CaregiverName}", fontText, Brushes.Black, textStartX, y);
            SafeDrawString(g, $"CNP: {CaregiverCnp}", fontText, Brushes.Black, textStartX + cnpOffset, y);
            y += lineHeight;

            void SafeDrawParagraph(string label, string? value)
            {
                try
                {
                    string text = $"{label} {value}";
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        g.DrawString(text, fontText, Brushes.Black, textStartX, y);
                        y += lineHeight;
                    }
                    else
                    {
                        var rect = new RectangleF(textStartX, y, contentWidth, bottom - y - 150);
                        var textSize = g.MeasureString(text, fontText, (int)contentWidth);
                        g.DrawString(text, fontText, Brushes.Black, rect, format);
                        int lineCount = (int)Math.Ceiling(textSize.Height / fontText.GetHeight(g));
                        y += lineCount * lineHeight * 0.75f;
                    }
                }
                catch { /* ignore drawing errors */ }
            }

            SafeDrawParagraph("DIAGNOSTIC MENȚIONAT DE PACIENT:", DiagnosisMentioned);
            SafeDrawParagraph("MEDICAMENTE UTILIZATE DE PACIENT:", MedicationsMentioned);

            if (ModeCode == "AC" || ModeCode == "AM")
            {
                float offset = 250;
                SafeDrawString(g, $"PARAFĂ MEDIC: {DoctorStamp}", fontText, Brushes.Black, textStartX, y);
                SafeDrawString(g, $"SERIE/NUMĂR REȚETĂ: {Series}", fontText, Brushes.Black, textStartX + offset, y);
                y += lineHeight;
                SafeDrawParagraph("DIAGNOSTIC:", Diagnostic);
            }

            SafeDrawParagraph("SIMPTOMATOLOGIE:", Symptoms);
            SafeDrawParagraph("SUSPICIUNE:", Suspicion);
            SafeDrawParagraph("CONSTATĂRILE FARMACISTULUI:", PharmacistObservations);

            try
            {
                if (MedicationsWithPrescription?.Count > 0)
                {
                    y = DrawMedicationSection(g, textStartX, y, contentWidth, "MEDICAMENTE ELIBERATE CU REȚETĂ",
                                              MedicationsWithPrescription, fontSection, fontText, fontText, false);
                }

                if (MedicationsWithoutPrescription?.Count > 0)
                {
                    y = DrawMedicationSection(g, textStartX, y, contentWidth, "MEDICAMENTE ELIBERATE FĂRĂ REȚETĂ",
                                              MedicationsWithoutPrescription, fontSection, fontText, fontText, false);
                }
            }
            catch { /* ignore errors drawing tables */ }

            SafeDrawParagraph("NOTE CĂTRE MEDIC:", NotesToDoctor);
            SafeDrawParagraph("RECOMANDAREA FARMACISTULUI:", PharmacistRecommendation);
            SafeDrawParagraph("Serviciu farmaceutic:", PharmaceuticalService);

            try
            {
                float footerY = bottom - 100;
                SafeDrawString(g, $"FARMACIST: {PharmacistNameEffective}", fontText, Brushes.Black, left, footerY);

                if (!string.IsNullOrEmpty(AssistantName))
                    SafeDrawString(g, $"ASISTENT: {AssistantName}", fontText, Brushes.Black, left, footerY + 20);

                SafeDrawString(g, PatientName, fontText, Brushes.Black, right - 220, footerY);
                SafeDrawString(g, "Document generat cu Recomandarea Farmacistului", fontSmall, Brushes.Gray, left, footerY + 50);
            }
            catch { }
        }

        private SD.Font SafeCreateFont(string name, float size, FontStyle style)
        {
            try
            {
                return new SD.Font(name, size, style);
            }
            catch
            {
                return new SD.Font("Arial", size, style); // fallback
            }
        }

        private void SafeDrawLogo(SD.Graphics g, float right, ref float y)
        {
            try
            {
                using var logo = SD.Image.FromFile("Resources/Images/farma.png");
                g.DrawImage(logo, right - 80, y, 80, 80);
            }
            catch { }
        }

        private void SafeDrawString(SD.Graphics g, string? text, SD.Font font, SD.Brush brush, float x, float y)
        {
            try
            {
                g.DrawString(text ?? "-", font, brush, x, y);
            }
            catch { }
        }

        private string SafeGetNextSequentialNumber(string modeCode)
        {
            try
            {
                string folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "RecomandareaFarmacistului");

                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, $"{modeCode}_counter.txt");
                int lastNumber = 0;

                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    int.TryParse(content, out lastNumber);
                }

                int nextNumber = lastNumber + 1;
                File.WriteAllText(filePath, nextNumber.ToString());
                return nextNumber.ToString("D4");
            }
            catch
            {
                return "0001"; // fallback if file can't be read/written
            }
        }

        //    protected override void OnPrintPage(PrintPageEventArgs e)
        //    {
        //        var g = e.Graphics;
        //        float margin = 50;
        //        float left = margin;
        //        float top = margin;
        //        float right = e.PageBounds.Width - margin;
        //        float bottom = e.PageBounds.Height - margin;
        //        float y = top;
        //        float contentWidth = right - left;

        //        using var fontTitle = new SD.Font("Arial", 16f, FontStyle.Bold);
        //        using var fontText = new SD.Font("Arial", 10f, FontStyle.Regular);
        //        using var fontSmall = new SD.Font("Arial", 7f, FontStyle.Regular);
        //        using var fontSection = new SD.Font("Arial", 12f, FontStyle.Bold);

        //        float lineHeight = fontText.GetHeight(g) * 1.5f; // uniform spacing for all rows

        //        try
        //        {
        //            using var logo = SD.Image.FromFile("Resources/Images/farma.png");
        //            float logoWidth = 80;
        //            float logoHeight = 80;
        //            float logoX = right - logoWidth;
        //            g.DrawImage(logo, logoX, y, logoWidth, logoHeight);
        //        }
        //        catch { }

        //        float textStartX = left;
        //        float titleWidth = contentWidth - 80;
        //        var format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

        //        string pageTitle = ModeCode switch
        //        {
        //            "AC" => "Act consecutiv prescripției",
        //            "AP" => "Act farmaceutic",
        //            "AM" => "Act mixt",
        //            _ => "Document farmaceutic"
        //        };
        //        g.DrawString(pageTitle, fontTitle, Brushes.Black, textStartX, y);
        //        y += lineHeight * 2.5f;

        //        string code = $"{ModeCode}-{SessionManager.CurrentUser?.Ncm ?? "0000"}-{GetNextSequentialNumber(ModeCode)}";

        //        g.DrawString($"Data: {IssueDate:dd.MM.yyyy HH:mm}", fontText, Brushes.Black, textStartX, y);
        //        g.DrawString(code, fontText, Brushes.Black, right - 180, y);
        //        y += lineHeight;

        //        g.DrawString($"FARMACIST: {PharmacistNameEffective}", fontText, Brushes.Black, textStartX, y);
        //        y += lineHeight;
        //        g.DrawString($"FARMACIA: {PharmacyName}", fontText, Brushes.Black, textStartX, y);
        //        y += lineHeight;
        //        g.DrawString($"ADRESĂ: {PharmacyAddress}", fontText, Brushes.Black, textStartX, y);
        //        y += lineHeight;
        //        g.DrawString($"TELEFON: {PharmacyPhone}", fontText, Brushes.Black, textStartX, y);
        //        y += lineHeight;

        //        float cnpOffset = 350;
        //        g.DrawString($"PACIENT: {PatientName}", fontText, Brushes.Black, textStartX, y);
        //        g.DrawString($"CNP: {PatientCnp}", fontText, Brushes.Black, textStartX + cnpOffset, y);
        //        y += lineHeight;

        //        g.DrawString($"APARȚINĂTOR: {CaregiverName}", fontText, Brushes.Black, textStartX, y);
        //        g.DrawString($"CNP: {CaregiverCnp}", fontText, Brushes.Black, textStartX + cnpOffset, y);
        //        y += lineHeight;

        //        void DrawParagraph(string label, string value)
        //        {
        //            string text = $"{label} {value}";

        //            if (string.IsNullOrWhiteSpace(value))
        //            {
        //                g.DrawString(text, fontText, Brushes.Black, textStartX, y);
        //                y += lineHeight;
        //            }
        //            else
        //            {
        //                var rect = new RectangleF(textStartX, y, contentWidth, bottom - y - 150);
        //                var textSize = g.MeasureString(text, fontText, (int)contentWidth);

        //                g.DrawString(text, fontText, Brushes.Black, rect, format);
        //                int lineCount = (int)Math.Ceiling(textSize.Height / fontText.GetHeight(g));
        //                y += lineCount * lineHeight * 0.75f;
        //            }
        //        }

        //        DrawParagraph("DIAGNOSTIC MENȚIONAT DE PACIENT:", DiagnosisMentioned);
        //        DrawParagraph("MEDICAMENTE UTILIZATE DE PACIENT:", MedicationsMentioned);

        //        if (ModeCode == "AC" || ModeCode == "AM")
        //        {
        //            float offset = 250;
        //            g.DrawString($"PARAFĂ MEDIC: {DoctorStamp}", fontText, Brushes.Black, textStartX, y);
        //            g.DrawString($"SERIE/NUMĂR REȚETĂ: {Series}", fontText, Brushes.Black, textStartX + offset, y);
        //            y += lineHeight;
        //            DrawParagraph("DIAGNOSTIC:", Diagnostic);
        //        }

        //        DrawParagraph("SIMPTOMATOLOGIE:", Symptoms);
        //        DrawParagraph("SUSPICIUNE:", Suspicion);
        //        DrawParagraph("CONSTATĂRILE FARMACISTULUI:", PharmacistObservations);

        //        if (MedicationsWithPrescription?.Count > 0)
        //        {
        //            y = DrawMedicationSection(g, textStartX, y, contentWidth, "MEDICAMENTE ELIBERATE CU REȚETĂ",
        //                                      MedicationsWithPrescription, fontSection, fontText, fontText, false);
        //        }

        //        if (MedicationsWithoutPrescription?.Count > 0)
        //        {
        //            y = DrawMedicationSection(g, textStartX, y, contentWidth, "MEDICAMENTE ELIBERATE FĂRĂ REȚETĂ",
        //                                      MedicationsWithoutPrescription, fontSection, fontText, fontText, false);
        //        }

        //        DrawParagraph("NOTE CĂTRE MEDIC:", NotesToDoctor);
        //        DrawParagraph("RECOMANDAREA FARMACISTULUI:", PharmacistRecommendation);
        //        DrawParagraph("Serviciu farmaceutic:", PharmaceuticalService);

        //        float footerY = bottom - 100;

        //        g.DrawString($"FARMACIST: {PharmacistNameEffective}", fontText, Brushes.Black, left, footerY);

        //        if (!string.IsNullOrEmpty(AssistantName))
        //        {
        //            g.DrawString($"ASISTENT: {AssistantName}", fontText, Brushes.Black, left, footerY + 20);
        //        }

        //        g.DrawString(PatientName, fontText, Brushes.Black, right - 220, footerY);

        //        g.DrawString("Document generat cu Recomandarea Farmacistului", fontSmall, Brushes.Gray, left, footerY + 50);

        //    }

        //    private string GetNextSequentialNumber(string modeCode)
        //    {
        //        string folder = Path.Combine(
        //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        //"RecomandareaFarmacistului");

        //        Directory.CreateDirectory(folder); 

        //        string filePath = Path.Combine(folder, $"{modeCode}_counter.txt");

        //        int lastNumber = 0;
        //        if (File.Exists(filePath))
        //        {
        //            string content = File.ReadAllText(filePath);
        //            int.TryParse(content, out lastNumber);
        //        }

        //        int nextNumber = lastNumber + 1;

        //        File.WriteAllText(filePath, nextNumber.ToString());

        //        return nextNumber.ToString("D4");
        //    }



        private float DrawMedicationSection(SD.Graphics g, float left, float y, float contentWidth, string title,
                                    List<MedicationLine> medications, SD.Font sectionFont, SD.Font textFont, SD.Font headerFont,
                                    bool noColor)
        {
            g.DrawString(title, sectionFont, Brushes.Black, left, y);
            y += sectionFont.GetHeight(g) * 1.3f;

            y = DrawCompactMedicationsTable(g, left, y, contentWidth, medications, textFont, headerFont, noColor);

            return y + 5f;
        }



        private float DrawCompactMedicationsTable(SD.Graphics g, float left, float y, float tableWidth,
                                           List<MedicationLine> medications, SD.Font font, SD.Font headerFont,
                                           bool noColor)
        {
            float[] colWidths = { 30, 200, 65, 65, 65, 70, 130 };
            float totalWidth = colWidths.Sum();
            float scale = 1f;

            if (totalWidth > tableWidth)
                scale = tableWidth / totalWidth;

            for (int i = 0; i < colWidths.Length; i++)
                colWidths[i] *= scale;

            float x = left;
            float headerHeight = headerFont.GetHeight(g) * 1.5f;
            float rowHeight = font.GetHeight(g) * 2f;
            string[] headers = { "NR CRT", "MEDICAMENT", "DIMIN.", "PRÂNZ", "SEARA", "NOAPTEA", "MOD ADMIN." };

            for (int i = 0; i < headers.Length; i++)
            {
                var headerRect = new RectangleF(x, y, colWidths[i], headerHeight);
                g.DrawRectangle(Pens.Gray, Rectangle.Round(headerRect));

                var textRect = new RectangleF(x + 2, y + 2, colWidths[i] - 4, headerHeight - 4);
                using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(headers[i], headerFont, Brushes.Black, textRect, format);

                x += colWidths[i];
            }

            y += headerHeight;

            for (int r = 0; r < medications.Count; r++)
            {
                x = left;
                var med = medications[r];
                string[] values = {
            (r + 1).ToString(),
     med.Name ?? "-",
            med.Morning ?? "-",
   med.Noon ?? "-",
         med.Evening ?? "-",
       med.Night ?? "-",
          med.AdministrationMode ?? "-"
        };

                for (int i = 0; i < values.Length; i++)
                {
                    var cellRect = new RectangleF(x, y, colWidths[i], rowHeight);
                    g.DrawRectangle(Pens.Gray, Rectangle.Round(cellRect));

                    var textRect = new RectangleF(x + 2, y + 2, colWidths[i] - 4, rowHeight - 4);
                    using var format = new StringFormat
                    {
                        Alignment = i == 0 ? StringAlignment.Center : StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };

                    g.DrawString(values[i], font, Brushes.Black, textRect, format);

                    x += colWidths[i];
                }

                y += rowHeight;
            }

            return y;
        }

        private float DrawWrappedTextCustom(SD.Graphics g, string text, SD.Font font, float textStartX, float y, float contentWidth)
        {
            if (string.IsNullOrWhiteSpace(text))
                return y;

            var rect = new RectangleF(textStartX, y, contentWidth, 10000);
            using var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.Word
            };

            g.DrawString(text, font, Brushes.Black, rect, format);

            var size = g.MeasureString(text, font, (int)contentWidth);
            return y + size.Height + font.GetHeight(g) * 0.3f;
        }
    }
}

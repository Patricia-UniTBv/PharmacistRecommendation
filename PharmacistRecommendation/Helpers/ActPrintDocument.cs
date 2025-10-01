using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
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
            var g = e.Graphics;
            float left = 30, top = 40;
            float right = e.PageBounds.Width - 100;
            float y = top;
            float contentWidth = right - left;

            using var fontTitle = new SD.Font("Arial", 16f, FontStyle.Bold);
            using var fontText = new SD.Font("Arial", 10f, FontStyle.Regular);
            using var fontSmall = new SD.Font("Arial", 7f, FontStyle.Regular);
            using var fontSection = new SD.Font("Arial", 12f, FontStyle.Bold);

            float lineHeight = fontText.GetHeight(g) * 1.2f;

            try
            {
                using var logo = SD.Image.FromFile("Resources/Images/farma.png");
                g.DrawImage(logo, left, y, 60, 60);
            }
            catch { }

            float textStartX = left + 90;
            float titleWidth = contentWidth - 90;
            StringFormat format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near, FormatFlags = StringFormatFlags.LineLimit };

            string pageTitle = ModeCode switch
            {
                "AC" => "Act consecutiv prescripției",
                "AP" => "Act propriu",
                "AM" => "Act mixt",
                _ => "Document farmaceutic"
            };
            RectangleF rectTitle = new RectangleF(textStartX, y, titleWidth, 40);
            g.DrawString(pageTitle, fontTitle, Brushes.Black, rectTitle, format);

            y += 50;

            string code = $"{ModeCode}-{SessionManager.CurrentUser?.Ncm ?? "0000"}-{new Random().Next(1000, 9999):D4}";
            g.DrawString($"Data: {IssueDate:dd.MM.yyyy HH:mm}", fontText, Brushes.Black, textStartX, y);
            g.DrawString(code, fontText, Brushes.Black, right - 150, y);
            y += lineHeight;
            g.DrawString($"FARMACIST: {SessionManager.CurrentUser?.FirstName} {SessionManager.CurrentUser?.LastName}", fontText, Brushes.Black, textStartX, y);
            y += lineHeight;

            g.DrawString($"FARMACIA: {PharmacyName}", fontText, Brushes.Black, textStartX, y);
            y += lineHeight;
            g.DrawString($"ADRESA: {PharmacyAddress}", fontText, Brushes.Black, textStartX, y);
            y += lineHeight;
            g.DrawString($"TELEFON: {PharmacyPhone}", fontText, Brushes.Black, textStartX, y);
            y += lineHeight;

            float cnpOffset = 200; 

            g.DrawString($"PACIENT: {PatientName}", fontText, Brushes.Black, textStartX, y);
            g.DrawString($"CNP: {PatientCnp}", fontText, Brushes.Black, textStartX + cnpOffset, y);
            y += lineHeight;

            g.DrawString($"APARȚINĂTOR: {CaregiverName}", fontText, Brushes.Black, textStartX, y);
            g.DrawString($"CNP: {CaregiverCnp}", fontText, Brushes.Black, textStartX + cnpOffset, y);
            y += lineHeight;

            void DrawWrappedText(string text)
            {
                RectangleF rect = new RectangleF(textStartX, y, contentWidth, e.PageBounds.Height - y - 150);
                g.DrawString(text, fontText, Brushes.Black, rect, format);
                y += g.MeasureString(text, fontText, (int)contentWidth).Height + lineHeight / 2;
            }

            DrawWrappedText($"DIAGNOSTIC MENȚIONAT DE PACIENT: {DiagnosisMentioned}");
            DrawWrappedText($"MEDICAMENTE UTILIZATE DE PACIENT: {MedicationsMentioned}");
            if (ModeCode == "AC" || ModeCode == "AM")
            {
                float offset = 250; 
                g.DrawString($"PARAFĂ MEDIC: {DoctorStamp}", fontText, Brushes.Black, textStartX, y);
                g.DrawString($"SERIE/NUMĂR MEDIC: {Series}", fontText, Brushes.Black, textStartX + offset, y);
                y += fontText.GetHeight(g) * 1.1f;
                DrawWrappedText($"DIAGNOSTIC: {Diagnostic}");
            }
            DrawWrappedText($"SIMPTOMATOLOGIE: {Symptoms}");
            DrawWrappedText($"SUSPICIUNE: {Suspicion}");
            DrawWrappedText($"CONSTATĂRILE FARMACISTULUI: {PharmacistObservations}");

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

            y = DrawWrappedTextCustom(g, NotesToDoctor, fontText, textStartX, y, contentWidth);
            y = DrawWrappedTextCustom(g, PharmacistRecommendation, fontText, textStartX, y, contentWidth);
            DrawWrappedText($"Serviciu farmaceutic: {PharmaceuticalService}");

            float footerY = e.PageBounds.Bottom - 100;
            string pharmacistName = $"{SessionManager.CurrentUser?.FirstName} {SessionManager.CurrentUser?.LastName} {SessionManager.CurrentUser?.Ncm}";
            g.DrawString(pharmacistName.ToUpper(), fontText, Brushes.Black, left, footerY);
            g.DrawString(PatientName?.ToUpper(), fontText, Brushes.Black, right - 120, footerY);

            g.DrawString("Document generat cu Recomandarea Farmacistului", fontSmall, Brushes.Gray, left, footerY + 35);
        }




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
            string[] headers = { "NR CRT", "MEDICAMENT", "DIMIN.", "PRÂNZ", "SEARA", "NOAPTEA", "MOD ADMIN" };

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

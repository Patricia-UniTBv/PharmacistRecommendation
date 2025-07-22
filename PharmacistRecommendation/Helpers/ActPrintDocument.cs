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
        public string? Series { get; set; }
        public string? Number { get; set; }
        public DateTime IssueDate { get; set; }
        public string? PatientName { get; set; }
        public string? PatientCnp { get; set; }
        public string? CaregiverName { get; set; }
        public string? CaregiverCnp { get; set; }
        public string? DoctorStamp { get; set; }
        public string? Diagnostic { get; set; }
        public string? DiagnosisMentioned { get; set; }
        public string? Symptoms { get; set; }
        public string? Suspicion { get; set; }
        public string? PharmacistObservations { get; set; }
        public string? PharmacistRecommendation { get; set; }
        public string? PharmaceuticalService { get; set; }
        public List<MedicationLine> MedicationsWithPrescription { get; set; } = new();
        public List<MedicationLine> MedicationsWithoutPrescription { get; set; } = new();
        private int currentMedicationWithPrescriptionIndex = 0;
        private int currentMedicationWithoutPrescriptionIndex = 0;
        private bool printingWithPrescription = true;
        private bool isFirstPage = true;


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
            float left = 40, top = 40;
            float right = e.PageBounds.Width - 40;
            float y = top;
            float contentWidth = right - left;

            using var fontTitle = new SD.Font("Arial", 16f, FontStyle.Bold);
            using var fontSubtitle = new SD.Font("Arial", 11f, FontStyle.Bold);
            using var fontText = new SD.Font("Arial", 10f, FontStyle.Regular);
            using var fontBold = new SD.Font("Arial", 10f, FontStyle.Bold);
            using var fontSection = new SD.Font("Arial", 12f, FontStyle.Bold);
            using var fontSmall = new SD.Font("Arial", 8f, FontStyle.Regular);

            float lineHeight = fontText.GetHeight(g) * 1.2f;
            float sectionSpacing = lineHeight * 1.2f;
            float logoSize = 80f;

            string Safe(string s) => string.IsNullOrWhiteSpace(s) ? "-" : s;

            if ((MedicationsWithPrescription == null || MedicationsWithPrescription.Count == 0) && printingWithPrescription)
            {
                printingWithPrescription = false;
            }


            if (isFirstPage)
            {
                DrawHeaderWithLogo(g, left, y, contentWidth, logoSize, fontTitle, fontSubtitle);
                y += logoSize + 15f;

                using (var grayBrush = new SolidBrush(SD.Color.FromArgb(100, 100, 100)))
                {
                    g.DrawString($"Data: {IssueDate:dd.MM.yyyy HH:mm}", fontBold, Brushes.Black, left, y);
                    //g.DrawString($"Serie: {Safe(Series)} / Nr: {Safe(Number)}", fontBold, Brushes.Black, right - 200, y);
                }
                y += lineHeight * 1.5f;

                y = DrawInformationSection(g, left, y, contentWidth, fontSection, fontText, lineHeight);
                y = DrawPatientSection(g, left, y, contentWidth, fontSection, fontText, lineHeight);
                y = DrawMedicalDetailsSection(g, left, y, contentWidth, fontSection, fontText, lineHeight);

                isFirstPage = false;
            }

            // Calculează cât spațiu a mai rămas pe pagină
            float availableHeight = e.PageBounds.Height - y - 110; // 110 pentru footer și eventuale margini
            float rowHeight = fontText.GetHeight(g) * 3.5f;
            int medicationsPerPage = (int)((availableHeight - sectionSpacing * 2) / rowHeight);

            // 1. Paginare pentru "cu rețetă"
            if (printingWithPrescription && MedicationsWithPrescription?.Count > 0)
            {
                y += sectionSpacing;
                var medsToDraw = MedicationsWithPrescription
                    .Skip(currentMedicationWithPrescriptionIndex)
                    .Take(medicationsPerPage)
                    .ToList();

                y = DrawMedicationSection(g, left, y, contentWidth, "MEDICAMENTE ELIBERATE CU REȚETĂ",
                                          medsToDraw, fontSection, fontText, fontBold);

                currentMedicationWithPrescriptionIndex += medsToDraw.Count;

                if (currentMedicationWithPrescriptionIndex < MedicationsWithPrescription.Count)
                {
                    e.HasMorePages = true;
                    return;
                }
                else
                {
                    // Doar dacă lista fără rețetă conține ceva, treci la următoarea secțiune
                    if (MedicationsWithoutPrescription != null && MedicationsWithoutPrescription.Count > 0)
                    {
                        printingWithPrescription = false;
                        currentMedicationWithPrescriptionIndex = 0;
                        e.HasMorePages = true;
                        return;
                    }
                    else
                    {
                        // Niciun tabel de continuat, deci finalizezi documentul aici!
                        currentMedicationWithPrescriptionIndex = 0;
                        printingWithPrescription = true;
                        // Recomandare și footer pe ultima pagină
                        y += sectionSpacing;
                        y = DrawRecommendationSection(g, left, y, contentWidth, fontSection, fontText, lineHeight);
                        DrawFooter(g, left, right, e.PageBounds.Bottom, fontText, fontSmall);
                        e.HasMorePages = false;
                        return;
                    }
                }
            }

            // 2. Paginare pentru "fără rețetă"
            if (!printingWithPrescription && MedicationsWithoutPrescription?.Count > 0)
            {
                if (currentMedicationWithoutPrescriptionIndex == 0)
                    y += sectionSpacing;

                var medsToDraw = MedicationsWithoutPrescription
                    .Skip(currentMedicationWithoutPrescriptionIndex)
                    .Take(medicationsPerPage)
                    .ToList();

                y = DrawMedicationSection(g, left, y, contentWidth, "MEDICAMENTE ELIBERATE FĂRĂ REȚETĂ",
                                          medsToDraw, fontSection, fontText, fontBold);

                currentMedicationWithoutPrescriptionIndex += medsToDraw.Count;

                if (currentMedicationWithoutPrescriptionIndex < MedicationsWithoutPrescription.Count)
                {
                    e.HasMorePages = true;
                    return;
                }
                else
                {
                    // Toate medicamentele sunt desenate, urmează secțiunea de recomandare și footer
                    currentMedicationWithoutPrescriptionIndex = 0;
                }
            }

            // 3. Secțiune recomandare și footer DOAR pe ultima pagină!
            y += sectionSpacing;
            y = DrawRecommendationSection(g, left, y, contentWidth, fontSection, fontText, lineHeight);
            DrawFooter(g, left, right, e.PageBounds.Bottom, fontText, fontSmall);

            // Reset pentru printare viitoare
            printingWithPrescription = true;
            e.HasMorePages = false;
        }


        private void DrawHeaderWithLogo(SD.Graphics g, float left, float y, float contentWidth, float logoSize, SD.Font titleFont, SD.Font subtitleFont)
        {
            var logoRect = new RectangleF(left, y, logoSize, logoSize);
            var symbolRect = new RectangleF(logoRect.X + 20, logoRect.Y, 60, 60);
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images", "logo.png");

            if (File.Exists(logoPath))
            {
                using (var logo = SD.Image.FromFile(logoPath))
                {
                    float aspectImage = (float)logo.Width / logo.Height;
                    float aspectRect = symbolRect.Width / symbolRect.Height;

                    float drawWidth, drawHeight;
                    float drawX, drawY;

                    if (aspectImage > aspectRect)
                    {
                        drawWidth = symbolRect.Width;
                        drawHeight = symbolRect.Width / aspectImage;
                        drawX = symbolRect.X;
                        drawY = symbolRect.Y + (symbolRect.Height - drawHeight) / 2;
                    }
                    else
                    {
                        drawHeight = symbolRect.Height;
                        drawWidth = symbolRect.Height * aspectImage;
                        drawX = symbolRect.X + (symbolRect.Width - drawWidth) / 2;
                        drawY = symbolRect.Y;
                    }

                    var destRect = new RectangleF(drawX, drawY, drawWidth, drawHeight);
                    g.DrawImage(logo, destRect);
                }
            }
            else
            {
                using (var symbolFont = new SD.Font("Arial", 24f, FontStyle.Bold))
                    g.DrawString("?", symbolFont, Brushes.Black, symbolRect.X, symbolRect.Y);
            }


            var titleX = left + logoSize + 20;
            var titleY = y + 10;
            g.DrawString("RECOMANDAREA FARMACISTULUI", titleFont, Brushes.DarkBlue, titleX, titleY);

            using (var subtitleBrush = new SolidBrush(SD.Color.FromArgb(100, 100, 100)))
            {
                g.DrawString("Consiliere farmaceutică profesională", subtitleFont, subtitleBrush, titleX, titleY + 25);
            }

            using (var pen = new Pen(SD.Color.FromArgb(70, 130, 180), 2))
            {
                g.DrawLine(pen, left, y + logoSize + 10, left + contentWidth, y + logoSize + 10);
            }
        }

        private float DrawInformationSection(SD.Graphics g, float left, float y, float contentWidth, SD.Font sectionFont, SD.Font textFont, float lineHeight)
        {
            g.DrawString("INFORMAȚII FARMACIE", sectionFont, new SolidBrush(SD.Color.FromArgb(70, 130, 180)), left + 10, y + 3);
            y += lineHeight * 1.4f;

            g.DrawString($"Nume farmacie: {PharmacyName}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"Adresă: {PharmacyAddress}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"Telefon: {PharmacyPhone}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight * 1.3f;

            return y;
        }

        private float DrawPatientSection(SD.Graphics g, float left, float y, float contentWidth, SD.Font sectionFont, SD.Font textFont, float lineHeight)
        {
            g.DrawString("INFORMAȚII PACIENT", sectionFont, new SolidBrush(SD.Color.FromArgb(70, 130, 180)), left + 10, y + 3);
            y += lineHeight * 1.4f;

            g.DrawString($"Nume pacient: {PatientName}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"CNP pacient: {PatientCnp}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"Aparținător: {CaregiverName}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"CNP aparținător: {CaregiverCnp}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight * 1.3f;

            return y;
        }

        private float DrawMedicalDetailsSection(SD.Graphics g, float left, float y, float contentWidth, SD.Font sectionFont, SD.Font textFont, float lineHeight)
        {
            g.DrawString("DETALII MEDICALE", sectionFont, new SolidBrush(SD.Color.FromArgb(70, 130, 180)), left + 10, y + 3);
            y += lineHeight * 1.4f;

            g.DrawString($"Parafă medic: {DoctorStamp}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"Serie: {Series}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"Număr: {Number}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"Diagnostic: {Diagnostic}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"Diagnostic menționat: {DiagnosisMentioned}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"Simptomatologie: {Symptoms}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"Suspiciune: {Suspicion}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight;
            g.DrawString($"Constatările farmacistului: {PharmacistObservations}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight * 1.3f;

            return y;
        }

        private float DrawMedicationSection(SD.Graphics g, float left, float y, float contentWidth, string title,
                                           List<MedicationLine> medications, SD.Font sectionFont, SD.Font textFont, SD.Font headerFont)
        {
            var titleRect = new RectangleF(left, y, contentWidth, sectionFont.GetHeight(g) * 1.3f);
            var titleColor = title.Contains("REȚETĂ") ? SD.Color.FromArgb(220, 20, 60) : SD.Color.FromArgb(30, 144, 255);
            var bgColor = title.Contains("REȚETĂ") ? SD.Color.FromArgb(255, 240, 245) : SD.Color.FromArgb(240, 248, 255);

            g.DrawString(title, sectionFont, new SolidBrush(titleColor), left + 10, y + 5);
            y += sectionFont.GetHeight(g) * 1.9f;

            y = DrawEnhancedMedicationsTable(g, left, y, contentWidth, medications, textFont, headerFont);

            return y;
        }

        private float DrawEnhancedMedicationsTable(SD.Graphics g, float left, float y, float tableWidth,
                                                 List<MedicationLine> medications, SD.Font font, SD.Font headerFont)
        {
            float[] colWidths = { 50, 220, 100, 80, 80, 80, 180 };
            float x = left;
            float headerHeight = headerFont.GetHeight(g) * 1.8f;
            float rowHeight = font.GetHeight(g) * 2.9f;
            string[] headers = { "NR\nCRT", "MEDICAMENT", "DIMINEAȚA", "PRÂNZ", "SEARA", "NOAPTEA", "MOD ADMINISTRARE" };

            for (int i = 0; i < headers.Length; i++)
            {
                var headerRect = new RectangleF(x, y, colWidths[i], headerHeight);

                using (var headerBrush = new SolidBrush(SD.Color.FromArgb(70, 130, 180)))
                {
                    g.FillRectangle(headerBrush, headerRect);
                }

                g.DrawRectangle(new Pen(SD.Color.FromArgb(25, 25, 112), 2), Rectangle.Round(headerRect));

                var textRect = new RectangleF(x + 5, y + 8, colWidths[i] - 10, headerHeight - 16);
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    g.DrawString(headers[i], headerFont, Brushes.White, textRect, format);
                }

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

                var rowColor = r % 2 == 0 ? SD.Color.FromArgb(248, 248, 255) : SD.Color.White;

                for (int i = 0; i < values.Length; i++)
                {
                    var cellRect = new RectangleF(x, y, colWidths[i], rowHeight);

                    using (var cellBrush = new SolidBrush(rowColor))
                    {
                        g.FillRectangle(cellBrush, cellRect);
                    }

                    g.DrawRectangle(new Pen(SD.Color.FromArgb(200, 200, 200)), Rectangle.Round(cellRect));

                    var textRect = new RectangleF(x + 5, y + 5, colWidths[i] - 10, rowHeight - 10);
                    using (var format = new StringFormat())
                    {
                        format.Alignment = i == 0 ? StringAlignment.Center : StringAlignment.Near;
                        format.LineAlignment = StringAlignment.Center;
                        format.Trimming = StringTrimming.EllipsisCharacter;
                        g.DrawString(values[i], font, Brushes.Black, textRect, format);
                    }

                    x += colWidths[i];
                }
                y += rowHeight;
            }

            return y + 10;
        }

        private float DrawRecommendationSection(SD.Graphics g, float left, float y, float contentWidth, SD.Font sectionFont, SD.Font textFont, float lineHeight)
        {
            g.DrawString("RECOMANDAREA FARMACISTULUI", sectionFont, new SolidBrush(SD.Color.FromArgb(0, 128, 0)), left + 10, y + 3);
            y += lineHeight * 1.6f;

            var recRect = new RectangleF(left + 10, y, contentWidth - 20, lineHeight * 3);
            g.FillRectangle(Brushes.White, recRect);
            g.DrawRectangle(new Pen(SD.Color.FromArgb(200, 200, 200)), Rectangle.Round(recRect));

            var textRect = new RectangleF(left + 20, y + 10, contentWidth - 40, lineHeight * 2.5f);
            g.DrawString(PharmacistRecommendation, textFont, Brushes.Black, textRect);
            y += lineHeight * 3.5f;

            g.DrawString($"Serviciu farmaceutic: {PharmaceuticalService}", textFont, Brushes.Black, left + 10, y);
            y += lineHeight * 1.5f;

            return y;
        }

        private void DrawFooter(SD.Graphics g, float left, float right, float bottom, SD.Font textFont, SD.Font smallFont)
        {
            var footerY = bottom - 80;

            g.DrawString("Semnătură farmacist:", textFont, Brushes.Black, left, footerY);
            g.DrawLine(Pens.Black, left + 150, footerY + 15, left + 350, footerY + 15);

            using (var grayBrush = new SolidBrush(SD.Color.FromArgb(128, 128, 128)))
            {
                g.DrawString("Document generat cu sistemul Recomandarea Farmacistului", smallFont, grayBrush, left, bottom - 30);
                g.DrawString($"Generat la: {DateTime.Now:dd.MM.yyyy HH:mm}", smallFont, grayBrush, right - 200, bottom - 30);
            }
        }
    }
}

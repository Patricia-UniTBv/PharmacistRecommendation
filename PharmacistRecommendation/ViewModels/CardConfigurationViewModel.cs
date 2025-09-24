using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using System.Drawing.Printing;
using System.Drawing;
using System.Windows.Forms;
using SD = System.Drawing;

namespace PharmacistRecommendation.ViewModels
{
    public partial class CardConfigurationViewModel : ObservableObject
    {
        private readonly IPharmacyCardService _pharmacyCardService;
        private readonly IPharmacyService _pharmacyService;
        [ObservableProperty] private string cardNumber;
        [ObservableProperty] private string firstName;
        [ObservableProperty] private string lastName;
        [ObservableProperty] private string cnp;
        [ObservableProperty] private string cid;
        [ObservableProperty] private string phone;
        [ObservableProperty] private string email;
        [ObservableProperty] private string gender = null;
        [ObservableProperty] private DateTime birthdate;
        [ObservableProperty] private bool isPrintButtonEnabled = false; 

        public List<string> GenderOptions { get; } = new() { "Feminin", "Masculin" };

        public event Action<PharmacyCardDTO?>? CloseRequested;
        private int pharmacyId { get; set; }
        private string _consentDecl;

        [ObservableProperty] private string validationMessage;

        [ObservableProperty]
        private string birthdateString;

        public CardConfigurationViewModel(IPharmacyCardService pharmacyCardService, IPharmacyService pharmacyService)
        {
            _pharmacyCardService = pharmacyCardService;
            _pharmacyService = pharmacyService;

            pharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1;
        }

        partial void OnCnpChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && value.Length == 13)
            {
                try
                {
                    int s = int.Parse(value.Substring(0, 1));
                    int yy = int.Parse(value.Substring(1, 2));
                    int mm = int.Parse(value.Substring(3, 2));
                    int dd = int.Parse(value.Substring(5, 2));

                    int year = s switch
                    {
                        1 or 2 => 1900 + yy,
                        3 or 4 => 1800 + yy,
                        5 or 6 => 2000 + yy,
                        _ => throw new Exception("CNP invalid")
                    };

                    var date = new DateTime(year, mm, dd);
                    Birthdate = date;
                    BirthdateString = date.ToString("dd/MM/yyyy");

                    Gender = (s % 2 == 0) ? "Feminin" : "Masculin";
                }
                catch
                {
                 
                }
            }
        }


        partial void OnBirthdateStringChanged(string value)
        {
            if (DateTime.TryParseExact(value, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var date))
            {
                Birthdate = date;
            }
        }


        [RelayCommand]
        private async Task GenerateCid()
        {
            if (string.IsNullOrWhiteSpace(Cnp) || Cnp.Length != 13)
            {
                await Shell.Current.DisplayAlert("Eroare", "Introduceți un CNP valid (13 caractere).", "OK");
                return;
            }

            try
            {
                var cid = CidGen.GetCidHash(Cnp); 

                if (!string.IsNullOrEmpty(cid))
                    Cid = cid;
                else
                    await Shell.Current.DisplayAlert("Eroare", "Nu s-a putut genera CID-ul.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Eroare neașteptată", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            ValidationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                ValidationMessage = "Introduceți numele și prenumele.";
                return;
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                ValidationMessage = "Introduceți numele și prenumele.";
                return;
            }

            if (string.IsNullOrWhiteSpace(CardNumber))
            {
                ValidationMessage = "Introduceți numărul cardului.";
                return;
            }

            try
            {
                var card = await _pharmacyCardService.CreateCardAsync(
                    code: cardNumber,
                    pharmacyId: pharmacyId, 
                    firstName: FirstName,
                    lastName: LastName,
                    cnp: Cnp,
                    cid: Cid,
                    email: Email,
                    phone: Phone, 
                    gender: Gender,
                    birthdate: birthdate
                );

                IsPrintButtonEnabled = true;
                await Shell.Current.DisplayAlert("Succes", "Cardul pacientului a fost salvat!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Eroare", $"A apărut o eroare la salvare: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Print()
        {
            var _pharmacy = await _pharmacyService.GetByIdAsync(pharmacyId);
            string Safe(string s) => string.IsNullOrWhiteSpace(s) ? "-" : s;

            _consentDecl = _pharmacy!.ConsentTemplate!
                .Replace("{PharmacyName}", Safe(_pharmacy.Name))
                .Replace("{PharmacyAddress}", Safe(_pharmacy.Address!))
                .Replace("{PharmacyFiscalCode}", Safe(_pharmacy.CUI!));

            using var pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = false;

            pd.PrintPage += Pd_PrintPage;

            var result = await Shell.Current.DisplayActionSheet("Alege tipărire sau PDF", "Anulează", null, "Tipărire", "Salvează ca PDF");

            if (result == "Tipărire")
            {
                using var dlg = new PrintDialog { Document = pd };
                if (dlg.ShowDialog() == DialogResult.OK)
                    pd.Print();
            }
            else if (result == "Salvează ca PDF")
            {
                string safeName = string.IsNullOrWhiteSpace(firstName + lastName) ? "Pacient" : $"{firstName}_{lastName}";
                string safeCard = string.IsNullOrWhiteSpace(cardNumber) ? "0000" : cardNumber;
                string defaultFileName = $"{safeName}_{safeCard}.pdf";

                using var sfd = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = defaultFileName,
                    Title = "Salvează PDF-ul"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                    pd.PrinterSettings.PrintToFile = true;
                    pd.PrinterSettings.PrintFileName = sfd.FileName;
                    pd.Print();
                    await Shell.Current.DisplayAlert("Succes", $"Fișier PDF salvat: {sfd.FileName}", "OK");
                }
            }
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;

            var margin = e.MarginBounds;

            float left = margin.Left + 5;
            float top = margin.Top + 5;
            float right = margin.Right - 5;
            float bottom = margin.Bottom - 5;

            using var fontText = new SD.Font("Tahoma", 10f, SD.FontStyle.Regular);
            float lineHeight = fontText.GetHeight(g) * 1.2f;

            string Safe(string s) => string.IsNullOrWhiteSpace(s) ? "-" : s;

            float y = top;

            g.DrawString($"Număr card: {Safe(cardNumber)}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"Nume: {Safe($"{firstName} {lastName}".Trim())}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"CNP: {Safe(Cnp)}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"CID: {Safe(Cid)}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"Telefon: {Safe(phone)}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"E-mail: {Safe(email)}", fontText, SD.Brushes.Black, left, y); y += lineHeight * 1.5f;

            float boxSize = 12;
            float xCheckbox = left;
            g.DrawRectangle(Pens.Black, xCheckbox, y, boxSize, boxSize);

            float padding = 5;
            float xText = xCheckbox + boxSize + padding;
            g.DrawString("Declar că datele furnizate mai sus sunt corecte.", fontText, Brushes.Black, xText, y);
            y += 100;

            var rect = new RectangleF(left, y, right - left, bottom - y - 70);
            g.DrawString(_consentDecl, fontText, Brushes.Black, rect);

            float footerSpacing = 10;

            g.DrawString($"Data: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", fontText, Brushes.Black,
                new SD.PointF(left, bottom - 3 * lineHeight - footerSpacing));
            g.DrawString($"Nume: {Safe($"{firstName} {lastName}".Trim())}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
                new SD.PointF(left, bottom - 2 * lineHeight - footerSpacing);
            g.DrawString("Semnătura: ______________________", fontText, Brushes.Black,
                new SD.PointF(left, bottom - 1 * lineHeight - footerSpacing));
        }

        [RelayCommand]
        private void NewCard()
        {
            CardNumber = string.Empty;
            LastName = string.Empty;
            FirstName = string.Empty;
            Cnp = string.Empty;
            Cid = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            Birthdate = DateTime.Today;
            Gender = null;
        }

    }
}

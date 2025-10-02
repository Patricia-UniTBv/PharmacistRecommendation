using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using System.Drawing.Printing;
using System.Drawing;
using System.Windows.Forms;
using SD = System.Drawing;
using System.Runtime.InteropServices;
using Entities.Services;

namespace PharmacistRecommendation.ViewModels
{
    public partial class CardConfigurationViewModel : ObservableObject
    {
        private readonly IPharmacyCardService _pharmacyCardService;
        private readonly IPharmacyService _pharmacyService;
        private readonly IPatientService _patientService;

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

        public CardConfigurationViewModel(IPharmacyCardService pharmacyCardService, IPharmacyService pharmacyService, IPatientService patientService)
        {
            _pharmacyCardService = pharmacyCardService;
            _pharmacyService = pharmacyService;
            _patientService = patientService;

            pharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1;
        }

        partial void OnCardNumberChanged(string oldValue, string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue))
                return;

            _ = LoadPatientByCardAsync(newValue);
        }

        private async Task LoadPatientByCardAsync(string cardNumber)
        {
            var card = await _pharmacyCardService.GetByCodeAsync(cardNumber.Trim());

            if (card == null)
            {
                return;
            }

            var patient = await _patientService.GetByIdAsync(card.PatientId);

            if (patient == null)
            {
                ValidationMessage = "Pacientul asociat cardului nu a fost găsit.";
                return;
            }

            FirstName = patient.FirstName;
            LastName = patient.LastName;
            Cnp = patient.Cnp;
            Birthdate = patient.Birthdate ?? DateTime.MinValue;
            Gender = patient.Gender;
            Phone = patient.Phone;
            Email = patient.Email;
            Cid = patient.Cid;

            IsPrintButtonEnabled = true;
            ValidationMessage = string.Empty;
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
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

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
                IntPtr handle = LoadLibrary(@"Platforms\Windows\CidGen64.dll");
                if (handle == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    await Shell.Current.DisplayAlert("Eroare DLL", $"Nu se poate încărca DLL-ul! Cod eroare: {error}", "OK");
                    return;
                }

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
                    birthdate: birthdate == default(DateTime)
                         ? new DateTime(1900, 1, 1)
                         : birthdate
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

        private async void Pd_PrintPage(object sender, PrintPageEventArgs e)
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

            try
            {
                using var logo = SD.Image.FromFile("Resources/Images/farma.png");
                float logoWidth = 80;  
                float logoHeight = 80; 
                float logoX = right - logoWidth;
                float logoY = top - 15;             
                g.DrawImage(logo, logoX, logoY, logoWidth, logoHeight);
            }
            catch {  }

            float y = top;

            g.DrawString($"Număr card: {Safe(cardNumber)}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"Nume pacient: {Safe($"{firstName} {lastName}".Trim())}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
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
            float footerY = bottom - 3 * lineHeight - footerSpacing;

            float indent = 20; // distanța față de marginea stângă

            // Data generării
            g.DrawString(
                $"Data: {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
                fontText,
                SD.Brushes.Black,
                new SD.PointF(left + indent, footerY)
            );

            // Numele farmacistului + NCM
            string pharmacistName = $"{SessionManager.CurrentUser?.FirstName} {SessionManager.CurrentUser?.LastName} {SessionManager.CurrentUser?.Ncm}";
            g.DrawString(
                $"Farmacist: {Safe(pharmacistName.Trim())}",
                fontText,
                SD.Brushes.Black,
                new SD.PointF(left + indent, footerY + lineHeight)
            );

            // Semnătura
            g.DrawString(
                "Semnătura: ______________________",
                fontText,
                SD.Brushes.Black,
                new SD.PointF(left + indent, footerY + 2 * lineHeight)
            );

            // Textul fix
            g.DrawString(
                "Document generat cu Recomandarea Farmacistului",
                fontText,
                SD.Brushes.Gray,
                new SD.PointF(left + indent, footerY + 3 * lineHeight)
            );

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

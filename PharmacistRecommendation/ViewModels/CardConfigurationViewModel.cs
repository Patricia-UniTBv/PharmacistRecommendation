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


        public CardConfigurationViewModel(IPharmacyCardService pharmacyCardService, IPharmacyService pharmacyService)
        {
            _pharmacyCardService = pharmacyCardService;
            _pharmacyService = pharmacyService;

            pharmacyId = 1; // to be modified 
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
            if (string.IsNullOrWhiteSpace(cardNumber) || string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                await Shell.Current.DisplayAlert("Eroare", "Introduceți datele obligatorii (nume, prenume, număr card).", "OK");
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

            _consentDecl = _pharmacy.ConsentTemplate
                .Replace("{PharmacyName}", _pharmacy.Name)
                .Replace("{PharmacyAddress}", _pharmacy.Address)
                .Replace("{PharmacyFiscalCode}", _pharmacy.Cui);

            using var pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = false;
            pd.PrinterSettings = new PrinterSettings();
            pd.PrintPage += Pd_PrintPage;

            using var dlg = new PrintDialog { Document = pd };
            if (dlg.ShowDialog() == DialogResult.OK)
                pd.Print();
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            float dpiX = g.DpiX, dpiY = g.DpiY;
            var m = e.PageSettings.Margins;

            float left = 30, top = 30;
            float right = e.PageBounds.Width - 30;
            float bottom = e.PageBounds.Height - 30;

            using var fontText = new SD.Font("Tahoma", 10f, SD.FontStyle.Regular);
            float lineHeight = fontText.GetHeight(g) * 1.2f;

            string Safe(string s) => string.IsNullOrWhiteSpace(s) ? "-" : s;

            float y = top;
            float contentWidth = right - left;

            g.DrawString($"Număr card: {Safe(cardNumber)}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"Nume: {Safe($"{firstName} {lastName}".Trim())}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"CNP: {Safe(Cnp)}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"CID: {Safe(Cid)}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"Telefon: {Safe(phone)}", fontText, SD.Brushes.Black, left, y); y += lineHeight;
            g.DrawString($"E-mail: {Safe(email)}", fontText, SD.Brushes.Black, left, y); y += lineHeight * 1.5f;

            float boxSize = 12;
            float xCheckbox = 30;
            g.DrawRectangle(Pens.Black, xCheckbox, y, boxSize, boxSize);

            float padding = 5;
            float xText = xCheckbox + boxSize + padding;
            g.DrawString("Declar că datele furnizate mai sus sunt corecte.", fontText, Brushes.Black, xText, y);
            y += 130;

            var rect = new RectangleF(left, y, right - left, bottom - y - 40);
            g.DrawString(_consentDecl, fontText, Brushes.Black, rect);

            g.DrawString($"Data: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", fontText, Brushes.Black, new SD.PointF(20, e.PageBounds.Height - 100));
            g.DrawString("Nume: __________________________", fontText, Brushes.Black, new SD.PointF(20, e.PageBounds.Height - 80));
            g.DrawString("Semnătura: ______________________", fontText, Brushes.Black, new SD.PointF(20, e.PageBounds.Height - 60));
        }
    }
}

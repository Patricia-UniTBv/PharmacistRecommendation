using CommunityToolkit.Maui.Views;
using DTO;
using PharmacistRecommendation.ViewModels;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace PharmacistRecommendation.Views;

public partial class CardConfigurationView : Popup
{
	public CardConfigurationView(CardConfigurationViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;

        vm.CloseRequested += OnCloseRequested;
    }
    private void OnCloseRequested(PharmacyCardDTO? result)
    {
        this.Close(result);
    }

    private void OnPrintClicked(object sender, EventArgs e)
    {
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
        var fontTitle = new System.Drawing.Font("Tahoma", 14, FontStyle.Bold);
        var fontText = new System.Drawing.Font("Tahoma", 10);
        float y = 20;

        // 1) Header – Card number etc.
        g.DrawString("Card number: 50100001", fontTitle, Brushes.Black, 20, y);
        y += 30;
        g.DrawString("Name: ________________________", fontText, Brushes.Black, 20, y);
        y += 20;
        g.DrawString("CNP: _________________________", fontText, Brushes.Black, 20, y);
        y += 20;
        g.DrawString("CID: _________________________", fontText, Brushes.Black, 20, y);
        y += 20;
        g.DrawString("Telefon: _____________________", fontText, Brushes.Black, 20, y);
        y += 20;
        g.DrawString("E-mail: ______________________", fontText, Brushes.Black, 20, y);

        y += 30;
        g.DrawLine(Pens.Black, 20, y, e.PageBounds.Right - 20, y);
        y += 20;

        // 2) Declaratie
        var decl = @"Declar că, datele furnizate mai sus sunt corecte.

Declar că, sunt de acord ca societatea ... prelucrarea datelor cu caracter personal, precum și furnizarea datelor menționate mai sus sunt voluntare.

Acest consimțământ poate fi revocat în orice moment...";  // completează cu textul tău

        var rect = new RectangleF(20, y, e.PageBounds.Width - 40, e.PageBounds.Height - y - 40);
        g.DrawString(decl, fontText, Brushes.Black, rect);

        // 3) Semnătură și dată jos
        g.DrawString($"Data: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", fontText, Brushes.Black,
                     new System.Drawing.PointF(20, e.PageBounds.Height - 60));
        g.DrawString("Semnătura: ______________________", fontText, Brushes.Black,
                     new System.Drawing.PointF(20, e.PageBounds.Height - 40));
    }
}

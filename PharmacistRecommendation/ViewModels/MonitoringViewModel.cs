using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using System.Collections.ObjectModel;


namespace PharmacistRecommendation.ViewModels;

public partial class MonitoringViewModel : ObservableObject
{
    private readonly IMonitoringService _monitoringService;
    private readonly IPatientService _patientService;
    private readonly IPdfReportService _pdfReportService;
    private readonly IEmailService _emailService;

    public MonitoringViewModel(IMonitoringService monitoringService,
                               IPatientService patientService, IPdfReportService pdfReportService, IEmailService emailService)
    {
        _monitoringService = monitoringService;
        _patientService = patientService;
        _pdfReportService = pdfReportService;
        _emailService = emailService;

        StartDate = DateTime.Today.AddDays(-7);
        EndDate = DateTime.Today;
    }

    public ObservableCollection<string> MonitoringTypes { get; } =
        new() { "cardio", "diabetes", "temperature" };

    [ObservableProperty] private string selectedMonitoringType = "cardio";

    public int PatientId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [ObservableProperty] private string cardNumber;
    [ObservableProperty] private string name;
    [ObservableProperty] private string cnp;
    [ObservableProperty] private string cid;
    [ObservableProperty] private int age;
    [ObservableProperty] private string gender;
    [ObservableProperty] private string patientEmail;

    [ObservableProperty] private decimal height;
    [ObservableProperty] private decimal weight;

    [ObservableProperty] private int? maxBloodPressure;
    [ObservableProperty] private int? minBloodPressure;
    [ObservableProperty] private int? heartRate;
    [ObservableProperty] private int? pulseOximetry;

    [ObservableProperty] private decimal? bloodGlucose;

    [ObservableProperty] private decimal? bodyTemperature;

    [ObservableProperty] private ObservableCollection<object> historyList = [];

    partial void OnCardNumberChanged(string oldValue, string newValue)
    {
        if (!string.IsNullOrWhiteSpace(newValue) && newValue.Length >= 7) // a se modifica cu 16!!! nr de cifre din cardul de sanatate
            _ = SearchPatientAsync();
    }

    [RelayCommand]
    private async Task SearchPatientAsync()
    {
        if (string.IsNullOrWhiteSpace(CardNumber))
            return;

        var patient = await _patientService.GetPatientByCardCodeAsync(CardNumber);

        if (patient is null)
        {
            Name = Cnp = Cid = Gender = string.Empty;
            Age = 0;
            PatientId = 0;
            return;
        }

        Name = $"{patient.FirstName} {patient.LastName}";
        Cnp = patient.Cnp!;
        Cid = patient.Cid!;
        Age = PatientHelper.CalculateAge(patient.Birthdate);
        Gender = patient.Gender!;
        PatientId = patient.Id;
        PatientEmail = patient.Email!;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (PatientId == 0)      // nu s-a selectat pacient
        {
            await Shell.Current.DisplayAlert("Avertizare", "Datele pacientului nu au fost introduse.", "OK");
            return;
        }

        var dto = new MonitoringDTO
        {
            PatientId = PatientId, // de inlocuit cu PatientId
            CardId = null,               
            MonitoringDate = DateTime.Now,
            MonitoringType = SelectedMonitoringType,
            Height = Height,
            Weight = Weight,
            Notes = string.Empty,

            // cardio
            MaxBloodPressure = MaxBloodPressure,
            MinBloodPressure = MinBloodPressure,
            HeartRate = HeartRate,
            PulseOximetry = PulseOximetry,

            // diabetes
            BloodGlucose = BloodGlucose,

            // temperature
            BodyTemperature = BodyTemperature
        };

        await _monitoringService.AddMonitoringAsync(dto, LoggedInUserId); // ia user-id din sesiune

        await Shell.Current.DisplayAlert("Success", "Datele au fost salvate!", "OK");

        await LoadHistoryAsync();
    }

    [RelayCommand]                    // va genera LoadHistoryCommand
    private async Task LoadHistoryAsync()
    {
        if (PatientId == 0) return;

        HistoryList.Clear();

        var start = StartDate.Date;
        var end = EndDate.Date.AddDays(1).AddTicks(-1);

        var rows = await _monitoringService.GetHistoryAsync(PatientId, start, end);

        foreach (var row in rows)
            HistoryList.Add(row);
    }

    [RelayCommand]
    private async Task GeneratePdfAsync()
    {
        var path = await _pdfReportService.CreatePatientReportAsync(PatientId,
            StartDate, EndDate);

        await Launcher.Default.OpenAsync(new OpenFileRequest("Raport", new ReadOnlyFile(path)));
    }

    [RelayCommand]
    private async Task SendEmailAsync()
    {
        var patient = await _patientService.GetByIdAsync(PatientId);
        if (patient == null)
        {
            await Shell.Current.DisplayAlert("Eroare", "Pacientul nu a fost găsit în sistem.", "OK");
            return;
        }

        string patientEmail = patient.Email?.Trim();
        if (string.IsNullOrWhiteSpace(patientEmail) || !patientEmail.Contains("@"))
        {
            patientEmail = await Shell.Current.DisplayPromptAsync(
                title: "Adresă e-mail",
                message: $"Pacientul {patient.LastName} {patient.FirstName} nu are e-mail salvat.\nIntrodu adresa:",
                accept: "OK",
                cancel: "Renunță",
                placeholder: "ex: ion.popescu@mail.com");

            if (string.IsNullOrWhiteSpace(patientEmail) || !patientEmail.Contains("@"))
            {
                await Shell.Current.DisplayAlert("Anulat", "Email invalid sau trimiterea a fost anulată.", "OK");
                return;
            }
        }

        var pdfPath = await _pdfReportService.CreatePatientReportAsync(PatientId, StartDate, EndDate);

        string subject = $"Raport monitorizare - {patient.LastName} {patient.FirstName}";
        string body = $"Buna ziua,%0A%0AAtașat găsiți raportul de monitorizare pentru perioada {StartDate:dd.MM.yyyy} – {EndDate:dd.MM.yyyy}.%0A%0AVă mulțumim!";

        // Nu mai scapi nimic la întâmplare: encodezi manual doar dacă e nevoie
        var mailtoUrl = $"mailto:{patientEmail}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
        System.Diagnostics.Debug.WriteLine(mailtoUrl);

        try
        {
            await Launcher.Default.OpenAsync(mailtoUrl);
            await Shell.Current.DisplayAlert("Atașează fișierul", $"Emailul este pregătit, dar trebuie să atașezi manual PDF-ul:\n{pdfPath}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Eroare", $"Trimiterea a eșuat: {ex.Message}", "OK");
        }
    }


    private int LoggedInUserId =>
        Preferences.Get("LoggedInUserId", 1);// de inlocuit 1 cu 0
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Models;
using Entities.Services;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;

namespace PharmacistRecommendation.ViewModels;

[QueryProperty(nameof(MonitoringId), "MonitoringId")]
[QueryProperty(nameof(IsReportViewMode), "IsReportViewMode")]
public partial class MonitoringViewModel : ObservableObject, IDisposable
{
    private readonly IMonitoringService _monitoringService;
    private readonly IPatientService _patientService;
    private readonly IPdfReportService _pdfReportService;
    private readonly IEmailConfigurationService _emailConfigurationService;
    private readonly IPharmacyService _pharmacyService;
    private readonly IPharmacyCardService _pharmacyCardService;
    private readonly int _pharmacyId;

    // Added for debounce
    private CancellationTokenSource? _debounceCts;
    private bool _isFillingPatientData;

    public MonitoringViewModel(IMonitoringService monitoringService, IPatientService patientService, IPdfReportService pdfReportService, IEmailConfigurationService emailConfigurationService, IPharmacyService pharmacyService, IPharmacyCardService pharmacyCardService)
    {
        _monitoringService = monitoringService;
        _patientService = patientService;
        _pdfReportService = pdfReportService;
        _emailConfigurationService = emailConfigurationService;
        _pharmacyService = pharmacyService;
        _pharmacyCardService = pharmacyCardService;

        // Initialize lists
        MonitoringTypes = new ObservableCollection<string>
        {
            "cardio",
            "diabetes",
            "temperature"
        };

        StartDate = DateTime.Today.AddDays(-7);
        EndDate = DateTime.Today;

        _pharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1;
        loggedInUserId = SessionManager.GetCurrentUserId() ?? 1;
    }

    public ObservableCollection<string> MonitoringTypes { get; } =
        new() { "cardio", "diabetes", "temperature" };

    [ObservableProperty] private string selectedMonitoringType = "cardio";

    public int PatientId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [ObservableProperty] private string? cardNumber;
    [ObservableProperty] private string? firstName;
    [ObservableProperty] private string? lastName;
    [ObservableProperty] private string? cnp;
    [ObservableProperty] private string? cid;
    [ObservableProperty] private int? age;
    [ObservableProperty] private string? gender;
    [ObservableProperty] private string? patientEmail;

    [ObservableProperty] private decimal? height;
    [ObservableProperty] private decimal? weight;

    [ObservableProperty] private int? maxBloodPressure;
    [ObservableProperty] private int? minBloodPressure;
    [ObservableProperty] private int? heartRate;
    [ObservableProperty] private int? pulseOximetry;

    [ObservableProperty] private decimal? bloodGlucose;

    [ObservableProperty] private decimal? bodyTemperature;

    [ObservableProperty] private ObservableCollection<object> historyList = [];

    [ObservableProperty] private string? _validationMessage;

    private int loggedInUserId { get; set; }

    private int _monitoringId;
    public int MonitoringId
    {
        get => _monitoringId;
        set
        {
            _monitoringId = value;
            if (value > 0)
                _ = LoadMonitoringAsync(value);
        }
    }

    [ObservableProperty]
    bool isReportViewMode = false;

    public bool IsSaveButtonVisible => true;

    partial void OnIsReportViewModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsSaveButtonVisible));
    }


    partial void OnCardNumberChanged(string? oldValue, string? newValue)
    {
        if (_isFillingPatientData) return;
        if (!string.IsNullOrWhiteSpace(newValue) && newValue.Length >= 3)
            _ = SearchPatientAsync();
    }

    partial void OnFirstNameChanged(string? oldValue, string? newValue)
    {
        if (_isFillingPatientData) return;
        if (!string.IsNullOrWhiteSpace(newValue))
            _ = SearchPatientAsync();
    }

    partial void OnLastNameChanged(string? oldValue, string? newValue)
    {
        if (_isFillingPatientData) return;
        if (!string.IsNullOrWhiteSpace(newValue))
            _ = SearchPatientAsync();
    }

    partial void OnCnpChanged(string? oldValue, string? newValue)
    {
        if (_isFillingPatientData) return;
        if (!string.IsNullOrWhiteSpace(newValue) && newValue.Length >= 5)
            _ = SearchPatientAsync();
    }

    [RelayCommand]
    private async Task SearchPatientAsync()
    {
        if (!string.IsNullOrWhiteSpace(CardNumber) || !string.IsNullOrWhiteSpace(Cnp) || 
           (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName)))
        {
            // Cancel and dispose previous search
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            try
            {
                // Debounce - wait for user to stop typing
                await Task.Delay(500, token);
                
                if (token.IsCancellationRequested) return;

                Patient? patient = null;

                if (!string.IsNullOrWhiteSpace(CardNumber) || !string.IsNullOrWhiteSpace(Cnp))
                {
                    patient = await _patientService.GetPatientAsync(CardNumber, Cnp, FirstName, LastName);
                }
                else
                {
                    patient = await _patientService.GetPatientAsync(null, null, FirstName, LastName);
                }

                if (token.IsCancellationRequested) return;

                await FillPatientData(patient);
            }
            catch (TaskCanceledException)
            {
                // Expected when new search starts
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching patient: {ex.Message}");
            }
        }
    }

    private async Task FillPatientData(Patient? patient)
    {
        if (patient is null)
            return;

        _isFillingPatientData = true;
        try
        {
            CardNumber = patient.CardNumber ?? CardNumber;
            FirstName = patient.FirstName;
            LastName = patient.LastName;
            Cnp = patient.Cnp!;
            Cid = patient.Cid!;
            Age = PatientHelper.CalculateAge(patient.Birthdate);
            Gender = patient.Gender!;
            PatientId = patient.Id;
            PatientEmail = patient.Email;
        }
        finally
        {
            _isFillingPatientData = false;
        }

        await LoadHistoryAsync();
    }


    [RelayCommand]
    private async Task SaveAsync()
    {

        Patient patient;
        int? cardId = null;

        if (!string.IsNullOrWhiteSpace(cardNumber))
        {
            var card = await _pharmacyCardService.CreateCardAsync(
                cardNumber, _pharmacyId, FirstName ?? "-", LastName ?? "-", Cnp, Cid, PatientEmail, null, Gender, Age.HasValue ? DateTime.Today.AddYears(-Age.Value) : null);
            patient = card.Patient;
            cardId = card.Id;
        }
        else
        {
            var patientDto = new Patient
            {
                FirstName = FirstName ?? "-",
                LastName = LastName ?? "-",
                Cnp = Cnp,
                Gender = Gender
            };
            patient = await _patientService.GetOrCreatePatientAsync(null, patientDto);
            cardId = patient.PharmacyCards?.FirstOrDefault()?.Id;
        }

        var dto = new MonitoringDTO
        {
            PatientId = patient.Id, 
            CardId = cardId,
            MonitoringDate = MonitoringId > 0 ? EndDate : DateTime.Now, // Past monitoring keeps its date; new monitoring gets now
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

        if (MonitoringId > 0)
        {
            await _monitoringService.UpdateMonitoringAsync(MonitoringId, dto);
        }
        else
        {
            await _monitoringService.AddMonitoringAsync(dto, loggedInUserId); 
        }

        await Shell.Current.DisplayAlert("Succes", "Datele au fost salvate!", "OK");

        await LoadHistoryAsync();
    }

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        var patientIds = new List<int>();

        if (!string.IsNullOrWhiteSpace(cnp))
        {
            var p = await _patientService.GetByCnpAsync(cnp.Trim());
            if (p != null) patientIds.Add(p.Id);
        }

        if (!string.IsNullOrWhiteSpace(cardNumber))
        {
            var p = await _patientService.GetByCardCodeAsync(cardNumber.Trim());
            if (p != null && !patientIds.Contains(p.Id)) patientIds.Add(p.Id);
        }

        if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
        {
            var p = await _patientService.GetByNameAsync(firstName.Trim(), lastName.Trim());
            if (p != null && !patientIds.Contains(p.Id)) patientIds.Add(p.Id);
        }

        if (!patientIds.Any())
        {
            ValidationMessage = "Nu există date pentru acest pacient.";
            return;
        }

        PatientId = patientIds.First(); 

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (HistoryList != null)
                HistoryList.Clear();
            else
                HistoryList = new ObservableCollection<object>();
        });

        ValidationMessage = string.Empty;

        var start = StartDate.Date;
        var end = EndDate.Date.AddDays(1).AddTicks(-1);

        var rows = await _monitoringService.GetHistoryByPatientIdsAsync(patientIds, start, end);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var row in rows)
                HistoryList.Add(row);
        });
    }


    [RelayCommand]
    private async Task GeneratePdfAsync()
    {
        var printDoc = await _pdfReportService.CreateMonitoringPatientReportAsync(PatientId, StartDate, EndDate);

        var pd = new System.Windows.Forms.PrintDialog();
        pd.Document = printDoc;

        if (pd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            printDoc.Print();
        }
    }



    [RelayCommand]
    private async Task SendEmailAsync()
    {
        var patient = await _patientService.GetPatientAsync(cardNumber, cnp, firstName, lastName);
        if (patient == null)
        {
            await Shell.Current.DisplayAlert("Eroare", "Pacientul nu a fost găsit în sistem.", "OK");
            return;
        }

        var pharmacy = await _pharmacyService.GetByIdAsync(_pharmacyId);
        string patientEmail = (PatientEmail?.Trim()) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(patientEmail) || !patientEmail.Contains("@"))
        {
            patientEmail = await Shell.Current.DisplayPromptAsync(
                title: "Adresă e-mail",
                message: $"Pacientul {patient.LastName} {patient.FirstName} nu are e-mail salvat.\nIntroduceți adresa:",
                accept: "OK",
                cancel: "Renunță",
                placeholder: "ex: ion.popescu@mail.com");

            if (string.IsNullOrWhiteSpace(patientEmail) || !patientEmail.Contains("@"))
            {
                await Shell.Current.DisplayAlert("Anulat", "Email invalid sau trimiterea a fost anulată.", "OK");
                return;
            }
        }

        var config = await _emailConfigurationService.GetByPharmacyIdAsync(_pharmacyId);
        if (config == null || string.IsNullOrWhiteSpace(config.Username) || string.IsNullOrWhiteSpace(config.Password))
        {
            await Shell.Current.DisplayAlert("Eroare", "Configurarea email nu este completă. Verificați pagina de configurare.", "OK");
            return;
        }

        var pdfPath = await _pdfReportService.CreateMonitoringPatientReportEmailAsync(PatientId, StartDate, EndDate);
        string subject = $"Raport monitorizare - {patient.LastName} {patient.FirstName}";
        string body =
      $"Bună ziua,\n\n" +
      $"Atașat găsiți raportul de monitorizare pentru perioada {StartDate:dd.MM.yyyy} – {EndDate:dd.MM.yyyy}.\n\n" +
      $"Cu stimă,\n" +
      $"{pharmacy.Name}, {pharmacy.Address}";

        try
        {
            var emailConfig = new Helpers.EmailConfiguration
            {
                SenderEmail = config.Username,
                SenderAppPassword = config.Password,
            };

            var sender = new EmailSenderService(emailConfig);
            await sender.SendEmailWithAttachmentAsync(pharmacy.Name, patientEmail, subject, body, pdfPath);

            await Shell.Current.DisplayAlert("Succes", "Emailul a fost trimis cu succes.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Eroare", $"Trimiterea eșuată: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void Clear()
    {
        CardNumber = string.Empty;
        Cnp = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Cid = string.Empty;
        Age = null;
        Gender = null;
        Weight = null;
        Height = null;
        MaxBloodPressure = null;
        MinBloodPressure = null;
        HeartRate = null;
        PulseOximetry = null;
        BloodGlucose = null;
        BodyTemperature = null;
        PatientEmail = string.Empty;
        HistoryList = null;
    }

    //Reports methods
    private async Task LoadMonitoringAsync(int id)
    {
        try
        {
        var monitoring = await _monitoringService.GetMonitoringByIdAsync(id);
        if (monitoring == null)
            return;

        EndDate = monitoring.MonitoringDate;
        StartDate = EndDate.AddMonths(-1);

        Height = monitoring.Height;
        Weight = monitoring.Weight;

        FirstName = monitoring.Patient.FirstName;
        LastName = monitoring.Patient.LastName;
        Cnp = monitoring.Patient.Cnp;
        Cid = monitoring.Patient.Cid;
        CardNumber = monitoring.Patient.CardNumber ?? string.Empty;

        PatientId = monitoring.PatientId;

        if (!string.IsNullOrWhiteSpace(monitoring.ParametersJson))
        {
            try
            {
                var p = JsonSerializer.Deserialize<MonitoringParameters>(monitoring.ParametersJson);

                MaxBloodPressure = p?.MaxBloodPressure;
                MinBloodPressure = p?.MinBloodPressure;
                HeartRate = p?.HeartRate;
                PulseOximetry = p?.PulseOximetry;
                BloodGlucose = p?.BloodGlucose;
                BodyTemperature = p?.BodyTemperature;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to deserialize monitoring parameters for id {monitoring.Id}: {ex.Message}");
            }
        }

        await LoadHistoryForPatientAsync(PatientId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load monitoring {id}: {ex.Message}");
        }
    }

    private async Task LoadHistoryForPatientAsync(int patientId)
    {
        if (patientId <= 0)
            return;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (HistoryList != null)
                HistoryList.Clear();
            else
                HistoryList = new ObservableCollection<object>();
        });

        ValidationMessage = string.Empty;

        var rows = await _monitoringService.GetHistoryByPatientIdsAsync(
            new List<int> { patientId },
            StartDate.Date,
            EndDate.Date.AddDays(1).AddTicks(-1)
        );

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var row in rows)
                HistoryList.Add(row);
        });
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;
    }
}
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

    public MonitoringViewModel(IMonitoringService monitoringService,
                               IPatientService patientService)
    {
        _monitoringService = monitoringService;
        _patientService = patientService;

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
        if (!string.IsNullOrWhiteSpace(newValue) && newValue.Length >= 6)
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
            PatientId = 1, // de inlocuit cu PatientId
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


    private int LoggedInUserId =>
        Preferences.Get("LoggedInUserId", 1);// de inlocuit 1 cu 0
}
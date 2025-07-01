using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Data;
using Entities.Models;
using Entities.Services;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using System.Collections.ObjectModel;
using static Microsoft.Maui.ApplicationModel.Permissions;
using Windows.Networking;

namespace PharmacistRecommendation.ViewModels;

public partial class CardioMonitoringViewModel : ObservableObject
{
    private readonly IMonitoringService _monitoringService;
    private readonly ICardioMonitoringService _cardioMonitoringService;
    private readonly IPatientService _patientService;
    public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-7);
    public DateTime EndDate { get; set; } = DateTime.Now;

    public CardioMonitoringViewModel(IMonitoringService monitoringService, ICardioMonitoringService cardioMonitoringService, IPatientService patientService)
    {
        _monitoringService = monitoringService;
        _cardioMonitoringService = cardioMonitoringService;
        _patientService = patientService;
    }

    public int PatientId { get; set; }

    [ObservableProperty]
    private string cardNumber;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string cnp;

    [ObservableProperty]
    private string cid;

    [ObservableProperty]
    private int age;

    [ObservableProperty]
    private string gender;

    [ObservableProperty]
    private decimal weight;

    [ObservableProperty]
    private decimal height;

    [ObservableProperty]
    private decimal? maxBloodPressure;

    [ObservableProperty]
    private decimal? minBloodPressure;

    [ObservableProperty]
    private int? heartRate;

    [ObservableProperty]
    private decimal? pulseOximetry;

    [ObservableProperty]
    private ObservableCollection<CardioMonitoringDTO> historyList = [];

    partial void OnCardNumberChanged(string oldValue, string newValue)
    {
        if (!string.IsNullOrWhiteSpace(newValue) && newValue.Length >= 5) 
        {
            _ = SearchPatientAsync(); 
        }
    }

    [RelayCommand]
    private async Task SearchPatientAsync()
    {
        if (string.IsNullOrEmpty(CardNumber))
        {
            return; // Sa afisez un mesj de eroare daca nu e completat campul acesta!
        }

        var patient = await _patientService.GetPatientByCardCodeAsync(CardNumber);

        if (patient != null)
        {
            Name = $"{patient.FirstName} {patient.LastName}";
            Cnp = patient.Cnp!;
            Cid = patient.Cid!;
            Age = PatientHelper.CalculateAge(patient.Birthdate);
            Gender = patient.Gender!;
            PatientId = patient.Id;
        }
        else
        {
            Name = Cnp = Cid = Gender = string.Empty;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (PatientId == 0)
        {
            return;
        }

        var monitoring = new Monitoring
        {
            PatientId = 1021, // !!!! this.PatientId
            MonitoringDate = DateTime.Now,
            Height = this.Height,
            Weight = this.Weight
        };

        await _monitoringService.SaveMonitoringAsync(monitoring);

        var cardioMonitoring = new CardioMonitoring
        {
            Monitoring = monitoring,
            MaxBloodPressure = this.MaxBloodPressure,
            MinBloodPressure = this.MinBloodPressure,
            HeartRate = this.HeartRate,
            PulseOximetry = this.PulseOximetry
        };

        await _cardioMonitoringService.SaveCardioMonitoringAsync(cardioMonitoring);
    }

    [RelayCommand]
    private async Task LoadHistory()
    {
        try
        {
            //to be deleted
            PatientId = 1;

           // HistoryList.Clear();

            var result = await _cardioMonitoringService.GetHistoryAsync(PatientId, StartDate, EndDate);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                HistoryList.Clear(); 
                foreach (var item in result)
                {
                    HistoryList.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Eroare", ex.Message, "OK");
        }
    }
}
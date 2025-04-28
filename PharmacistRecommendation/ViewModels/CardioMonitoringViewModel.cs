using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Data;
using Entities.Models;
using Entities.Services;
using Entities.Services.Interfaces;

namespace PharmacistRecommendation.ViewModels;

public partial class CardioMonitoringViewModel : ObservableObject
{
    private readonly ICardioMonitoringService _cardioService;
    private readonly IMonitoringService _monitoringService;

    public CardioMonitoringViewModel(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
    }

    public int PatientId { get; set; }

    [ObservableProperty]
    private string cardNumber;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string cnp;

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

    [RelayCommand]
    private async Task SearchPatientAsync()
    {
        if (string.IsNullOrEmpty(CardNumber))
        {
            return; // Sa afisez un mesj de eroare daca nu e completat campul acesta!
        }

        //var patient = await _patientService.GetPatientByCardNumberAsync(CardNumber);
        //if (patient == null)
        //{
        //    return;
        //}

        //PatientId = patient.Id; // Seteaza ID-ul pacientului

        PatientId = 1;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        //if (PatientId == 0)
        //{
        //    // Daca pacientul nu este gasit, nu se poate salva monitorizarea
        //    return;
        //}

        var monitoring = new Monitoring
        {
            PatientId = 1, // !!!! this.PatientId
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

        await _monitoringService.SaveCardioMonitoringAsync(cardioMonitoring);
    }
}
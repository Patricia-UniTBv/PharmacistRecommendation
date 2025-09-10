using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Models;
using Entities.Services;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Helpers.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    [QueryProperty(nameof(Mode), "mode")]
    public partial class MixedActIssuanceViewModel : ObservableObject
    {
        private string PrescriptionsPath { get; set; }
        private string ReceiptsPath { get; set; }

        private readonly IPrescriptionService _prescriptionService;
        private readonly IAdministrationModeService _administrationModeService;
        private readonly IPharmacyService _pharmacyService;
        private readonly IImportConfigurationService _importService;
        private readonly IMedicationService _medicationService;

        [ObservableProperty]
        string mode;

        partial void OnModeChanged(string value)
        {
            ShowWithPrescription = value == "mixed" || value == "withprescription";
            ShowWithoutPrescription = value == "mixed" || value == "withoutprescription";

            if (ShowWithPrescription && !ShowWithoutPrescription)
            {
                PageTitle = "Emitere act consecutiv prescripției";
            }
            else if (!ShowWithPrescription && ShowWithoutPrescription)
            {
                PageTitle = "Emitere act propriu";
            }
        }

        private int pharmacyId { get; set; }
        [ObservableProperty]
        public bool showWithPrescription = true;
        [ObservableProperty]
        public bool showWithoutPrescription = true;
        [ObservableProperty]
        string pageTitle = "Emitere act mixt";
        [ObservableProperty]
        string? cardNumber;
        [ObservableProperty]
        string? patientName;
        [ObservableProperty]
        string? patientCnp;
        [ObservableProperty]
        string? caregiverName;
        [ObservableProperty]
        string? caregiverCnp;
        [ObservableProperty]
        string? patientDiagnosis;
        [ObservableProperty]
        string? usedMedications;
        [ObservableProperty]
        string? doctorStamp;
        [ObservableProperty]
        string? prescriptionSeries;
        [ObservableProperty]
        string? prescriptionNumber;
        [ObservableProperty]
        string? prescriptionDiagnosis;
        [ObservableProperty]
        string? symptoms;
        [ObservableProperty]
        string? suspicion;
        [ObservableProperty]
        string? pharmacistObservations;
        [ObservableProperty]
        ObservableCollection<PrescriptionDrugModel> medicationsWithPrescription = new();
        [ObservableProperty]
        ObservableCollection<ReceiptDrugModel> medicationsWithoutPrescription = new();
        [ObservableProperty]
        string notesToDoctor;
        [ObservableProperty]
        string pharmacistRecommendation;
        [ObservableProperty]
        ObservableCollection<string> pharmaceuticalServices = new() { "Aderență la tratament", "Administrare medicamente", "Consiliere", "Consiliere dispozitive medicale", "Consiliere OTC", "Consiliere RX", "Consiliere suplimente",
        "Farmacovigilenta", "Masurare parametri biologici", "Preparare medicamente", "Teste rapide", "Vaccinare"};
        [ObservableProperty]
        string selectedPharmaceuticalService = "Aderență la tratament";
        [ObservableProperty]
        ObservableCollection<AdministrationMode> administrationModes = new();
        [ObservableProperty]
        AdministrationMode? administrationMode;
        [ObservableProperty]
        bool canPrint = false;
        [ObservableProperty]
        bool isPrintButtonEnabled = false;

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private string selectedMedication;

        public ObservableCollection<string> AllMedications { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Suggestions { get; } = new ObservableCollection<string>();

        public IRelayCommand<string> AddSuggestionCommand { get; }
        public Entry SearchEntryReference { get; set; }

        public ObservableCollection<string> FilteredMedications { get; } = new();
        public bool ShowSuggestions { get; set; }
        private CancellationTokenSource _cts;


        public MixedActIssuanceViewModel(IPrescriptionService prescriptionService, IAdministrationModeService administrationModeService, IPharmacyService pharmacyService, IImportConfigurationService importService, IMedicationService medicationService)
        {
            _prescriptionService = prescriptionService;
            _administrationModeService = administrationModeService;
            _pharmacyService = pharmacyService;
            _importService = importService;
            _medicationService = medicationService;

            AddSuggestionCommand = new RelayCommand<string>(AddSuggestionToText);

            _ = LoadMedicationsAsync();
            LoadAdministrationModes();

            pharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1;
        }

        public async Task LoadMedicationsAsync()
        {
            var all = await _medicationService.GetAllMedicationsAsync();
            foreach (var med in all)
            {
                AllMedications.Add(med.Denumire);
            }
        }

        public void UpdateSuggestions(string text)
        {
            Suggestions.Clear();

            if (string.IsNullOrWhiteSpace(text))
                return;

            var separators = new char[] { ' ', ',' };
            var parts = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var lastWord = parts.LastOrDefault()?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(lastWord))
                return;

            var filtered = AllMedications
                .Where(m => m.IndexOf(lastWord, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(m => m)
                .Take(20);

            foreach (var med in filtered)
                Suggestions.Add(med);
        }

        //public void UpdateSuggestionsForRow(ReceiptDrugModel row, string query)
        //{
        //    row.FilteredMedications.Clear();

        //    if (!string.IsNullOrWhiteSpace(query))
        //    {
        //        var matches = AllMedications
        //            .Where(m => m.Contains(query, StringComparison.OrdinalIgnoreCase))
        //            .ToList();

        //        foreach (var m in matches)
        //            row.FilteredMedications.Add(m);

        //        row.ShowSuggestions = row.FilteredMedications.Any();
        //    }
        //    else
        //    {
        //        row.ShowSuggestions = false;
        //    }
        //}


        public void AddSuggestionToText(string suggestion)
        {
            var separators = new char[] { ' ', ',' };
            var parts = SearchText?.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(p => p.Trim())
                                   .ToList() ?? new List<string>();

            if (parts.Any())
                parts[parts.Count - 1] = suggestion;
            else
                parts.Add(suggestion);

            SearchText = string.Join(" ", parts);

            Suggestions.Clear();

            Task.Run(async () =>
            {
                await Task.Delay(50);
                MainThread.BeginInvokeOnMainThread(() => SearchEntryReference?.Focus());
            });
        }

        [RelayCommand]
        private async Task ImportDataAsync()
        {
            var imp = await _importService.GetById(pharmacyId);
            if (imp == null)
            {
                await ShowAlert("Configurați calea directoarelor!");
            }
            else
            {
                ReceiptsPath = imp.ReceiptPath!;
                PrescriptionsPath = imp.PrescriptionPath!;
            }

            try
            {
                if (mode == "mixed" || mode == "withoutprescription")
                {

                    var lastFolder = ReceiptImportService.GetLastDatedFolder(ReceiptsPath);
                    if (lastFolder == null)
                    {
                        await ShowAlert("Nu există fișier de importat.");
                        return;
                    }
                    string logFile = ReceiptImportService.FindTextOrLogFile(lastFolder);
                    var import = ReceiptImportService.ImportLastReceipt(logFile);

                    MedicationsWithoutPrescription.Clear();
                    foreach (var drug in import.Medications)
                    {
                        MedicationsWithoutPrescription.Add(drug);
                    }
                }
                if (mode == "mixed" || mode == "withprescription")
                {
                    string filePath = PrescriptionImportService.GetLastPrescriptionFile(PrescriptionsPath);
                    if (filePath == null)
                    {
                        await ShowAlert("Nu există fișier de importat.");
                        return;
                    }
                    var import = PrescriptionImportService.ImportFromXml(filePath);

                    PatientCnp = import.PatientCnp;
                    PrescriptionSeries = import.PrescriptionSeries;
                    PrescriptionNumber = import.PrescriptionNumber;
                    DoctorStamp = import.DoctorStamp;
                    PrescriptionDiagnosis = import.Diagnosis;

                    MedicationsWithPrescription.Clear();
                    foreach (var drug in import.Drugs)
                    {
                        MedicationsWithPrescription.Add(drug);
                    }
                }
                if (mode == "mixed")
                {
                    var prescriptionNames = MedicationsWithPrescription.Select(m => m.Name).ToHashSet();

                    var filtered = MedicationsWithoutPrescription
                        .Where(m => !prescriptionNames.Contains(m.Name))
                        .ToList();

                    MedicationsWithoutPrescription.Clear();
                    foreach (var med in filtered)
                        MedicationsWithoutPrescription.Add(med);
                }

            }
            catch (Exception ex)
            {
                await ShowAlert($"Eroare la import: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(PatientName) && string.IsNullOrWhiteSpace(CaregiverName))
            {
                await ShowAlert("Completează numele pacientului/aparținătorului!");
                return;
            }
            if (string.IsNullOrWhiteSpace(PatientCnp) && string.IsNullOrWhiteSpace(CaregiverCnp))
            {
                await ShowAlert("Completează CNP-ul pacientului/aparținătorului!");
                return;
            }

            var prescription = new Prescription
            {
                PatientName = this.PatientName,
                PatientCnp = this.PatientCnp,
                CaregiverName = this.CaregiverName,
                CaregiverCnp = this.CaregiverCnp,
                Number = this.PrescriptionNumber,
                Series = this.PrescriptionSeries,
                Diagnostic = this.PrescriptionDiagnosis,
                DiagnosisMentionedByPatient = this.PatientDiagnosis,
                Symptoms = this.Symptoms,
                Suspicion = this.Suspicion,
                PharmacistObservations = this.PharmacistObservations,
                NotesToDoctor = this.NotesToDoctor,
                PharmacistRecommendation = this.PharmacistRecommendation,
                PharmaceuticalService = this.SelectedPharmaceuticalService,
                DoctorStamp = this.DoctorStamp,
                IssueDate = DateTime.Now,
                PrescriptionMedications = this.MedicationsWithPrescription
         .Select(m => new PrescriptionMedication
         {
             Name = m.Name,
             Morning = m.Morning,
             Noon = m.Noon,
             Evening = m.Evening,
             Night = m.Night,
             AdministrationModeId = m.AdministrationMode?.Id,
             IsWithPrescription = true
         })
         .Concat(
             this.MedicationsWithoutPrescription
                 .Select(m => new PrescriptionMedication
                 {
                     Name = m.Name,
                     Morning = m.Morning,
                     Noon = m.Noon,
                     Evening = m.Evening,
                     Night = m.Night,
                     AdministrationModeId = m.AdministrationMode?.Id,
                     IsWithPrescription = false
                 })
         )
         .ToList()
            };

            await _prescriptionService.AddPrescriptionAsync(prescription);
            IsPrintButtonEnabled = true;
            await ShowAlert("Rețeta a fost salvată cu succes!");
        }

        public async void FilterMedications(string searchText, PrescriptionDrugModel drug)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(100, _cts.Token); // debounce

                // Ensure we're on the main thread for UI updates
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    drug.FilteredMedications.Clear();

                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        drug.ShowSuggestions = false;
                        return;
                    }

                    var filtered = AllMedications
                        .Where(m => m.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Take(10)
                        .ToList(); // Materialize the query

                    foreach (var med in filtered)
                    {
                        drug.FilteredMedications.Add(med);
                    }

                    drug.ShowSuggestions = drug.FilteredMedications.Any();

                    // Force property change notification
                    OnPropertyChanged(nameof(drug.FilteredMedications));
                    OnPropertyChanged(nameof(drug.ShowSuggestions));
                });
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelled, do nothing
                return;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error filtering medications: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    drug.ShowSuggestions = false;
                });
            }
        }

        [RelayCommand]
        public async void SelectMedication(string medName)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var drug in MedicationsWithPrescription)
                {
                    if (drug.ShowSuggestions && drug.FilteredMedications.Contains(medName))
                    {
                        drug.Name = medName;
                        drug.ShowSuggestions = false;
                        drug.FilteredMedications.Clear();
                        break;
                    }
                }
            });
        }

        [RelayCommand]
        private void AddMedicationWithPrescription()
        {
            MedicationsWithPrescription.Add(new PrescriptionDrugModel { Index = MedicationsWithPrescription.Count + 1 });
        }

        [RelayCommand]
        private void AddMedicationWithoutPrescription()
        {
            MedicationsWithoutPrescription.Add(new ReceiptDrugModel { Index = MedicationsWithoutPrescription.Count + 1 });
        }

        [RelayCommand]
        void DeleteMedicationWithPrescription(PrescriptionDrugModel item)
        {
            if (item != null && MedicationsWithPrescription.Contains(item))
            {
                MedicationsWithPrescription.Remove(item);
                for (int i = 0; i < MedicationsWithPrescription.Count; i++)
                {
                    MedicationsWithPrescription[i].Index = i + 1;
                }
            }
        }

        [RelayCommand]
        void DeleteMedicationWithoutPrescription(ReceiptDrugModel item)
        {
            if (item != null && MedicationsWithoutPrescription.Contains(item))
            {
                MedicationsWithoutPrescription.Remove(item);
                for (int i = 0; i < MedicationsWithoutPrescription.Count; i++)
                {
                    MedicationsWithoutPrescription[i].Index = i + 1;
                }
            }
        }

        [RelayCommand]
        private void NewRecommendation()
        {
            CardNumber = PatientName = PatientCnp = CaregiverName = CaregiverCnp = PatientDiagnosis = UsedMedications = "";
            DoctorStamp = PrescriptionSeries = PrescriptionNumber = PrescriptionDiagnosis = "";
            Symptoms = Suspicion = PharmacistObservations = NotesToDoctor = PharmacistRecommendation = "";
            MedicationsWithPrescription.Clear();
            MedicationsWithoutPrescription.Clear();
        }

        [RelayCommand]
        private async void Print()
        {
            var _pharmacy = await _pharmacyService.GetByIdAsync(pharmacyId);

            var medsWithPrescription = MedicationsWithPrescription.Select(m => new ActPrintDocument.MedicationLine
            {
                Name = m.Name,
                Morning = m.Morning,
                Noon = m.Noon,
                Evening = m.Evening,
                Night = m.Night,
                AdministrationMode = m.AdministrationMode?.Name ?? ""
            }).ToList();

            var medsWithoutPrescription = MedicationsWithoutPrescription.Select(m => new ActPrintDocument.MedicationLine
            {
                Name = m.Name,
                Morning = m.Morning,
                Noon = m.Noon,
                Evening = m.Evening,
                Night = m.Night,
                AdministrationMode = m.AdministrationMode?.Name ?? ""
            }).ToList();

            var printDoc = new ActPrintDocument
            {
                PharmacyName = _pharmacy!.Name,
                PharmacyAddress = _pharmacy.Address!,
                PharmacyPhone = _pharmacy!.Phone!,
                Logo = _pharmacy!.Logo,
                Series = this.PrescriptionSeries!,
                Number = this.PrescriptionNumber!,
                IssueDate = DateTime.Now,
                PatientName = this.PatientName!,
                PatientCnp = this.PatientCnp!,
                CaregiverName = this.CaregiverName!,
                CaregiverCnp = this.CaregiverCnp!,
                DoctorStamp = this.DoctorStamp!,
                Diagnostic = this.PrescriptionDiagnosis!,
                DiagnosisMentioned = this.PatientDiagnosis!,
                Symptoms = this.Symptoms!,
                Suspicion = this.Suspicion!,
                PharmacistObservations = this.PharmacistObservations!,
                PharmacistRecommendation = this.PharmacistRecommendation,
                PharmaceuticalService = this.SelectedPharmaceuticalService,
                MedicationsWithPrescription = medsWithPrescription,
                MedicationsWithoutPrescription = medsWithoutPrescription
            };

            var pd = new System.Windows.Forms.PrintDialog();
            pd.Document = printDoc;
            if (pd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                printDoc.Print();
            }
        }


        private Task ShowAlert(string message)
        {
            return Shell.Current?.DisplayAlert("Info", message, "OK") ?? Task.CompletedTask;
        }

        private async void LoadAdministrationModes()
        {
            var modes = await _administrationModeService.GetAllAsync();
            var activeModes = modes.Where(c => (bool)c.IsActive).ToList();
            AdministrationModes = new ObservableCollection<AdministrationMode>(activeModes);
        }
    }
}
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
        private const string PrescriptionsPath = @"C:\Users\Patricia\Desktop\Statie_temp";

        private readonly IPrescriptionService _prescriptionService;
        private readonly IAdministrationModeService _administrationModeService;
        private readonly IPharmacyService _pharmacyService;

        [ObservableProperty]
        string mode;

        partial void OnModeChanged(string value)
        {
            ShowWithPrescription = value == "both" || value == "withprescription";
            ShowWithoutPrescription = value == "both" || value == "withoutprescription";

            if(ShowWithPrescription && !ShowWithoutPrescription)
            {
                PageTitle = "Emitere act consecutiv prescripției";
            }
            else if(!ShowWithPrescription && ShowWithoutPrescription)
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
        ObservableCollection<PrescriptionDrugModel> medicationsWithoutPrescription = new();
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

        public ObservableCollection<string> AllMedications { get; } = new()
        {
            "Paracetamol 1",
            "Paracetamol 2",
            "Algocalmin",
            "Nurofen",
            "Ibuprofen",
            "Aspirin",
            "Diclofenac",
            "Ketoprofen",
            "Metamizol",
            "Tramadol",
            "Codeine"
            // a se încărca din DB  !!!!!!!
        };

        public MixedActIssuanceViewModel(IPrescriptionService prescriptionService, IAdministrationModeService administrationModeService, IPharmacyService pharmacyService)
        {
            _prescriptionService = prescriptionService;
            _administrationModeService = administrationModeService;
            _pharmacyService = pharmacyService;
            LoadAdministrationModes();

            pharmacyId = 1; // a se modifica cu ID-ul farmaciei curente!!
        }

        [RelayCommand]
        private async Task ImportDataAsync()
        {
            try
            {
                if (mode == "actpropriu") // IMPORT DIN CASA DE MARCAT
                {
                    // Import doar din bon
                    //var lastBonFile = GetLastReceiptLogFile(caleBonuri); 
                    //var medsFaraPrescriptie = ImportMedicationsFromReceiptLog(lastBonFile);

                    //MedicationsWithoutPrescription = new ObservableCollection<MedicationModel>(
                    //medsFaraPrescriptie.Select((m, i) => new MedicationModel { Name = m, Index = i + 1 }));
                }
                else if (mode == "mixed" || mode == "withprescription")
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
                        drug.DisplayText = drug.Name + " " + drug.Concentration + " " + drug.PharmaceuticalForm + " " + drug.Dose;
                        MedicationsWithPrescription.Add(drug);
                    }
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
            if (string.IsNullOrWhiteSpace(PatientName))
            {
                await ShowAlert("Completează numele pacientului!");
                return;
            }
            if (string.IsNullOrWhiteSpace(PatientCnp))
            {
                await ShowAlert("Completează CNP-ul pacientului!");
                return;
            }
            if (string.IsNullOrWhiteSpace(PrescriptionNumber))
            {
                await ShowAlert("Completează numărul rețetei!");
                return;
            }
            if (string.IsNullOrWhiteSpace(PrescriptionSeries))
            {
                await ShowAlert("Completează seria rețetei!");
                return;
            }
            if (MedicationsWithPrescription.Count == 0)
            {
                await ShowAlert("Adaugă cel puțin un medicament cu rețetă!");
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


        public void FilterMedications(string searchText, PrescriptionDrugModel drug)
        {
            drug.FilteredMedications.Clear();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                drug.ShowSuggestions = false;
                return;
            }

            var filtered = AllMedications
                .Where(m => m.StartsWith(searchText, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            foreach (var med in filtered)
                drug.FilteredMedications.Add(med);

            drug.ShowSuggestions = filtered.Any();
        }

        [RelayCommand]
        public void SelectMedication(string medName)
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
        }

        [RelayCommand]
        private void AddMedicationWithPrescription()
        {
            MedicationsWithPrescription.Add(new PrescriptionDrugModel { Index = MedicationsWithPrescription.Count + 1 });
        }

        [RelayCommand]
        private void AddMedicationWithoutPrescription()
        {
            MedicationsWithoutPrescription.Add(new PrescriptionDrugModel { Index = MedicationsWithoutPrescription.Count + 1 });
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
        void DeleteMedicationWithoutPrescription(PrescriptionDrugModel item)
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
            AdministrationModes = new ObservableCollection<AdministrationMode>(modes);
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Models;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.ViewModels
{
    public partial class MixedActIssuanceViewModel : ObservableObject
    {
        private const string PrescriptionsPath = @"C:\Users\Patricia\Desktop\Statie_temp";

        private readonly IPrescriptionService _prescriptionService;

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
        ObservableCollection<string> pharmaceuticalServices = new() { "Consiliere", "Eliberare rețetă", "Supraveghere tratament" };
        [ObservableProperty]
        string selectedPharmaceuticalService;
        [ObservableProperty]
        bool canPrint = false;

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

        public MixedActIssuanceViewModel(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        [RelayCommand]
        private async Task ImportDataAsync()
        {
            try
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
            catch (Exception ex)
            {
                await ShowAlert($"Eroare la import: {ex.Message}");
            }
        }
        [RelayCommand]
        private async Task SaveAsync()
        {
            // Ex: la ce câmpuri vrei să NU fie nulle/empty
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

            // Mapping către entitățile pentru DB
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
                IssueDate = DateTime.Now, // sau din UI
                PrescriptionMedications = this.MedicationsWithPrescription
                    .Select(m => new PrescriptionMedication
                    {
                        Name = m.Name,
                        Concentration = m.Concentration,
                        PharmaceuticalForm = m.PharmaceuticalForm,
                        Dose = m.Dose,
                        DiseaseCode = m.DiseaseCode,
                        Morning = m.Morning,
                        Noon = m.Noon,
                        Evening = m.Evening,
                        Night = m.Night,
                        // etc.
                    })
                    .ToList()
            };

            await _prescriptionService.AddPrescriptionAsync(prescription);
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
                // Re-index remaining items
                for (int i = 0; i < MedicationsWithPrescription.Count; i++)
                {
                    MedicationsWithPrescription[i].Index = i + 1;
                }
            }
        }

        [RelayCommand]
        private void NewRecommendation()
        {
            // Reset all fields
            CardNumber = PatientName = PatientCnp = CaregiverName = CaregiverCnp = PatientDiagnosis = UsedMedications = "";
            DoctorStamp = PrescriptionSeries = PrescriptionNumber = PrescriptionDiagnosis = "";
            Symptoms = Suspicion = PharmacistObservations = NotesToDoctor = PharmacistRecommendation = "";
            MedicationsWithPrescription.Clear();
            MedicationsWithoutPrescription.Clear();
        }

        [RelayCommand]
        private void Close()
        {
            // Close page logic, depends on your navigation setup
        }

        [RelayCommand]
        private void Print()
        {
            // Print logic
        }

        private Task ShowAlert(string message)
        {
            // Use Shell.Current if available, else adapt for your platform
            return Shell.Current?.DisplayAlert("Info", message, "OK") ?? Task.CompletedTask;
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private const string PrescriptionsPath = @"C:\Users\Patricia\Desktop\Statie_temp"; // can be set from configuration in the future

        [ObservableProperty]
        string cardNumber;
        [ObservableProperty]
        string patientName;
        [ObservableProperty]
        string patientCnp;
        [ObservableProperty]
        string caregiverName;
        [ObservableProperty]
        string caregiverCnp;
        [ObservableProperty]
        string patientDiagnosis;
        [ObservableProperty]
        string usedMedications;
        [ObservableProperty]
        string doctorStamp;
        [ObservableProperty]
        string prescriptionSeries;
        [ObservableProperty]
        string prescriptionNumber;
        [ObservableProperty]
        string prescriptionDiagnosis;
        [ObservableProperty]
        string symptoms;
        [ObservableProperty]
        string suspicion;
        [ObservableProperty]
        string pharmacistObservations;
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

        public MixedActIssuanceViewModel()
        {
            // Optionally: load pharma services from DB
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

                // Populate fields
                PatientCnp = import.PatientCnp;
                PrescriptionSeries = import.PrescriptionSeries;
                PrescriptionNumber = import.PrescriptionNumber;
                DoctorStamp = import.DoctorStamp;
                PrescriptionDiagnosis = import.Diagnosis;

                // Table
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
            // Simple example validation
            if (string.IsNullOrWhiteSpace(PatientCnp) ||
                string.IsNullOrWhiteSpace(PrescriptionNumber) ||
                MedicationsWithPrescription.Count == 0)
            {
                await ShowAlert("Completează toate datele obligatorii și adaugă cel puțin un medicament cu rețetă.");
                return;
            }
            // TODO: Save logic (DB, file, etc)
            await ShowAlert("Datele au fost salvate cu succes!");
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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Entities.Models;
using Entities.Services.Interfaces;

namespace PharmacistRecommendation.ViewModels
{
    public class ConflictResolutionViewModel : INotifyPropertyChanged
    {
        private readonly IMedicationImportService _importService;
        private bool _isLoading;
        private TaskCompletionSource<List<MedicationConflict>>? _completionSource;

        public ConflictResolutionViewModel(IMedicationImportService importService)
        {
            _importService = importService;
            Conflicts = new ObservableCollection<ConflictViewModel>();

            KeepAllExistingCommand = new Command(KeepAllExisting);
            UpdateAllToImportCommand = new Command(UpdateAllToImport);
            ApplySelectionsCommand = new Command(ApplySelections);
        }

        public ObservableCollection<ConflictViewModel> Conflicts { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand KeepAllExistingCommand { get; }
        public ICommand UpdateAllToImportCommand { get; }
        public ICommand ApplySelectionsCommand { get; }

        public async Task<List<MedicationConflict>> ShowConflictsAsync(List<MedicationConflict> conflicts)
        {
            _completionSource = new TaskCompletionSource<List<MedicationConflict>>();

            Conflicts.Clear();
            foreach (var conflict in conflicts)
            {
                Conflicts.Add(new ConflictViewModel(conflict));
            }

            return await _completionSource.Task;
        }

        private void KeepAllExisting()
        {
            foreach (var conflict in Conflicts)
            {
                conflict.KeepExisting = true;
            }
        }

        private void UpdateAllToImport()
        {
            foreach (var conflict in Conflicts)
            {
                conflict.UpdateToImport = true;
            }
        }

        private void ApplySelections()
        {
            var resolvedConflicts = new List<MedicationConflict>();

            foreach (var conflictVm in Conflicts)
            {
                var originalConflict = conflictVm.OriginalConflict;

                if (conflictVm.KeepExisting)
                {
                    originalConflict.Resolution = ConflictResolution.KeepExisting;
                    originalConflict.UserWantsUpdate = false;
                }
                else if (conflictVm.UpdateToImport)
                {
                    originalConflict.Resolution = ConflictResolution.UpdateToImport;
                    originalConflict.UserWantsUpdate = true;
                }
                else if (conflictVm.SkipMedication)
                {
                    originalConflict.Resolution = ConflictResolution.Skip;
                    originalConflict.UserWantsUpdate = false;
                }

                resolvedConflicts.Add(originalConflict);
            }

            _completionSource?.SetResult(resolvedConflicts);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ConflictViewModel : INotifyPropertyChanged
    {
        private bool _keepExisting = true; 
        private bool _updateToImport;
        private bool _skipMedication;

        public ConflictViewModel(MedicationConflict conflict)
        {
            OriginalConflict = conflict;
            ConflictId = Guid.NewGuid().ToString();

            MedicationDisplayName = $"{conflict.ManualMedication.Denumire} ({conflict.ManualMedication.CodCIM})";
            ConflictSummary = $"CodCIM: {conflict.ManualMedication.CodCIM} - {conflict.ConflictingFields.Count} field(s) different";

            ConflictDetails = new ObservableCollection<ConflictDetail>();
            foreach (var field in conflict.ConflictingFields)
            {
                var existingValue = GetFieldValue(conflict.ManualMedication, field);
                var importValue = GetFieldValue(MapCsvToMedication(conflict.CsvData), field);

                ConflictDetails.Add(new ConflictDetail
                {
                    FieldName = field,
                    ExistingValue = existingValue ?? "(empty)",
                    ImportValue = importValue ?? "(empty)"
                });
            }
        }

        public MedicationConflict OriginalConflict { get; }
        public string ConflictId { get; }
        public string MedicationDisplayName { get; }
        public string ConflictSummary { get; }
        public ObservableCollection<ConflictDetail> ConflictDetails { get; }

        public bool KeepExisting
        {
            get => _keepExisting;
            set
            {
                _keepExisting = value;
                if (value)
                {
                    _updateToImport = false;
                    _skipMedication = false;
                    OnPropertyChanged(nameof(UpdateToImport));
                    OnPropertyChanged(nameof(SkipMedication));
                }
                OnPropertyChanged();
            }
        }

        public bool UpdateToImport
        {
            get => _updateToImport;
            set
            {
                _updateToImport = value;
                if (value)
                {
                    _keepExisting = false;
                    _skipMedication = false;
                    OnPropertyChanged(nameof(KeepExisting));
                    OnPropertyChanged(nameof(SkipMedication));
                }
                OnPropertyChanged();
            }
        }

        public bool SkipMedication
        {
            get => _skipMedication;
            set
            {
                _skipMedication = value;
                if (value)
                {
                    _keepExisting = false;
                    _updateToImport = false;
                    OnPropertyChanged(nameof(KeepExisting));
                    OnPropertyChanged(nameof(UpdateToImport));
                }
                OnPropertyChanged();
            }
        }

        private string? GetFieldValue(Medication medication, string fieldName)
        {
            return fieldName switch
            {
                "Denumire" => medication.Denumire,
                "DCI" => medication.DCI,
                "FormaFarmaceutica" => medication.FormaFarmaceutica,
                "Concentratia" => medication.Concentratia,
                "FirmaProducatoare" => medication.FirmaProducatoare,
                "FirmaDetinatoare" => medication.FirmaDetinatoare,
                "CodATC" => medication.CodATC,
                "ActiuneTerapeutica" => medication.ActiuneTerapeutica,
                "Prescriptie" => medication.Prescriptie,
                "NrData" => medication.NrData,
                "Ambalaj" => medication.Ambalaj,
                "VolumAmbalaj" => medication.VolumAmbalaj,
                "Valabilitate" => medication.Valabilitate,
                "Bulina" => medication.Bulina,
                "Diez" => medication.Diez,
                "Stea" => medication.Stea,
                "Triunghi" => medication.Triunghi,
                "Dreptunghi" => medication.Dreptunghi,
                _ => null
            };
        }

        private Medication MapCsvToMedication(CsvMedicationRow csvRow)
        {
            return new Medication
            {
                CodCIM = csvRow.CodCIM,
                Denumire = csvRow.DenumireComericala,
                DCI = csvRow.DCI,
                FormaFarmaceutica = csvRow.FormaFarmaceutica,
                Concentratia = csvRow.Concentratie,
                FirmaProducatoare = csvRow.FirmaProducatoare,
                FirmaDetinatoare = csvRow.FirmaDetinatoare,
                CodATC = csvRow.CodATC,
                ActiuneTerapeutica = csvRow.ActiuneTerapeutica,
                Prescriptie = csvRow.Prescriptie,
                NrData = csvRow.NrDataAmbalaj,
                Ambalaj = csvRow.Ambalaj,
                VolumAmbalaj = csvRow.VolumAmbalaj,
                Valabilitate = csvRow.ValabilitateAmbalaj,
                Bulina = csvRow.Bulina,
                Diez = csvRow.Diez,
                Stea = csvRow.Stea,
                Triunghi = csvRow.Triunghi,
                Dreptunghi = csvRow.Dreptunghi
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ConflictDetail
    {
        public string FieldName { get; set; } = string.Empty;
        public string ExistingValue { get; set; } = string.Empty;
        public string ImportValue { get; set; } = string.Empty;
    }
}
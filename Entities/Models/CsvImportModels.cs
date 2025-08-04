namespace Entities.Models
{
    public class CsvMedicationRow
    {
        public string CodCIM { get; set; } = string.Empty;
        public string DenumireComericala { get; set; } = string.Empty;
        public string DCI { get; set; } = string.Empty;
        public string FormaFarmaceutica { get; set; } = string.Empty;
        public string Concentratie { get; set; } = string.Empty;
        public string FirmaProducatoare { get; set; } = string.Empty;
        public string FirmaDetinatoare { get; set; } = string.Empty;
        public string CodATC { get; set; } = string.Empty;
        public string ActiuneTerapeutica { get; set; } = string.Empty;
        public string Prescriptie { get; set; } = string.Empty;
        public string NrDataAmbalaj { get; set; } = string.Empty;
        public string Ambalaj { get; set; } = string.Empty;
        public string VolumAmbalaj { get; set; } = string.Empty;
        public string ValabilitateAmbalaj { get; set; } = string.Empty;
        public string Bulina { get; set; } = string.Empty;
        public string Diez { get; set; } = string.Empty;
        public string Stea { get; set; } = string.Empty;
        public string Triunghi { get; set; } = string.Empty;
        public string Dreptunghi { get; set; } = string.Empty;
        public string DataActualizare { get; set; } = string.Empty;
        
        // Helper method to convert empty strings to null
        public void NormalizeEmptyStrings()
        {
            if (string.IsNullOrWhiteSpace(CodCIM)) CodCIM = null;
            if (string.IsNullOrWhiteSpace(DenumireComericala)) DenumireComericala = null;
            if (string.IsNullOrWhiteSpace(DCI)) DCI = null;
            if (string.IsNullOrWhiteSpace(FormaFarmaceutica)) FormaFarmaceutica = null;
            if (string.IsNullOrWhiteSpace(Concentratie)) Concentratie = null;
            if (string.IsNullOrWhiteSpace(FirmaProducatoare)) FirmaProducatoare = null;
            if (string.IsNullOrWhiteSpace(FirmaDetinatoare)) FirmaDetinatoare = null;
            if (string.IsNullOrWhiteSpace(CodATC)) CodATC = null;
            if (string.IsNullOrWhiteSpace(ActiuneTerapeutica)) ActiuneTerapeutica = null;
            if (string.IsNullOrWhiteSpace(Prescriptie)) Prescriptie = null;
            if (string.IsNullOrWhiteSpace(NrDataAmbalaj)) NrDataAmbalaj = null;
            if (string.IsNullOrWhiteSpace(Ambalaj)) Ambalaj = null;
            if (string.IsNullOrWhiteSpace(VolumAmbalaj)) VolumAmbalaj = null;
            if (string.IsNullOrWhiteSpace(ValabilitateAmbalaj)) ValabilitateAmbalaj = null;
            if (string.IsNullOrWhiteSpace(Bulina)) Bulina = null;
            if (string.IsNullOrWhiteSpace(Diez)) Diez = null;
            if (string.IsNullOrWhiteSpace(Stea)) Stea = null;
            if (string.IsNullOrWhiteSpace(Triunghi)) Triunghi = null;
            if (string.IsNullOrWhiteSpace(Dreptunghi)) Dreptunghi = null;
        }
    }

    public class CsvImportResult
    {
        public List<Medication> NewMedications { get; set; } = new();
        public List<MedicationUpdate> UpdatedMedications { get; set; } = new();
        public List<MedicationCodeChange> CodeChanges { get; set; } = new();
        public List<MedicationConflict> ManualMedicationConflicts { get; set; } = new();
        public int ProcessedCount { get; set; }
        public int SkippedCount { get; set; }
        public int AddedCount => NewMedications.Count;
        public int UpdatedCount => UpdatedMedications.Count;
        public int CodeChangesCount => CodeChanges.Count;
        public int ConflictsCount => ManualMedicationConflicts.Count;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public bool HasErrors => Errors.Any();
        public bool HasWarnings => Warnings.Any();
        public bool HasConflicts => ManualMedicationConflicts.Any();
        public bool RequiresUserAction => HasConflicts || CodeChanges.Any(c => c.RequiresUserConfirmation);
    }

    public class MedicationUpdate
    {
        public Medication ExistingMedication { get; set; } = null!;
        public Medication NewData { get; set; } = null!;
        public List<string> ChangedFields { get; set; } = new();
        public bool IsApproved { get; set; }
    }

    public class MedicationCodeChange
    {
        public Medication ExistingMedication { get; set; } = null!;
        public string NewCodCIM { get; set; } = string.Empty;
        public string OldCodCIM { get; set; } = string.Empty;
        public bool RequiresUserConfirmation { get; set; }
        public bool IsApproved { get; set; }
        public string Reason { get; set; } = "CodCIM change detected";
    }

    public class MedicationConflict
    {
        public Medication ManualMedication { get; set; } = null!;
        public CsvMedicationRow CsvData { get; set; } = null!;
        public List<string> ConflictingFields { get; set; } = new();
        public bool UserWantsUpdate { get; set; }
        public ConflictResolution Resolution { get; set; } = ConflictResolution.Pending;
    }

    public enum ConflictResolution
    {
        Pending,
        KeepExisting,
        UpdateToImport,
        Skip
    }

    public class CsvImportOptions
    {
        public bool UpdateManualMedications { get; set; } = false;
        public bool ConfirmCodeChanges { get; set; } = true;
        public bool SkipDataActualizareField { get; set; } = true;
        public string ImportDataSource { get; set; } = "CSV_Import";
        public bool AutoApproveUpdates { get; set; } = false;
        public bool CreateBackup { get; set; } = true;
        public int BatchSize { get; set; } = 100;
    }
}
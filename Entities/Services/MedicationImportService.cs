using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Entities.Services
{
    public class MedicationImportService : IMedicationImportService
    {
        private readonly IMedicationRepository _medicationRepository;
        private readonly ICsvFileParser _csvParser;
        private readonly ILogger<MedicationImportService> _logger;

        public MedicationImportService(
            IMedicationRepository medicationRepository,
            ICsvFileParser csvParser,
            ILogger<MedicationImportService> logger)
        {
            _medicationRepository = medicationRepository;
            _csvParser = csvParser;
            _logger = logger;
        }

        public async Task<CsvImportResult> PreviewCsvImportAsync(List<CsvMedicationRow> csvData)
        {
            var result = new CsvImportResult();
            var existingMedications = await _medicationRepository.GetAllNoTrackingAsync();
            var codCimLookup = BuildCodCimLookup(existingMedications);

            foreach (var csvRow in csvData)
            {
                try
                {
                    ProcessCsvRow(csvRow, existingMedications, codCimLookup, result, previewOnly: true);
                    result.ProcessedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing CodCIM {csvRow.CodCIM}: {ex.Message}");
                    result.SkippedCount++;
                }
            }

            return result;
        }

        public async Task<CsvImportResult> ExecuteCsvImportAsync(List<CsvMedicationRow> csvData, CsvImportOptions options)
        {
            var result = new CsvImportResult();
            var existingMedications = await _medicationRepository.GetAllAsync();
            var codCimLookup = BuildCodCimLookup(existingMedications);

            foreach (var csvRow in csvData)
            {
                try
                {
                    ProcessCsvRow(csvRow, existingMedications, codCimLookup, result, previewOnly: false, options);
                    result.ProcessedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing CodCIM {csvRow.CodCIM}: {ex.Message}");
                    result.SkippedCount++;
                }
            }

            if (!result.HasErrors)
            {
                await ExecuteDatabaseOperations(result);

                try
                {
                    // Build a HashSet of new CodCIMs for O(1) lookups
                    var newCodCIMs = new HashSet<string>(
                        csvData
                            .Where(row => !string.IsNullOrWhiteSpace(row.CodCIM))
                            .Select(row => row.CodCIM!),
                        StringComparer.Ordinal);

                    // Reload from DB to get the actual current state after inserts/updates
                    var currentMedications = await _medicationRepository.GetAllAsync();

                    // Detect inactive: existing CSV_Import medications not in the new data
                    var csvImportMedications = currentMedications
                        .Where(m => m.DataSource == "CSV_Import" && !string.IsNullOrWhiteSpace(m.CodCIM))
                        .ToList();

                    var toMarkInactiveIds = csvImportMedications
                        .Where(m => !newCodCIMs.Contains(m.CodCIM!) && m.IsActive)
                        .Select(m => m.Id)
                        .ToList();

                    if (toMarkInactiveIds.Count > 0)
                    {
                        await _medicationRepository.BatchUpdateActiveStatusAsync(toMarkInactiveIds, false);
                        result.Warnings.Add($"Marked {toMarkInactiveIds.Count} medications as inactive (not found in new import)");
                    }

                    // Reactivate: medications that were previously inactive but are present in new data
                    var toReactivateIds = currentMedications
                        .Where(m => !string.IsNullOrWhiteSpace(m.CodCIM) && !m.IsActive && newCodCIMs.Contains(m.CodCIM!))
                        .Select(m => m.Id)
                        .ToList();

                    if (toReactivateIds.Count > 0)
                    {
                        await _medicationRepository.BatchUpdateActiveStatusAsync(toReactivateIds, true);
                        result.Warnings.Add($"Reactivated {toReactivateIds.Count} medications (found again in import)");
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing inactive medications: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<List<CsvMedicationRow>> ParseCsvFileAsync(Stream csvStream)
        {
            return await _csvParser.ParseCsvAsync(csvStream);
        }

        public async Task<List<CsvMedicationRow>> ParseExcelFileAsync(Stream excelStream)
        {
            return await _csvParser.ParseExcelAsync(excelStream);
        }

        public async Task<CsvImportResult> HandleManualMedicationConflictsAsync(List<MedicationConflict> conflicts)
        {
            var result = new CsvImportResult();

            foreach (var conflict in conflicts.Where(c => c.UserWantsUpdate))
            {
                try
                {
                    var updatedMedication = MapCsvToMedication(conflict.CsvData);
                    updatedMedication.Id = conflict.ManualMedication.Id;
                    updatedMedication.CreatedAt = conflict.ManualMedication.CreatedAt;

                    updatedMedication.DataSource = conflict.Resolution == ConflictResolution.UpdateToImport
                        ? "CSV_Import"
                        : "Manual_Updated"; 

                    await _medicationRepository.UpdateAsync(updatedMedication);
                    result.ProcessedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error updating manual medication {conflict.ManualMedication.CodCIM}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<CsvImportResult> HandleCodeChangesAsync(List<MedicationCodeChange> codeChanges)
        {
            var result = new CsvImportResult();

            foreach (var codeChange in codeChanges.Where(c => c.IsApproved))
            {
                try
                {
                    await _medicationRepository.UpdateCodCIMAsync(
                        codeChange.ExistingMedication.Id,
                        codeChange.NewCodCIM,
                        codeChange.OldCodCIM);
                    result.ProcessedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error updating CodCIM from {codeChange.OldCodCIM} to {codeChange.NewCodCIM}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<List<string>> DetectInactiveMedicationsAsync(List<CsvMedicationRow> newImportData)
        {
            var existingCsvMedications = await _medicationRepository.GetByDataSourceAsync("CSV_Import");

            var newCodCIMs = new HashSet<string>(
                newImportData
                    .Where(row => !string.IsNullOrWhiteSpace(row.CodCIM))
                    .Select(row => row.CodCIM!),
                StringComparer.Ordinal);

            var inactiveCodCIMs = existingCsvMedications
                .Where(m => !string.IsNullOrWhiteSpace(m.CodCIM) &&
                           !newCodCIMs.Contains(m.CodCIM!))
                .Select(m => m.CodCIM!)
                .ToList();

            return inactiveCodCIMs;
        }

        public async Task<int> MarkMedicationsAsInactiveAsync(List<string> codCIMsToMarkInactive)
        {
            var codCimSet = new HashSet<string>(codCIMsToMarkInactive, StringComparer.Ordinal);
            var medicationsToUpdate = await _medicationRepository.GetAllAsync();
            var idsToMarkInactive = medicationsToUpdate
                .Where(m => !string.IsNullOrWhiteSpace(m.CodCIM) && codCimSet.Contains(m.CodCIM!) && m.IsActive)
                .Select(m => m.Id)
                .ToList();

            if (idsToMarkInactive.Count > 0)
            {
                await _medicationRepository.BatchUpdateActiveStatusAsync(idsToMarkInactive, false);
            }

            return idsToMarkInactive.Count;
        }

        public async Task<int> MarkMedicationsAsActiveAsync(List<string> codCIMsToMarkActive)
        {
            var codCimSet = new HashSet<string>(codCIMsToMarkActive, StringComparer.Ordinal);
            var medicationsToUpdate = await _medicationRepository.GetAllAsync();
            var idsToReactivate = medicationsToUpdate
                .Where(m => !string.IsNullOrWhiteSpace(m.CodCIM) && codCimSet.Contains(m.CodCIM!) && !m.IsActive)
                .Select(m => m.Id)
                .ToList();

            if (idsToReactivate.Count > 0)
            {
                await _medicationRepository.BatchUpdateActiveStatusAsync(idsToReactivate, true);
            }

            return idsToReactivate.Count;
        }

        /// <summary>
        /// Builds a Dictionary keyed by CodCIM for O(1) lookups instead of O(N) linear scans.
        /// </summary>
        private static Dictionary<string, Medication> BuildCodCimLookup(List<Medication> medications)
        {
            var lookup = new Dictionary<string, Medication>(StringComparer.Ordinal);
            foreach (var m in medications)
            {
                if (!string.IsNullOrWhiteSpace(m.CodCIM) && !lookup.ContainsKey(m.CodCIM))
                {
                    lookup[m.CodCIM] = m;
                }
            }
            return lookup;
        }

        private void ProcessCsvRow(CsvMedicationRow csvRow, List<Medication> existingMedications,
            Dictionary<string, Medication> codCimLookup,
            CsvImportResult result, bool previewOnly, CsvImportOptions options = null)
        {
            Medication existingByCodCIM = null;
            if (!string.IsNullOrWhiteSpace(csvRow.CodCIM))
            {
                codCimLookup.TryGetValue(csvRow.CodCIM, out existingByCodCIM);
            }

            if (existingByCodCIM != null)
            {
                var updatedMedication = MapCsvToMedication(csvRow);
                var changedFields = GetChangedFields(existingByCodCIM, updatedMedication);

                if (changedFields.Any())
                {
                    if (existingByCodCIM.DataSource == "Manual")
                    {
                        result.ManualMedicationConflicts.Add(new MedicationConflict
                        {
                            ManualMedication = existingByCodCIM,
                            CsvData = csvRow,
                            ConflictingFields = changedFields
                        });
                    }
                    else
                    {
                        result.UpdatedMedications.Add(new MedicationUpdate
                        {
                            ExistingMedication = existingByCodCIM,
                            NewData = updatedMedication,
                            ChangedFields = changedFields
                        });
                    }
                }
            }
            else
            {
                var possibleMatch = FindMedicationByAllFieldsExceptCodCIM(csvRow, existingMedications);

                if (possibleMatch != null)
                {
                    result.CodeChanges.Add(new MedicationCodeChange
                    {
                        ExistingMedication = possibleMatch,
                        NewCodCIM = csvRow.CodCIM,
                        OldCodCIM = possibleMatch.CodCIM,
                        RequiresUserConfirmation = possibleMatch.DataSource == "Manual"
                    });
                }
                else
                {
                    var newMedication = MapCsvToMedication(csvRow);
                    newMedication.DataSource = options?.ImportDataSource ?? "CSV_Import";
                    result.NewMedications.Add(newMedication);
                }
            }
        }

        private Medication FindMedicationByAllFieldsExceptCodCIM(CsvMedicationRow csvRow, List<Medication> existingMedications)
        {
            return existingMedications.FirstOrDefault(m =>
                m.CodCIM != csvRow.CodCIM && 
                NullSafeEquals(m.Denumire, csvRow.DenumireComericala) &&
                NullSafeEquals(m.DCI, csvRow.DCI) &&
                NullSafeEquals(m.FormaFarmaceutica, csvRow.FormaFarmaceutica) &&
                NullSafeEquals(m.Concentratia, csvRow.Concentratie) &&
                NullSafeEquals(m.FirmaProducatoare, csvRow.FirmaProducatoare) &&
                NullSafeEquals(m.FirmaDetinatoare, csvRow.FirmaDetinatoare) &&
                NullSafeEquals(m.CodATC, csvRow.CodATC) &&
                NullSafeEquals(m.ActiuneTerapeutica, csvRow.ActiuneTerapeutica) &&
                NullSafeEquals(m.Prescriptie, csvRow.Prescriptie) &&
                NullSafeEquals(m.NrData, csvRow.NrDataAmbalaj) &&
                NullSafeEquals(m.Ambalaj, csvRow.Ambalaj) &&
                NullSafeEquals(m.VolumAmbalaj, csvRow.VolumAmbalaj) &&
                NullSafeEquals(m.Valabilitate, csvRow.ValabilitateAmbalaj) &&
                NullSafeEquals(m.Bulina, csvRow.Bulina) &&
                NullSafeEquals(m.Diez, csvRow.Diez) &&
                NullSafeEquals(m.Stea, csvRow.Stea) &&
                NullSafeEquals(m.Triunghi, csvRow.Triunghi) &&
                NullSafeEquals(m.Dreptunghi, csvRow.Dreptunghi)
            );
        }

        private List<string> GetChangedFields(Medication existing, Medication updated)
        {
            var changedFields = new List<string>();

            if (!NullSafeEquals(existing.Denumire, updated.Denumire)) changedFields.Add("Denumire");
            if (!NullSafeEquals(existing.DCI, updated.DCI)) changedFields.Add("DCI");
            if (!NullSafeEquals(existing.FormaFarmaceutica, updated.FormaFarmaceutica)) changedFields.Add("FormaFarmaceutica");
            if (!NullSafeEquals(existing.Concentratia, updated.Concentratia)) changedFields.Add("Concentratia");
            if (!NullSafeEquals(existing.FirmaProducatoare, updated.FirmaProducatoare)) changedFields.Add("FirmaProducatoare");
            if (!NullSafeEquals(existing.FirmaDetinatoare, updated.FirmaDetinatoare)) changedFields.Add("FirmaDetinatoare");
            if (!NullSafeEquals(existing.CodATC, updated.CodATC)) changedFields.Add("CodATC");
            if (!NullSafeEquals(existing.ActiuneTerapeutica, updated.ActiuneTerapeutica)) changedFields.Add("ActiuneTerapeutica");
            if (!NullSafeEquals(existing.Prescriptie, updated.Prescriptie)) changedFields.Add("Prescriptie");
            if (!NullSafeEquals(existing.NrData, updated.NrData)) changedFields.Add("NrData");
            if (!NullSafeEquals(existing.Ambalaj, updated.Ambalaj)) changedFields.Add("Ambalaj");
            if (!NullSafeEquals(existing.VolumAmbalaj, updated.VolumAmbalaj)) changedFields.Add("VolumAmbalaj");
            if (!NullSafeEquals(existing.Valabilitate, updated.Valabilitate)) changedFields.Add("Valabilitate");
            if (!NullSafeEquals(existing.Bulina, updated.Bulina)) changedFields.Add("Bulina");
            if (!NullSafeEquals(existing.Diez, updated.Diez)) changedFields.Add("Diez");
            if (!NullSafeEquals(existing.Stea, updated.Stea)) changedFields.Add("Stea");
            if (!NullSafeEquals(existing.Triunghi, updated.Triunghi)) changedFields.Add("Triunghi");
            if (!NullSafeEquals(existing.Dreptunghi, updated.Dreptunghi)) changedFields.Add("Dreptunghi");

            return changedFields;
        }

        /// <summary>
        /// Treats null and empty/whitespace strings as equal to avoid false positives
        /// when comparing DB values (which may be null) against parsed CSV values.
        /// </summary>
        private static bool NullSafeEquals(string? a, string? b)
        {
            var normA = string.IsNullOrWhiteSpace(a) ? null : a;
            var normB = string.IsNullOrWhiteSpace(b) ? null : b;
            return string.Equals(normA, normB, StringComparison.Ordinal);
        }

        private Medication MapCsvToMedication(CsvMedicationRow csvRow)
        {
            return new Medication
            {
                CodCIM = NullIfEmpty(csvRow.CodCIM),
                Denumire = NullIfEmpty(csvRow.DenumireComericala),
                DCI = NullIfEmpty(csvRow.DCI),
                FormaFarmaceutica = NullIfEmpty(csvRow.FormaFarmaceutica),
                Concentratia = NullIfEmpty(csvRow.Concentratie),
                FirmaProducatoare = NullIfEmpty(csvRow.FirmaProducatoare),
                FirmaDetinatoare = NullIfEmpty(csvRow.FirmaDetinatoare),
                CodATC = NullIfEmpty(csvRow.CodATC),
                ActiuneTerapeutica = NullIfEmpty(csvRow.ActiuneTerapeutica),
                Prescriptie = NullIfEmpty(csvRow.Prescriptie),
                NrData = NullIfEmpty(csvRow.NrDataAmbalaj),
                Ambalaj = NullIfEmpty(csvRow.Ambalaj),
                VolumAmbalaj = NullIfEmpty(csvRow.VolumAmbalaj),
                Valabilitate = NullIfEmpty(csvRow.ValabilitateAmbalaj),
                Bulina = NullIfEmpty(csvRow.Bulina),
                Diez = NullIfEmpty(csvRow.Diez),
                Stea = NullIfEmpty(csvRow.Stea),
                Triunghi = NullIfEmpty(csvRow.Triunghi),
                Dreptunghi = NullIfEmpty(csvRow.Dreptunghi),
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        private static string? NullIfEmpty(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private async Task ExecuteDatabaseOperations(CsvImportResult result)
        {
            if (result.NewMedications.Any())
            {
                await _medicationRepository.BatchAddAsync(result.NewMedications);
                _logger.LogInformation($"Added {result.NewMedications.Count} new medications");
            }

            if (result.UpdatedMedications.Any())
            {
                var medicationsToUpdate = new List<Medication>();
                foreach (var update in result.UpdatedMedications)
                {
                    ApplyChangesToMedication(update.ExistingMedication, update.NewData);
                    medicationsToUpdate.Add(update.ExistingMedication);
                }
                await _medicationRepository.BatchUpdateAsync(medicationsToUpdate);
            }

            foreach (var codeChange in result.CodeChanges.Where(c => !c.RequiresUserConfirmation))
            {
                await _medicationRepository.UpdateCodCIMAsync(
                    codeChange.ExistingMedication.Id,
                    codeChange.NewCodCIM,
                    codeChange.OldCodCIM);
            }

            _logger.LogInformation($"Import completed: {result.ProcessedCount} processed, {result.SkippedCount} skipped");
        }

        private void ApplyChangesToMedication(Medication existing, Medication updated)
        {
            existing.Denumire = updated.Denumire;
            existing.DCI = updated.DCI;
            existing.FormaFarmaceutica = updated.FormaFarmaceutica;
            existing.Concentratia = updated.Concentratia;
            existing.FirmaProducatoare = updated.FirmaProducatoare;
            existing.FirmaDetinatoare = updated.FirmaDetinatoare;
            existing.CodATC = updated.CodATC;
            existing.ActiuneTerapeutica = updated.ActiuneTerapeutica;
            existing.Prescriptie = updated.Prescriptie;
            existing.NrData = updated.NrData;
            existing.Ambalaj = updated.Ambalaj;
            existing.VolumAmbalaj = updated.VolumAmbalaj;
            existing.Valabilitate = updated.Valabilitate;
            existing.Bulina = updated.Bulina;
            existing.Diez = updated.Diez;
            existing.Stea = updated.Stea;
            existing.Triunghi = updated.Triunghi;
            existing.Dreptunghi = updated.Dreptunghi;
            existing.UpdatedAt = DateTime.Now;
        }

        public async Task<List<CsvMedicationRow>> ParseCustomNomenclatorCsvFileAsync(Stream csvStream)
        {
            return await _csvParser.ParseCustomNomenclatorCsvAsync(csvStream);
        }

        public async Task<List<CsvMedicationRow>> ParseCustomNomenclatorExcelFileAsync(Stream excelStream)
        {
            return await _csvParser.ParseCustomNomenclatorExcelAsync(excelStream);
        }

        public async Task<CsvImportResult> PreviewCustomNomenclatorImportAsync(List<CsvMedicationRow> csvData)
        {
            var result = new CsvImportResult();
            var existingMedications = await _medicationRepository.GetAllNoTrackingAsync();

            foreach (var csvRow in csvData)
            {
                try
                {
                    ProcessCustomNomenclatorCsvRow(csvRow, existingMedications, result, previewOnly: true);
                    result.ProcessedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing {csvRow.DenumireComericala}: {ex.Message}");
                    result.SkippedCount++;
                }
            }

            return result;
        }

        public async Task<CsvImportResult> ExecuteCustomNomenclatorImportAsync(List<CsvMedicationRow> csvData, CsvImportOptions options)
        {
            var result = new CsvImportResult();
            var existingMedications = await _medicationRepository.GetAllAsync();

            foreach (var csvRow in csvData)
            {
                try
                {
                    ProcessCustomNomenclatorCsvRow(csvRow, existingMedications, result, previewOnly: false, options);
                    result.ProcessedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing {csvRow.DenumireComericala}: {ex.Message}");
                    result.SkippedCount++;
                }
            }

            if (!result.HasErrors)
            {
                await ExecuteDatabaseOperations(result);
            }

            return result;
        }

        private void ProcessCustomNomenclatorCsvRow(CsvMedicationRow csvRow, List<Medication> existingMedications,
            CsvImportResult result, bool previewOnly, CsvImportOptions? options = null)
        {
            // First, check for any existing custom nomenclator with this exact name
            var existingByCustomName = existingMedications.FirstOrDefault(m =>
                m.CustomNomenclatorName == csvRow.DenumireComericala &&
                m.DataSource == "Custom_Nomenclator");

            if (existingByCustomName != null)
            {
                // Entry with this custom name already exists
                if (!string.IsNullOrWhiteSpace(csvRow.CodCIM) && existingByCustomName.CodCIM != csvRow.CodCIM)
                {
                    // Same name but different CodCIM - potential conflict
                    result.Warnings.Add($"Custom name '{csvRow.DenumireComericala}' already exists with different CodCIM (existing: '{existingByCustomName.CodCIM}', new: '{csvRow.CodCIM}'). Skipping.");
                    return;
                }
                
                // Check if anything changed and update if necessary
                var hasChanges = HasCustomNomenclatorChanges(existingByCustomName, csvRow);
                if (hasChanges)
                {
                    var updatedMedication = CreateCustomNomenclatorMedication(csvRow, existingByCustomName.LinkedOfficialMedication);
                    updatedMedication.Id = existingByCustomName.Id;
                    updatedMedication.CreatedAt = existingByCustomName.CreatedAt;
                    
                    var changedFields = GetChangedFields(existingByCustomName, updatedMedication);
                    result.UpdatedMedications.Add(new MedicationUpdate
                    {
                        ExistingMedication = existingByCustomName,
                        NewData = updatedMedication,
                        ChangedFields = changedFields
                    });
                }
                // If no changes, skip (avoid duplicate)
                return;
            }

            // No existing entry with this custom name - proceed with creation
            if (!string.IsNullOrWhiteSpace(csvRow.CodCIM))
            {
                // Find official medication for linking
                var officialMedication = existingMedications.FirstOrDefault(m => 
                    m.CodCIM == csvRow.CodCIM && m.DataSource == "CSV_Import");
                
                var newMedication = CreateCustomNomenclatorMedication(csvRow, officialMedication);
                result.NewMedications.Add(newMedication);
            }
            else
            {
                // No CodCIM - create as supplement
                var newMedication = CreateCustomNomenclatorMedication(csvRow, null);
                result.NewMedications.Add(newMedication);
            }
        }

        private Medication CreateCustomNomenclatorMedication(CsvMedicationRow csvRow, Medication? officialMedication)
        {
            var medication = new Medication
            {
                // If linked to official, copy its data; otherwise use CSV data
                CodCIM = csvRow.CodCIM, // This is the CodW
                Denumire = officialMedication?.Denumire ?? csvRow.DenumireComericala,
                DCI = officialMedication?.DCI ?? csvRow.DCI,
                FormaFarmaceutica = officialMedication?.FormaFarmaceutica,
                Concentratia = officialMedication?.Concentratia,
                FirmaProducatoare = officialMedication?.FirmaProducatoare ?? csvRow.FirmaProducatoare,
                FirmaDetinatoare = officialMedication?.FirmaDetinatoare,
                CodATC = officialMedication?.CodATC ?? csvRow.CodATC,
                ActiuneTerapeutica = officialMedication?.ActiuneTerapeutica ?? csvRow.ActiuneTerapeutica,
                Prescriptie = officialMedication?.Prescriptie,
                NrData = officialMedication?.NrData,
                Ambalaj = officialMedication?.Ambalaj,
                VolumAmbalaj = officialMedication?.VolumAmbalaj,
                Valabilitate = officialMedication?.Valabilitate,
                Bulina = officialMedication?.Bulina,
                Diez = officialMedication?.Diez,
                Stea = officialMedication?.Stea,
                Triunghi = officialMedication?.Triunghi,
                Dreptunghi = officialMedication?.Dreptunghi,
                
                // Custom nomenclator specific data
                CustomNomenclatorName = csvRow.DenumireComericala, // Store the custom name
                LinkedOfficialMedicationId = officialMedication?.Id, // Link if exists
                
                DataSource = "Custom_Nomenclator",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = true
            };

            return medication;
        }

        // Helper method to check for changes
        private bool HasCustomNomenclatorChanges(Medication existing, CsvMedicationRow csvRow)
        {
            return existing.CustomNomenclatorName != csvRow.DenumireComericala ||
                   existing.DCI != csvRow.DCI ||
                   existing.CodATC != csvRow.CodATC ||
                   existing.ActiuneTerapeutica != csvRow.ActiuneTerapeutica ||
                   existing.FirmaProducatoare != csvRow.FirmaProducatoare;
        }
    }
}
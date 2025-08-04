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
            var existingMedications = await _medicationRepository.GetAllAsync();

            foreach (var csvRow in csvData)
            {
                try
                {
                    await ProcessCsvRow(csvRow, existingMedications, result, previewOnly: true);
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

            foreach (var csvRow in csvData)
            {
                try
                {
                    await ProcessCsvRow(csvRow, existingMedications, result, previewOnly: false, options);
                    result.ProcessedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing CodCIM {csvRow.CodCIM}: {ex.Message}");
                    result.SkippedCount++;
                }
            }

            // Execute the actual database operations
            if (!result.HasErrors)
            {
                await ExecuteDatabaseOperations(result);

                // NEW: Handle inactive medications
                try
                {
                    // Detect which medications should become inactive
                    var inactiveCodCIMs = await DetectInactiveMedicationsAsync(csvData);

                    // Mark them as inactive
                    if (inactiveCodCIMs.Any())
                    {
                        var inactiveCount = await MarkMedicationsAsInactiveAsync(inactiveCodCIMs);
                        result.Warnings.Add($"Marked {inactiveCount} medications as inactive (not found in new import)");
                    }

                    // Mark medications as active if they're back in the import
                    var activeCodCIMs = csvData
                        .Where(row => !string.IsNullOrWhiteSpace(row.CodCIM))
                        .Select(row => row.CodCIM!)
                        .Distinct()
                        .ToList();

                    var reactivatedCount = await MarkMedicationsAsActiveAsync(activeCodCIMs);
                    if (reactivatedCount > 0)
                    {
                        result.Warnings.Add($"Reactivated {reactivatedCount} medications (found again in import)");
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

                    // FIXED: Change DataSource to indicate it's now synchronized with import data
                    updatedMedication.DataSource = conflict.Resolution == ConflictResolution.UpdateToImport
                        ? "CSV_Import"
                        : "Manual_Updated"; // Or use a hybrid designation

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
            // Get all existing medications from CSV imports
            var existingCsvMedications = await _medicationRepository.GetByDataSourceAsync("CSV_Import");

            // Get CodCIMs from the new import
            var newCodCIMs = newImportData
                .Where(row => !string.IsNullOrWhiteSpace(row.CodCIM))
                .Select(row => row.CodCIM!)
                .Distinct()
                .ToList();

            // Find medications that exist in database but NOT in new import
            var inactiveCodCIMs = existingCsvMedications
                .Where(m => !string.IsNullOrWhiteSpace(m.CodCIM) &&
                           !newCodCIMs.Contains(m.CodCIM))
                .Select(m => m.CodCIM!)
                .ToList();

            return inactiveCodCIMs;
        }

        public async Task<int> MarkMedicationsAsInactiveAsync(List<string> codCIMsToMarkInactive)
        {
            var medicationsToUpdate = await _medicationRepository.GetAllAsync();
            var medicationsToMarkInactive = medicationsToUpdate
                .Where(m => codCIMsToMarkInactive.Contains(m.CodCIM) && m.IsActive)
                .ToList();

            foreach (var medication in medicationsToMarkInactive)
            {
                medication.IsActive = false;
                medication.UpdatedAt = DateTime.Now;
                await _medicationRepository.UpdateAsync(medication);
            }

            return medicationsToMarkInactive.Count;
        }

        public async Task<int> MarkMedicationsAsActiveAsync(List<string> codCIMsToMarkActive)
        {
            var medicationsToUpdate = await _medicationRepository.GetAllAsync();
            var medicationsToMarkActive = medicationsToUpdate
                .Where(m => codCIMsToMarkActive.Contains(m.CodCIM) && !m.IsActive)
                .ToList();

            foreach (var medication in medicationsToMarkActive)
            {
                medication.IsActive = true;
                medication.UpdatedAt = DateTime.Now;
                await _medicationRepository.UpdateAsync(medication);
            }

            return medicationsToMarkActive.Count;
        }

        private async Task ProcessCsvRow(CsvMedicationRow csvRow, List<Medication> existingMedications,
            CsvImportResult result, bool previewOnly, CsvImportOptions options = null)
        {
            // Check if CodCIM already exists
            var existingByCodCIM = existingMedications.FirstOrDefault(m => m.CodCIM == csvRow.CodCIM);

            if (existingByCodCIM != null)
            {
                // CodCIM exists - check if data has changed
                var updatedMedication = MapCsvToMedication(csvRow);
                var changedFields = GetChangedFields(existingByCodCIM, updatedMedication);

                if (changedFields.Any())
                {
                    if (existingByCodCIM.DataSource == "Manual")
                    {
                        // Manual medication - needs user confirmation
                        result.ManualMedicationConflicts.Add(new MedicationConflict
                        {
                            ManualMedication = existingByCodCIM,
                            CsvData = csvRow,
                            ConflictingFields = changedFields
                        });
                    }
                    else
                    {
                        // Import medication - update directly
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
                // CodCIM doesn't exist - check if it's a new code for existing medication
                var possibleMatch = FindMedicationByAllFieldsExceptCodCIM(csvRow, existingMedications);

                if (possibleMatch != null)
                {
                    // Found matching medication with different CodCIM
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
                    // Completely new medication
                    var newMedication = MapCsvToMedication(csvRow);
                    newMedication.DataSource = options?.ImportDataSource ?? "CSV_Import";
                    result.NewMedications.Add(newMedication);
                }
            }
        }

        private Medication FindMedicationByAllFieldsExceptCodCIM(CsvMedicationRow csvRow, List<Medication> existingMedications)
        {
            return existingMedications.FirstOrDefault(m =>
                m.CodCIM != csvRow.CodCIM && // Different CodCIM
                m.Denumire == csvRow.DenumireComericala &&
                m.DCI == csvRow.DCI &&
                m.FormaFarmaceutica == csvRow.FormaFarmaceutica &&
                m.Concentratia == csvRow.Concentratie &&
                m.FirmaProducatoare == csvRow.FirmaProducatoare &&
                m.FirmaDetinatoare == csvRow.FirmaDetinatoare &&
                m.CodATC == csvRow.CodATC &&
                m.ActiuneTerapeutica == csvRow.ActiuneTerapeutica &&
                m.Prescriptie == csvRow.Prescriptie &&
                m.NrData == csvRow.NrDataAmbalaj &&
                m.Ambalaj == csvRow.Ambalaj &&
                m.VolumAmbalaj == csvRow.VolumAmbalaj &&
                m.Valabilitate == csvRow.ValabilitateAmbalaj &&
                m.Bulina == csvRow.Bulina &&
                m.Diez == csvRow.Diez &&
                m.Stea == csvRow.Stea &&
                m.Triunghi == csvRow.Triunghi &&
                m.Dreptunghi == csvRow.Dreptunghi
            );
        }

        private List<string> GetChangedFields(Medication existing, Medication updated)
        {
            var changedFields = new List<string>();

            if (existing.Denumire != updated.Denumire) changedFields.Add("Denumire");
            if (existing.DCI != updated.DCI) changedFields.Add("DCI");
            if (existing.FormaFarmaceutica != updated.FormaFarmaceutica) changedFields.Add("FormaFarmaceutica");
            if (existing.Concentratia != updated.Concentratia) changedFields.Add("Concentratia");
            if (existing.FirmaProducatoare != updated.FirmaProducatoare) changedFields.Add("FirmaProducatoare");
            if (existing.FirmaDetinatoare != updated.FirmaDetinatoare) changedFields.Add("FirmaDetinatoare");
            if (existing.CodATC != updated.CodATC) changedFields.Add("CodATC");
            if (existing.ActiuneTerapeutica != updated.ActiuneTerapeutica) changedFields.Add("ActiuneTerapeutica");
            if (existing.Prescriptie != updated.Prescriptie) changedFields.Add("Prescriptie");
            if (existing.NrData != updated.NrData) changedFields.Add("NrData");
            if (existing.Ambalaj != updated.Ambalaj) changedFields.Add("Ambalaj");
            if (existing.VolumAmbalaj != updated.VolumAmbalaj) changedFields.Add("VolumAmbalaj");
            if (existing.Valabilitate != updated.Valabilitate) changedFields.Add("Valabilitate");
            if (existing.Bulina != updated.Bulina) changedFields.Add("Bulina");
            if (existing.Diez != updated.Diez) changedFields.Add("Diez");
            if (existing.Stea != updated.Stea) changedFields.Add("Stea");
            if (existing.Triunghi != updated.Triunghi) changedFields.Add("Triunghi");
            if (existing.Dreptunghi != updated.Dreptunghi) changedFields.Add("Dreptunghi");

            return changedFields;
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
                Dreptunghi = csvRow.Dreptunghi,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        private async Task ExecuteDatabaseOperations(CsvImportResult result)
        {
            // Add new medications
            if (result.NewMedications.Any())
            {
                await _medicationRepository.BatchAddAsync(result.NewMedications);
                _logger.LogInformation($"Added {result.NewMedications.Count} new medications");
            }

            // Update existing medications
            foreach (var update in result.UpdatedMedications)
            {
                ApplyChangesToMedication(update.ExistingMedication, update.NewData);
                await _medicationRepository.UpdateAsync(update.ExistingMedication);
            }

            // Handle code changes
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
    }
}
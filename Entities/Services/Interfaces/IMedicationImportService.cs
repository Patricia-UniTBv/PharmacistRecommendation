using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface IMedicationImportService
    {
        Task<CsvImportResult> PreviewCsvImportAsync(List<CsvMedicationRow> csvData);
        Task<CsvImportResult> ExecuteCsvImportAsync(List<CsvMedicationRow> csvData, CsvImportOptions options);
        Task<List<CsvMedicationRow>> ParseCsvFileAsync(Stream csvStream);
        Task<List<CsvMedicationRow>> ParseExcelFileAsync(Stream excelStream);
        Task<CsvImportResult> HandleManualMedicationConflictsAsync(List<MedicationConflict> conflicts);
        Task<CsvImportResult> HandleCodeChangesAsync(List<MedicationCodeChange> codeChanges);
    }
}
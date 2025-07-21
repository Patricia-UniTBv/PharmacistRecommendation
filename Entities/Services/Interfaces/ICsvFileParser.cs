using Entities.Models;

namespace Entities.Services.Interfaces
{
    public interface ICsvFileParser
    {
        Task<List<CsvMedicationRow>> ParseCsvAsync(Stream csvStream);
        Task<List<CsvMedicationRow>> ParseExcelAsync(Stream excelStream);
        bool ValidateCsvStructure(Stream csvStream);
        List<string> GetRequiredColumns();
    }
}
using Entities.Models;
using Entities.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Entities.Services
{
    public class CsvFileParser : ICsvFileParser
    {
        private readonly ILogger<CsvFileParser> _logger;

        public CsvFileParser(ILogger<CsvFileParser> logger)
        {
            _logger = logger;
        }

        public async Task<List<CsvMedicationRow>> ParseCsvAsync(Stream csvStream)
        {
            var medications = new List<CsvMedicationRow>();

            try
            {
                csvStream.Position = 0;
                using var reader = new StreamReader(csvStream, Encoding.UTF8);

                var headerLine = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(headerLine))
                {
                    throw new InvalidOperationException("CSV file is empty or invalid");
                }

                var headers = headerLine.Split(',').Select(h => h.Trim('"').Trim()).ToArray();
                ValidateHeaders(headers);

                string line;
                int rowNumber = 1;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    rowNumber++;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var medication = ParseCsvLine(line, headers);
                        if (medication != null)
                        {
                            medications.Add(medication);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error parsing CSV line {rowNumber}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CSV file");
                throw;
            }

            return medications;
        }

        public async Task<List<CsvMedicationRow>> ParseExcelAsync(Stream excelStream)
        {
            await Task.CompletedTask;
            throw new NotSupportedException(
                "Excel file import is temporarily unavailable due to library conflicts. " +
                "Please convert your Excel file to CSV format:\n\n" +
                "1. Open your Excel file\n" +
                "2. Go to File > Save As\n" +
                "3. Choose 'CSV (Comma delimited)' format\n" +
                "4. Save and try importing the CSV file instead\n\n" +
                "We apologize for the inconvenience and are working to resolve this issue."
            );
        }

        public bool ValidateCsvStructure(Stream csvStream)
        {
            try
            {
                csvStream.Position = 0;
                using var reader = new StreamReader(csvStream, Encoding.UTF8);
                var headerLine = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(headerLine))
                    return false;

                var headers = headerLine.Split(',').Select(h => h.Trim('"').Trim()).ToArray();
                ValidateHeaders(headers);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetRequiredColumns()
        {
            return new List<string>
            {
                "Cod CIM",
                "Denumire comerciala",
                "DCI",
                "Forma farmaceutica",
                "Concentratie",
                "Firma / tara producatoare APP",
                "Firma / tara detinatoare APP",
                "Cod ATC",
                "Actiune terapeutica",
                "Prescriptie",
                "Nr / data ambalaj APP",
                "Ambalaj",
                "Volum ambalaj",
                "Valabilitate ambalaj",
                "Bulina",
                "Diez",
                "Stea",
                "Triunghi",
                "Dreptunghi",
                "Data actualizare"
            };
        }

        private void ValidateHeaders(string[] headers)
        {
            var requiredColumns = GetRequiredColumns();
            var missingColumns = requiredColumns.Where(req => !headers.Contains(req, StringComparer.OrdinalIgnoreCase)).ToList();

            if (missingColumns.Any())
            {
                throw new InvalidOperationException($"Missing required columns: {string.Join(", ", missingColumns)}");
            }
        }

        private CsvMedicationRow ParseCsvLine(string line, string[] headers)
        {
            var values = ParseCsvValues(line);

            if (values.Length != headers.Length)
            {
                throw new InvalidOperationException($"Column count mismatch. Expected {headers.Length}, got {values.Length}");
            }

            return CreateMedicationRow(values, headers);
        }

        private CsvMedicationRow CreateMedicationRow(string[] values, string[] headers)
        {
            var medication = new CsvMedicationRow();

            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                var value = i < values.Length ? values[i] : "";

                if (header.Equals("Bulina", StringComparison.OrdinalIgnoreCase) ||
                    header.Equals("Diez", StringComparison.OrdinalIgnoreCase) ||
                    header.Equals("Stea", StringComparison.OrdinalIgnoreCase) ||
                    header.Equals("Triunghi", StringComparison.OrdinalIgnoreCase) ||
                    header.Equals("Dreptunghi", StringComparison.OrdinalIgnoreCase))
                {
                    value = value.ToUpper() == "X" ? "X" : null;
                }

                if (header.Equals("Cod CIM", StringComparison.OrdinalIgnoreCase))
                    medication.CodCIM = value;
                else if (header.Equals("Denumire comerciala", StringComparison.OrdinalIgnoreCase))
                    medication.DenumireComericala = value;
                else if (header.Equals("DCI", StringComparison.OrdinalIgnoreCase))
                    medication.DCI = value;
                else if (header.Equals("Forma farmaceutica", StringComparison.OrdinalIgnoreCase))
                    medication.FormaFarmaceutica = value;
                else if (header.Equals("Concentratie", StringComparison.OrdinalIgnoreCase))
                    medication.Concentratie = value;
                else if (header.Equals("Firma / tara producatoare APP", StringComparison.OrdinalIgnoreCase))
                    medication.FirmaProducatoare = value;
                else if (header.Equals("Firma / tara detinatoare APP", StringComparison.OrdinalIgnoreCase))
                    medication.FirmaDetinatoare = value;
                else if (header.Equals("Cod ATC", StringComparison.OrdinalIgnoreCase))
                    medication.CodATC = value;
                else if (header.Equals("Actiune terapeutica", StringComparison.OrdinalIgnoreCase))
                    medication.ActiuneTerapeutica = value;
                else if (header.Equals("Prescriptie", StringComparison.OrdinalIgnoreCase))
                    medication.Prescriptie = value;
                else if (header.Equals("Nr / data ambalaj APP", StringComparison.OrdinalIgnoreCase))
                    medication.NrDataAmbalaj = value;
                else if (header.Equals("Ambalaj", StringComparison.OrdinalIgnoreCase))
                    medication.Ambalaj = value;
                else if (header.Equals("Volum ambalaj", StringComparison.OrdinalIgnoreCase))
                    medication.VolumAmbalaj = value;
                else if (header.Equals("Valabilitate ambalaj", StringComparison.OrdinalIgnoreCase))
                    medication.ValabilitateAmbalaj = value;
                else if (header.Equals("Bulina", StringComparison.OrdinalIgnoreCase))
                    medication.Bulina = value;
                else if (header.Equals("Diez", StringComparison.OrdinalIgnoreCase))
                    medication.Diez = value;
                else if (header.Equals("Stea", StringComparison.OrdinalIgnoreCase))
                    medication.Stea = value;
                else if (header.Equals("Triunghi", StringComparison.OrdinalIgnoreCase))
                    medication.Triunghi = value;
                else if (header.Equals("Dreptunghi", StringComparison.OrdinalIgnoreCase))
                    medication.Dreptunghi = value;
                else if (header.Equals("Data actualizare", StringComparison.OrdinalIgnoreCase))
                    medication.DataActualizare = value;
            }

            if (string.IsNullOrWhiteSpace(medication.CodCIM))
            {
                throw new InvalidOperationException("CodCIM is required");
            }

            medication.NormalizeEmptyStrings();
            return medication;
        }

        private string[] ParseCsvValues(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString().Trim());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            values.Add(currentValue.ToString().Trim());
            return values.ToArray();
        }

        public async Task<List<CsvMedicationRow>> ParseCustomNomenclatorCsvAsync(Stream csvStream)
        {
            var medications = new List<CsvMedicationRow>();
            csvStream.Position = 0;

            using var reader = new StreamReader(csvStream, Encoding.UTF8);
            var headerLine = await reader.ReadLineAsync();
            
            if (string.IsNullOrWhiteSpace(headerLine))
                throw new InvalidOperationException("CSV file is empty or has no header");

            var headers = headerLine.Split(',').Select(h => h.Trim('"').Trim()).ToArray();
            ValidateCustomNomenclatorHeaders(headers);

            string? line;
            int lineNumber = 1;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var medication = ParseCustomNomenclatorCsvLine(line, headers);
                    medication.NormalizeEmptyStrings();
                    medications.Add(medication);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error parsing line {lineNumber}: {ex.Message}");
                }
            }

            return medications;
        }

        public bool ValidateCustomNomenclatorCsvStructure(Stream csvStream)
        {
            try
            {
                csvStream.Position = 0;
                using var reader = new StreamReader(csvStream, Encoding.UTF8);
                var headerLine = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(headerLine))
                    return false;

                var headers = headerLine.Split(',').Select(h => h.Trim('"').Trim()).ToArray();
                ValidateCustomNomenclatorHeaders(headers);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetCustomNomenclatorRequiredColumns()
        {
            return new List<string>
            {
                "Denumire",
                "Producator", 
                "Cod W", // Optional - might be empty for supplements
                "Cod ATC",
                "Tip ANM",
                "DCI"
            };
        }

        private void ValidateCustomNomenclatorHeaders(string[] headers)
        {
            var requiredColumns = new[] { "Denumire", "Producator" }; // minimum required
            var missingColumns = requiredColumns.Where(req => !headers.Contains(req, StringComparer.OrdinalIgnoreCase)).ToList();

            if (missingColumns.Any())
            {
                throw new InvalidOperationException($"Missing required columns: {string.Join(", ", missingColumns)}");
            }
        }

        private CsvMedicationRow ParseCustomNomenclatorCsvLine(string line, string[] headers)
        {
            var values = ParseCsvValues(line);

            if (values.Length != headers.Length)
            {
                throw new InvalidOperationException($"Column count mismatch. Expected {headers.Length}, got {values.Length}");
            }

            return CreateCustomNomenclatorMedicationRow(values, headers);
        }

        private CsvMedicationRow CreateCustomNomenclatorMedicationRow(string[] values, string[] headers)
        {
            var medication = new CsvMedicationRow();

            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                var value = i < values.Length ? values[i] : "";

                switch (header.ToLower())
                {
                    case "denumire":
                        medication.DenumireComericala = value;
                        break;
                    case "producator":
                        medication.FirmaProducatoare = value;
                        break;
                    case "cod w":
                        medication.CodCIM = string.IsNullOrWhiteSpace(value) ? null : value; // CodW = CodCIM, can be null
                        break;
                    case "cod atc":
                        medication.CodATC = value;
                        break;
                    case "tip anm":
                        medication.ActiuneTerapeutica = value; // Store in existing field
                        break;
                    case "dci":
                        medication.DCI = value;
                        break;
                }
            }

            return medication;
        }
    }
}
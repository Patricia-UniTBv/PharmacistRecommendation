using ClosedXML.Excel;
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
                ThrowIfBinaryContent(csvStream);

                using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

                var headerLine = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(headerLine))
                {
                    throw new InvalidOperationException("CSV file is empty or invalid");
                }

                var delimiter = DetectDelimiter(headerLine);
                var headers = SplitHeaderLine(headerLine, delimiter);
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
                        var medication = ParseCsvLine(line, headers, delimiter);
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
            return await Task.Run(() =>
            {
                var medications = new List<CsvMedicationRow>();

                using var workbook = new XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.First();
                var firstRow = worksheet.FirstRowUsed();
                if (firstRow == null)
                    throw new InvalidOperationException("Excel file is empty");

                var headers = firstRow.CellsUsed()
                    .Select(c => c.GetString().Trim())
                    .ToArray();
                ValidateHeaders(headers);

                var dataRows = worksheet.RowsUsed().Skip(1);
                int rowNumber = 1;
                foreach (var row in dataRows)
                {
                    rowNumber++;
                    try
                    {
                        var values = new string[headers.Length];
                        for (int i = 0; i < headers.Length; i++)
                            values[i] = row.Cell(i + 1).GetString().Trim();

                        var medication = CreateMedicationRow(values, headers);
                        if (medication != null)
                            medications.Add(medication);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error parsing Excel row {rowNumber}: {ex.Message}");
                    }
                }

                return medications;
            });
        }

        public async Task<List<CsvMedicationRow>> ParseCustomNomenclatorCsvAsync(Stream csvStream)
        {
            var medications = new List<CsvMedicationRow>();
            csvStream.Position = 0;
            ThrowIfBinaryContent(csvStream);

            using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var headerLine = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(headerLine))
                throw new InvalidOperationException("CSV file is empty or has no header");

            var delimiter = DetectDelimiter(headerLine);
            var headers = SplitHeaderLine(headerLine, delimiter);
            ValidateCustomNomenclatorHeaders(headers);

            string? line;
            int lineNumber = 1;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var medication = ParseCustomNomenclatorCsvLine(line, headers, delimiter);
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

        public async Task<List<CsvMedicationRow>> ParseCustomNomenclatorExcelAsync(Stream excelStream)
        {
            return await Task.Run(() =>
            {
                var medications = new List<CsvMedicationRow>();

                using var workbook = new XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.First();
                var firstRow = worksheet.FirstRowUsed();
                if (firstRow == null)
                    throw new InvalidOperationException("Excel file is empty");

                var headers = firstRow.CellsUsed()
                    .Select(c => c.GetString().Trim())
                    .ToArray();
                ValidateCustomNomenclatorHeaders(headers);

                var dataRows = worksheet.RowsUsed().Skip(1);
                int rowNumber = 1;
                foreach (var row in dataRows)
                {
                    rowNumber++;
                    try
                    {
                        var values = new string[headers.Length];
                        for (int i = 0; i < headers.Length; i++)
                            values[i] = row.Cell(i + 1).GetString().Trim();

                        var medication = CreateCustomNomenclatorMedicationRow(values, headers);
                        medication.NormalizeEmptyStrings();
                        medications.Add(medication);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error parsing Excel row {rowNumber}: {ex.Message}");
                    }
                }

                return medications;
            });
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

                var delimiter = DetectDelimiter(headerLine);
                var headers = SplitHeaderLine(headerLine, delimiter);
                ValidateHeaders(headers);

                return true;
            }
            catch
            {
                return false;
            }
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

                var delimiter = DetectDelimiter(headerLine);
                var headers = SplitHeaderLine(headerLine, delimiter);
                ValidateCustomNomenclatorHeaders(headers);

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

        public List<string> GetCustomNomenclatorRequiredColumns()
        {
            return new List<string>
            {
                "Denumire",
                "Producator",
                "Cod W",
                "Cod ATC",
                "Tip ANM",
                "DCI"
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

        private void ValidateCustomNomenclatorHeaders(string[] headers)
        {
            var requiredColumns = new[] { "Denumire", "Producator" };
            var missingColumns = requiredColumns.Where(req => !headers.Contains(req, StringComparer.OrdinalIgnoreCase)).ToList();

            if (missingColumns.Any())
            {
                throw new InvalidOperationException(
                    $"Missing required columns: {string.Join(", ", missingColumns)}. " +
                    $"Found columns: [{string.Join(", ", headers)}]");
            }
        }

        private CsvMedicationRow ParseCsvLine(string line, string[] headers, char delimiter)
        {
            var values = ParseCsvValues(line, delimiter);

            if (values.Length != headers.Length)
            {
                throw new InvalidOperationException($"Column count mismatch. Expected {headers.Length}, got {values.Length}");
            }

            return CreateMedicationRow(values, headers);
        }

        private CsvMedicationRow ParseCustomNomenclatorCsvLine(string line, string[] headers, char delimiter)
        {
            var values = ParseCsvValues(line, delimiter);

            if (values.Length != headers.Length)
            {
                throw new InvalidOperationException($"Column count mismatch. Expected {headers.Length}, got {values.Length}");
            }

            return CreateCustomNomenclatorMedicationRow(values, headers);
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
                        medication.CodCIM = string.IsNullOrWhiteSpace(value) ? null : value;
                        break;
                    case "cod atc":
                        medication.CodATC = value;
                        break;
                    case "tip anm":
                        medication.ActiuneTerapeutica = value;
                        break;
                    case "dci":
                        medication.DCI = value;
                        break;
                }
            }

            return medication;
        }

        private string[] ParseCsvValues(string line, char delimiter)
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
                else if (c == delimiter && !inQuotes)
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

        /// <summary>
        /// Auto-detects the CSV delimiter by checking if the header line contains more 
        /// semicolons than commas (common for European/Romanian CSV exports from Excel).
        /// </summary>
        private static char DetectDelimiter(string headerLine)
        {
            int commaCount = headerLine.Count(c => c == ',');
            int semicolonCount = headerLine.Count(c => c == ';');

            return semicolonCount > commaCount ? ';' : ',';
        }

        /// <summary>
        /// Splits a header line by the given delimiter, trimming quotes and whitespace from each column name.
        /// </summary>
        private static string[] SplitHeaderLine(string headerLine, char delimiter)
        {
            // Use the same quote-aware parsing as data lines to handle quoted headers
            var values = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < headerLine.Length; i++)
            {
                char c = headerLine[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
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

        /// <summary>
        /// Checks the first bytes of the stream for binary signatures (e.g. ZIP/XLSX magic bytes).
        /// Resets stream position after checking. Throws with a user-friendly message if binary content is detected.
        /// </summary>
        private static void ThrowIfBinaryContent(Stream stream)
        {
            if (!stream.CanRead || stream.Length < 4)
                return;

            stream.Position = 0;
            Span<byte> header = stackalloc byte[4];
            int bytesRead = stream.Read(header);
            stream.Position = 0;

            if (bytesRead < 4)
                return;

            // ZIP magic bytes (PK\x03\x04) — .xlsx files are ZIP archives
            if (header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04)
            {
                throw new InvalidOperationException(
                    "Fișierul selectat este un fișier Excel (.xlsx), nu un fișier CSV.\n\n" +
                    "Vă rugăm convertiți fișierul în format CSV:\n" +
                    "1. Deschideți fișierul în Excel\n" +
                    "2. Mergeți la File → Save As\n" +
                    "3. Selectați formatul 'CSV UTF-8 (Comma delimited)' sau 'CSV (Comma delimited)'\n" +
                    "4. Salvați și importați fișierul CSV rezultat");
            }

            // Check for non-text content (control characters in the first bytes, excluding BOM and common whitespace)
            for (int i = 0; i < bytesRead; i++)
            {
                byte b = header[i];
                // Allow: tab (0x09), LF (0x0A), CR (0x0D), printable ASCII (0x20+), UTF-8 continuation (0x80+), BOM bytes (0xEF, 0xBB, 0xBF)
                if (b < 0x09 || (b > 0x0D && b < 0x20))
                {
                    throw new InvalidOperationException(
                        "Fișierul selectat nu pare să fie un fișier text CSV valid.\n" +
                        "Asigurați-vă că fișierul este salvat în format CSV (text), nu Excel (.xlsx).");
                }
            }
        }
    }
}
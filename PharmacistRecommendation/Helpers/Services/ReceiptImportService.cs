using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers.Services
{
    public class ReceiptImportService
    {
        public static string GetLastDatedFolder(string rootPath)
        {
            var directories = Directory.GetDirectories(rootPath);
            if (directories.Length == 0)
                return null;

            var latest = directories
                .Select(dir => new
                {
                    Path = dir,
                    Name = Path.GetFileName(dir),
                    Date = DateTime.TryParseExact(
                        Path.GetFileName(dir),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var dt) ? dt : (DateTime?)null
                })
                .Where(x => x.Date != null)
                .OrderByDescending(x => x.Date)
                .FirstOrDefault();

            return latest?.Path;
        }

        public static string FindTextOrLogFile(string folderPath)
        {
            var txtFile = Directory.GetFiles(folderPath, "*.txt").FirstOrDefault();
            if (txtFile != null)
                return txtFile;

            return Directory.GetFiles(folderPath, "*.log").FirstOrDefault();
        }

        public static ReceiptImportModel ImportLastReceipt(string logFilePath)
        {
            var lines = File.ReadAllLines(logFilePath);
            var receiptLines = new List<string>();
            bool inLastReceipt = false;

            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].Contains("Bon fiscal inchis") && !inLastReceipt)
                {
                    inLastReceipt = true;
                    continue;
                }
                if (inLastReceipt)
                {
                    if (lines[i].Contains("Bon fiscal inchis"))
                        break;
                    if (lines[i].Contains("Vanzare:"))
                        receiptLines.Add(lines[i]);
                }
            }
            receiptLines.Reverse();

            int index = 1;
            var model = new ReceiptImportModel
            {
                Medications = receiptLines
                    .Select(line =>
                    {

                        var idx = line.IndexOf("Vanzare:") + "Vanzare:".Length;
                        var afterVanzare = line.Substring(idx).TrimStart();
                        var endIdx = afterVanzare.IndexOf("->");
                        var name = (endIdx > 0 ? afterVanzare.Substring(0, endIdx) : afterVanzare).Trim();
                        return new ReceiptDrugModel {
                            Index = index++,
                            Name = name };
                    })
                    .GroupBy(drug => drug.Name, StringComparer.InvariantCultureIgnoreCase)
                    .Select(g => g.First())
                    .ToList()
            };

            return model;
        }

    }
}

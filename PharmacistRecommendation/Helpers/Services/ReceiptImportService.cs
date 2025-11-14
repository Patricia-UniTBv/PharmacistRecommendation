using System.Globalization;

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

       

    }
}

using System.Xml;

namespace PharmacistRecommendation.Helpers.Services
{
    public class PrescriptionImportService
    {
        public static string GetLastPrescriptionFile(string folderPath)
        {
            var dir = new DirectoryInfo(folderPath);
            var files = dir.GetFiles()
                .Where(x => !x.Name.Contains("_RSP"))
                .OrderByDescending(x => x.LastWriteTime)
                .ToList();

            foreach (var file in files)
            {
                try
                {
                    var text = File.ReadAllText(file.FullName);
                    if (text.Contains("<prescription"))
                        return file.FullName;
                }
                catch { }
            }
            return null;
        }

        public static PrescriptionImportModel ImportFromXml(string filePath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            var ns = new XmlNamespaceManager(xmlDoc.NameTable);
            ns.AddNamespace("p", "http://www.cnas.ro/pel/1.0");

            var prescriptionNode = xmlDoc.SelectSingleNode("//p:prescription", ns);
            if (prescriptionNode == null)
                throw new Exception("Nodul <prescription> nu există!");

            var model = new PrescriptionImportModel
            {
                PatientCnp = prescriptionNode.Attributes["personCID"]?.InnerText,
                PrescriptionSeries = prescriptionNode.Attributes["series"]?.InnerText,
                PrescriptionNumber = prescriptionNode.Attributes["no"]?.InnerText,
                DoctorStamp = prescriptionNode.Attributes["physicianStencilNo"]?.InnerText,
                Diagnosis = prescriptionNode.Attributes["diagnostic"]?.InnerText
            };

            int index = 1;
            foreach (XmlNode drugNode in prescriptionNode.SelectNodes("p:prescriptionDrug", ns))
            {
                var drug = new PrescriptionDrugModel
                {
                    Index = index++,
                    Name = drugNode.Attributes["activeSubstance"]?.InnerText,
                    Concentration = drugNode.Attributes["concentration"]?.InnerText,
                    PharmaceuticalForm = drugNode.Attributes["pharmaceuticalForm"]?.InnerText,
                    Dose = drugNode.Attributes["dose"]?.InnerText,
                    DiseaseCode = drugNode.Attributes["diseaseCode"]?.InnerText,
                };
                model.Drugs.Add(drug);
            }

            return model;
        }

    }
}

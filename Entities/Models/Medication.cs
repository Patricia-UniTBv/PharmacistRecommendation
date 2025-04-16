using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Medication
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? PharmaceuticalForm { get; set; }

    public string? AtcCode { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<DoctorMedication> DoctorMedications { get; set; } = new List<DoctorMedication>();

    public virtual ICollection<MedicationDocument> MedicationDocuments { get; set; } = new List<MedicationDocument>();

    public virtual ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } = new List<PrescriptionMedication>();
}

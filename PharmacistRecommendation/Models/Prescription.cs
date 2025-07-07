using System;
using System.Collections.Generic;

namespace PharmacistRecommendation.Models;

public partial class Prescription
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? DocumentId { get; set; }

    public string? Series { get; set; }

    public string? Number { get; set; }

    public string? Diagnostic { get; set; }

    public DateTime IssueDate { get; set; }

    public DateTime? FillDate { get; set; }

    public virtual Document? Document { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } = new List<PrescriptionMedication>();
}

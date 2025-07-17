using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Prescription
{
    public int Id { get; set; }

    public int? PatientId { get; set; }

    public int? DocumentId { get; set; }

    public string? Series { get; set; }

    public string? Number { get; set; }

    public string? Diagnostic { get; set; }

    public DateTime IssueDate { get; set; }

    public DateTime? FillDate { get; set; }

    public string? DiagnosisMentionedByPatient { get; set; }

    public string? Symptoms { get; set; }

    public string? Suspicion { get; set; }

    public string? PharmacistObservations { get; set; }

    public string? NotesToDoctor { get; set; }

    public string? PharmacistRecommendation { get; set; }

    public string? PharmaceuticalService { get; set; }

    public string? DoctorStamp { get; set; }

    public string? CaregiverName { get; set; }

    public string? CaregiverCnp { get; set; }

    public string? PatientName { get; set; }

    public string? PatientCnp { get; set; }

    public virtual Document? Document { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } = new List<PrescriptionMedication>();
}

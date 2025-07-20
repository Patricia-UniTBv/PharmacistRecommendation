using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PrescriptionMedication
{
    public int Id { get; set; }

    public int PrescriptionId { get; set; }

    public int? MedicationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Morning { get; set; }

    public string? Noon { get; set; }

    public string? Evening { get; set; }

    public string? Night { get; set; }

    public int? AdministrationModeId { get; set; }

    public bool? IsWithPrescription { get; set; }

    public virtual AdministrationMode? AdministrationMode { get; set; }

    public virtual Medication? Medication { get; set; }

    public virtual Prescription Prescription { get; set; } = null!;
}

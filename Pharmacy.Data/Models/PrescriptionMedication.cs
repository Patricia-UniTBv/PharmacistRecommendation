using System;
using System.Collections.Generic;

namespace Pharmacy.Data.Models;

public partial class PrescriptionMedication
{
    public int Id { get; set; }

    public int PrescriptionId { get; set; }

    public int MedicationId { get; set; }

    public string? Dosage { get; set; }

    public int Quantity { get; set; }

    public string? Instructions { get; set; }

    public virtual Medication Medication { get; set; } = null!;

    public virtual Prescription Prescription { get; set; } = null!;
}
